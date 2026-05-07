using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml;

namespace Vullnerability.db
{
    // Читает vullist.xlsx БДУ ФСТЭК и льёт в SQLite-БД батчами:
    // все INSERT'ы прогоняем в одной транзакции, временные id для связей — отрицательные.
    public class ExcelImporter
    {
        public class ImportStatistics
        {
            public int AddedVulns;
            public int SkippedVulns;
        }

        private const int HeaderRow = 3;
        private const int DataStartRow = 4;

        private readonly string _connStr;

        public ExcelImporter(string connectionString)
        {
            _connStr = connectionString;
        }

        public ImportStatistics ImportFromExcel(string path)
        {
            var stats = new ImportStatistics();

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                // в vullist.xlsx 2 листа: «Уязвимости» и «Компоненты» (по строке на каждый компонент).
                // если BDU есть в «Компонентах», берём продукты оттуда и в листе 1 их разбор пропускаем
                ExcelWorksheet sheetVulns = FindSheet(package, "Уязвимости")
                                          ?? package.Workbook.Worksheets.FirstOrDefault();
                ExcelWorksheet sheetComps = FindSheet(package, "Компоненты");

                var componentBdus = sheetComps != null && sheetComps != sheetVulns
                    ? PrescanComponentsBdus(sheetComps)
                    : new HashSet<string>();

                if (sheetVulns != null && sheetVulns.Dimension != null)
                    ImportWorksheet(sheetVulns, stats, componentBdus);

                if (sheetComps != null && sheetComps != sheetVulns && sheetComps.Dimension != null)
                    ImportComponentsSheet(sheetComps);
            }

            return stats;
        }

        // ищем лист по имени, регистр и лишние пробелы игнорируем
        private static ExcelWorksheet FindSheet(ExcelPackage pkg, string name)
        {
            string target = NormHeader(name);
            return pkg.Workbook.Worksheets.FirstOrDefault(
                ws => NormHeader(ws.Name) == target);
        }

        // пробегаемся по «Компонентам» и собираем BDU-коды, которые там есть
        private HashSet<string> PrescanComponentsBdus(ExcelWorksheet ws)
        {
            var set = new HashSet<string>();
            int rowCount = ws.Dimension.End.Row;
            for (int row = DataStartRow; row <= rowCount; row++)
            {
                string bdu = GetCell(ws, row, "Идентификатор", takeLast: false);
                if (!string.IsNullOrWhiteSpace(bdu) && bdu.StartsWith("BDU:"))
                    set.Add(bdu);
            }
            return set;
        }

        // ---- Импорт листа «Уязвимости» ----
        private void ImportWorksheet(ExcelWorksheet ws, ImportStatistics stats, HashSet<string> componentBdus)
        {
            // все справочники вытягиваем в память один раз
            HashSet<string> existingBdu = LoadHashSet("SELECT bdu_code FROM vulnerabilities");
            Dictionary<string, int> vendors = LoadDict("SELECT id, name FROM vendors");
            Dictionary<string, int> productTypes = LoadDict("SELECT id, name FROM product_types");
            Dictionary<string, int> osPlatforms = LoadDict("SELECT id, name FROM os_platforms");
            Dictionary<string, int> vulnClasses = LoadDict("SELECT id, name FROM vuln_classes");
            Dictionary<string, int> severities = LoadDict("SELECT id, name FROM severity_levels");
            Dictionary<string, int> statuses = LoadDict("SELECT id, name FROM vuln_statuses");
            Dictionary<string, int> states = LoadDict("SELECT id, name FROM vuln_states");
            Dictionary<string, int> exploits = LoadDict("SELECT id, name FROM exploit_availabilities");
            Dictionary<string, int> exMethods = LoadDict("SELECT id, name FROM exploitation_methods");
            Dictionary<string, int> fixMethods = LoadDict("SELECT id, name FROM fix_methods");
            Dictionary<string, int> incidents = LoadDict("SELECT id, name FROM incident_relations");
            Dictionary<string, int> cwes = LoadDict("SELECT id, code FROM cwes");

            Dictionary<(int, string), int> products = LoadProductDict();
            Dictionary<(int, int), bool> prodTypeRel = LoadProductTypeRels();

            // буферы, куда накапливаем всё перед INSERT'ом
            var dtVulns = CreateVulnsTable();
            var dtVulnProds = CreateVulnProdsTable();
            var dtLinks = CreateLinksTable();
            var dtExtIds = CreateExtIdsTable();
            var dtMitigations = CreateMitigationsTable();
            var dtTesting = CreateTestingUpdatesTable();
            var dtVulnCwes = CreateVulnCwesTable();

            var newProductTypeRels = new List<(int productId, int typeId)>();
            var bduCodesOrder = new List<string>();

            int rowCount = ws.Dimension.End.Row;
            for (int row = DataStartRow; row <= rowCount; row++)
            {
                string bdu = GetCell(ws, row, "Идентификатор", takeLast: false);
                if (string.IsNullOrWhiteSpace(bdu) || !bdu.StartsWith("BDU:")) continue;

                if (existingBdu.Contains(bdu))
                {
                    stats.SkippedVulns++;
                    continue;
                }
                existingBdu.Add(bdu);

                // временный отрицательный id, потом будем менять на реальный
                int tempVulnKey = -(bduCodesOrder.Count + 1);
                bduCodesOrder.Add(bdu);

                bool skipProductsParse = componentBdus != null && componentBdus.Contains(bdu);

                ProcessRow(ws, row, tempVulnKey, bdu,
                    vendors, products, productTypes, osPlatforms,
                    vulnClasses, severities, statuses, states,
                    exploits, exMethods, fixMethods, incidents, cwes,
                    dtVulns, dtVulnProds, dtLinks, dtExtIds,
                    dtMitigations, dtTesting, dtVulnCwes,
                    newProductTypeRels, prodTypeRel,
                    skipProductsParse);

                stats.AddedVulns++;
            }

            if (dtVulns.Rows.Count == 0) return;

            BulkLoad(dtVulns, dtVulnProds, dtLinks, dtExtIds, dtMitigations, dtTesting, dtVulnCwes,
                     bduCodesOrder, newProductTypeRels);
        }

        // ---- Импорт листа «Компоненты» ----
        // здесь одна строка = один компонент; берём оттуда вендора/продукт/версию/платформу
        // и дописываем в vulnerability_products. Сама уязвимость уже в БД.
        private void ImportComponentsSheet(ExcelWorksheet ws)
        {
            // bdu_code → реальный id из БД
            var bduIdMap = LoadBduIdMap();

            Dictionary<string, int> vendors = LoadDict("SELECT id, name FROM vendors");
            Dictionary<string, int> productTypes = LoadDict("SELECT id, name FROM product_types");
            Dictionary<string, int> osPlatforms = LoadDict("SELECT id, name FROM os_platforms");
            Dictionary<(int, string), int> products = LoadProductDict();
            Dictionary<(int, int), bool> prodTypeRel = LoadProductTypeRels();

            // накапливаем результат в DataTable, дубли режем через HashSet по четвёрке ключей
            var dtVulnProds = CreateVulnProdsTable();
            var seenVulnProd = new HashSet<(int, int, string, int)>();
            var newProductTypeRels = new List<(int productId, int typeId)>();

            int rowCount = ws.Dimension.End.Row;
            for (int row = DataStartRow; row <= rowCount; row++)
            {
                string bdu = GetCell(ws, row, "Идентификатор", takeLast: false);
                if (string.IsNullOrWhiteSpace(bdu) || !bdu.StartsWith("BDU:")) continue;
                // если уязвимости с таким кодом в БД нет — строку пропускаем
                if (!bduIdMap.TryGetValue(bdu, out int vulnId)) continue;

                string vendorVal = GetCell(ws, row, "Вендор ПО");
                string productVal = GetCell(ws, row, "Название ПО", aliases: new[] { "Наименование ПО" });
                string versionVal = GetCell(ws, row, "Версия ПО");
                string platformVal = GetCell(ws, row, "Наименование ОС и тип аппаратной платформы");
                string typeVal = GetCell(ws, row, "Тип ПО");

                if (string.IsNullOrWhiteSpace(productVal)) continue;

                int? vendorId = !string.IsNullOrWhiteSpace(vendorVal)
                    ? GetOrAdd(vendors, "vendors", "name", vendorVal.Trim(), maxLen: 255)
                    : null;

                int productId = GetOrAddProduct(products, vendorId, productVal.Trim());

                int? platformId = !string.IsNullOrWhiteSpace(platformVal)
                    ? GetOrAdd(osPlatforms, "os_platforms", "name", platformVal.Trim(), maxLen: 500)
                    : null;

                // «Тип ПО» периодически идёт через запятую — разламываем и берём все
                foreach (var t in SplitMulti(typeVal))
                {
                    int? typeId = GetOrAdd(productTypes, "product_types", "name", t, maxLen: 255);
                    if (typeId.HasValue)
                    {
                        var key = (productId, typeId.Value);
                        if (!prodTypeRel.ContainsKey(key))
                        {
                            prodTypeRel[key] = true;
                            newProductTypeRels.Add((productId, typeId.Value));
                        }
                    }
                }

                string ver = Trunc(versionVal, 255);
                int platKey = platformId ?? 0;
                if (!seenVulnProd.Add((vulnId, productId, ver ?? "", platKey))) continue;

                dtVulnProds.Rows.Add(
                    vulnId,
                    productId,
                    (object)ver ?? DBNull.Value,
                    (object)platformId ?? DBNull.Value);
            }

            if (dtVulnProds.Rows.Count == 0 && newProductTypeRels.Count == 0) return;

            using (var conn = new SQLiteConnection(_connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        if (dtVulnProds.Rows.Count > 0)
                            DoBulkCopy(conn, tx, "vulnerability_products", dtVulnProds);

                        foreach (var rel in newProductTypeRels)
                        {
                            using (var cmd = new SQLiteCommand(
                                "INSERT OR IGNORE INTO product_product_types(product_id, product_type_id) VALUES(@p, @t);",
                                conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@p", rel.productId);
                                cmd.Parameters.AddWithValue("@t", rel.typeId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // bdu_code → id из таблицы vulnerabilities
        private Dictionary<string, int> LoadBduIdMap()
        {
            var map = new Dictionary<string, int>();
            using (var conn = new SQLiteConnection(_connStr))
            using (var cmd = new SQLiteCommand("SELECT id, bdu_code FROM vulnerabilities", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        map[rd.GetString(1)] = rd.GetInt32(0);
            }
            return map;
        }

        // ---- Обработка одной строки листа «Уязвимости» ----
        private void ProcessRow(ExcelWorksheet ws, int row, int tempVulnKey, string bduCode,
            Dictionary<string, int> vendors,
            Dictionary<(int, string), int> products,
            Dictionary<string, int> productTypes,
            Dictionary<string, int> osPlatforms,
            Dictionary<string, int> vulnClasses,
            Dictionary<string, int> severities,
            Dictionary<string, int> statuses,
            Dictionary<string, int> states,
            Dictionary<string, int> exploits,
            Dictionary<string, int> exMethods,
            Dictionary<string, int> fixMethods,
            Dictionary<string, int> incidents,
            Dictionary<string, int> cwes,
            DataTable dtVulns, DataTable dtVulnProds, DataTable dtLinks,
            DataTable dtExtIds, DataTable dtMitigations, DataTable dtTesting,
            DataTable dtVulnCwes,
            List<(int productId, int typeId)> newProductTypeRels,
            Dictionary<(int, int), bool> prodTypeRel,
            bool skipProductsParse)
        {
            // ---- CVSS ----
            var (v2vec, v2sc) = ParseCvss(GetCell(ws, row, "CVSS 2.0"));
            var (v3vec, v3sc) = ParseCvss(GetCell(ws, row, "CVSS 3.0"));
            var (v4vec, v4sc) = ParseCvss(GetCell(ws, row, "CVSS 4.0"));

            // в BDU «Уровень опасности» бывает слитым из нескольких фраз по CVSS,
            // разбиваем на строки и храним полным текстом, а severity_level_id берём из первой фразы
            string severityRaw = NormalizeSeverityText(GetCell(ws, row, "Уровень опасности уязвимости"));
            string sevName = ExtractSeverity(severityRaw);
            int? severityId = sevName != null && severities.TryGetValue(sevName, out var sid) ? (int?)sid : null;

            // в этих полях Excel иногда несколько значений через запятую, поэтому берём первое
            int? vulnClassId = GetOrAdd(vulnClasses, "vuln_classes", "name", FirstOf(GetCell(ws, row, "Класс уязвимости")), maxLen: 255);
            int? statusId = GetOrAdd(statuses, "vuln_statuses", "name", FirstOf(GetCell(ws, row, "Статус уязвимости")), maxLen: 128);
            int? stateId = GetOrAdd(states, "vuln_states", "name", FirstOf(GetCell(ws, row, "Состояние уязвимости")), maxLen: 64);
            int? exploitId = GetOrAdd(exploits, "exploit_availabilities", "name", FirstOf(GetCell(ws, row, "Наличие эксплойта")), maxLen: 128);
            int? exMethodId = GetOrAdd(exMethods, "exploitation_methods", "name", FirstOf(GetCell(ws, row, "Способ эксплуатации")), maxLen: 255);
            int? fixMethodId = GetOrAdd(fixMethods, "fix_methods", "name", FirstOf(GetCell(ws, row, "Способ устранения")), maxLen: 255);
            int? incidentId = GetOrAdd(incidents, "incident_relations", "name", FirstOf(GetCell(ws, row, "Связь с инцидентами ИБ")), maxLen: 64);

            // CWE бывает несколько в одной ячейке: «… (CWE-122), … (CWE-416)» — основные кладём
            // в vulnerability_cwes, а «первый» в vulnerabilities.cwe_id (для старого фильтра Form1)
            string cweCodesRaw = GetCell(ws, row, "Тип ошибки CWE");
            string cweDescRaw = GetCell(ws, row, "Описание ошибки CWE");
            var cweCodeList = ExtractCweCodes(cweCodesRaw);
            // в «Тип ошибки» кодов нет — пытаемся из «Описания»
            if (cweCodeList.Count == 0) cweCodeList = ExtractCweCodes(cweDescRaw);

            int? cweId = null;
            var seenCwesForVuln = new HashSet<int>();
            for (int k = 0; k < cweCodeList.Count; k++)
            {
                string code = cweCodeList[k];
                int id = GetOrAddCwe(cwes, code, k == 0 ? cweDescRaw : null);
                if (cweId == null) cweId = id;
                if (seenCwesForVuln.Add(id))
                {
                    dtVulnCwes.Rows.Add(tempVulnKey, id);
                }
            }

            // сама запись уязвимости
            dtVulns.Rows.Add(
                Trunc(bduCode, 32),
                GetCell(ws, row, "Наименование уязвимости") ?? bduCode,
                GetCell(ws, row, "Описание уязвимости"),
                ParseDate(GetCell(ws, row, "Дата выявления")),
                ParseDate(GetCell(ws, row, "Дата публикации")),
                ParseDate(GetCell(ws, row, "Дата последнего обновления")),
                Trunc(v2vec, 255), v2sc,
                Trunc(v3vec, 255), v3sc,
                Trunc(v4vec, 255), v4sc,
                GetCell(ws, row, "Информация об устранении"),
                GetCell(ws, row, "Прочая информация"),
                GetCell(ws, row, "Последствия эксплуатации уязвимости"),
                (object)severityRaw ?? DBNull.Value,
                vulnClassId, severityId, statusId, stateId,
                exploitId, exMethodId, fixMethodId, incidentId, cweId);

            // продукты: если BDU есть в «Компонентах» — пропускаем, их добавит ImportComponentsSheet
            if (!skipProductsParse)
            {
                string vendorVal = GetCell(ws, row, "Вендор ПО");
                string productVal = GetCell(ws, row, "Название ПО", aliases: new[] { "Наименование ПО" });
                string versionVal = GetCell(ws, row, "Версия ПО");
                string platformVal = GetCell(ws, row, "Наименование ОС и тип аппаратной платформы");
                string typeVal = GetCell(ws, row, "Тип ПО");

                // в BDU эти поля — «параллельные» списки (i-й продукт идёт с i-й версией), а НЕ декартово произведение,
                // иначе на больших записях выбивается OutOfMemory. Берём N = max(длин) и идём по кругу.
                const int MAX_ROWS_PER_VULN = 2000;

                var platformIdsList = SplitMulti(platformVal)
                    .Select(p => GetOrAdd(osPlatforms, "os_platforms", "name", p, maxLen: 500))
                    .Where(id => id.HasValue)
                    .Select(id => (int?)id.Value)
                    .ToList();

                var typeIds = SplitMulti(typeVal)
                    .Select(t => GetOrAdd(productTypes, "product_types", "name", t, maxLen: 255))
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                var vendorList = SplitMulti(vendorVal).ToList();
                var productList = SplitMulti(productVal).ToList();
                var versionList = SplitMulti(versionVal).ToList();

                int n = Math.Max(1, Math.Max(Math.Max(productList.Count, versionList.Count), platformIdsList.Count));
                if (n > MAX_ROWS_PER_VULN) n = MAX_ROWS_PER_VULN;

                var seenProductsForTypes = new HashSet<int>();

                for (int i = 0; i < n; i++)
                {
                    string vName = vendorList.Count > 0 ? vendorList[i % vendorList.Count] : null;
                    string pName = productList.Count > 0 ? productList[i % productList.Count] : null;
                    string ver = versionList.Count > 0 ? versionList[i % versionList.Count] : null;
                    int? pid = platformIdsList.Count > 0 ? platformIdsList[i % platformIdsList.Count] : null;

                    int? vendorId = !string.IsNullOrWhiteSpace(vName)
                        ? GetOrAdd(vendors, "vendors", "name", vName.Trim(), maxLen: 255)
                        : null;

                    if (string.IsNullOrWhiteSpace(pName)) continue;

                    int productId = GetOrAddProduct(products, vendorId, pName.Trim());

                    if (seenProductsForTypes.Add(productId))
                    {
                        foreach (var typeId in typeIds)
                        {
                            var key = (productId, typeId);
                            if (!prodTypeRel.ContainsKey(key))
                            {
                                prodTypeRel[key] = true;
                                newProductTypeRels.Add((productId, typeId));
                            }
                        }
                    }

                    dtVulnProds.Rows.Add(
                        tempVulnKey,
                        productId,
                        (object)Trunc(ver, 255) ?? DBNull.Value,
                        (object)pid ?? DBNull.Value);
                }
            }

            // ссылки на источники (1:N)
            string refs = GetCell(ws, row, "Ссылки на источники") ?? GetCell(ws, row, "Ссылки");
            foreach (string url in SplitMultiLines(refs))
            {
                if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    dtLinks.Rows.Add(tempVulnKey, Trunc(url, 2000));
            }

            // внешние ID: CVE, GHSA, MS и т. п.
            string extRaw = GetCell(ws, row, "Идентификаторы других систем описаний уязвимости");
            foreach (var pair in ParseExternalIds(extRaw))
            {
                dtExtIds.Rows.Add(tempVulnKey,
                    Trunc(pair.Item2, 128),
                    (object)Trunc(pair.Item1, 32) ?? DBNull.Value);
            }

            // в старом формате эти ID были в отдельных колонках — проверяем и их
            foreach (var src in new[] { "CVE", "OSVDB ID", "Bugtraq ID", "ISS X-Force ID", "Exploit Database ID" })
            {
                string raw = GetCell(ws, row, src);
                foreach (string code in SplitMulti(raw))
                    dtExtIds.Rows.Add(tempVulnKey, Trunc(code, 128), Trunc(src, 32));
            }

            // меры по устранению (1:N)
            string mitigationRaw = GetCell(ws, row, "Возможные меры по устранению");
            foreach (string m in SplitMultiLines(mitigationRaw))
            {
                dtMitigations.Rows.Add(tempVulnKey, m);
            }

            // инфо о проведённых испытаниях (последние две колонки «Идентификатор» и «Наименование»)
            string testingId = GetCell(ws, row, "Идентификатор", takeLast: true);
            string testingName = GetCell(ws, row, "Наименование", takeLast: true);
            // если testingId — тот же BDU-код, значит «Наименование» было одно и попало в первую колонку
            if (!string.IsNullOrWhiteSpace(testingId) && testingId != bduCode)
            {
                dtTesting.Rows.Add(tempVulnKey,
                    (object)Trunc(testingId, 255) ?? DBNull.Value,
                    (object)Trunc(testingName, 500) ?? DBNull.Value);
            }
            else if (!string.IsNullOrWhiteSpace(testingName))
            {
                dtTesting.Rows.Add(tempVulnKey, DBNull.Value, Trunc(testingName, 500));
            }
        }

        // усекаем строку до maxLen, null/пустую возвращаем null
        private static string Trunc(string s, int maxLen)
        {
            if (s == null) return null;
            s = s.Trim();
            if (s.Length == 0) return null;
            return s.Length > maxLen ? s.Substring(0, maxLen) : s;
        }

        // ---- Bulk INSERT всех буферов в одной транзакции ----
        private void BulkLoad(DataTable dtVulns, DataTable dtVulnProds, DataTable dtLinks,
                              DataTable dtExtIds, DataTable dtMitigations, DataTable dtTesting,
                              DataTable dtVulnCwes,
                              List<string> bduOrder,
                              List<(int productId, int typeId)> newProductTypeRels)
        {
            using (var conn = new SQLiteConnection(_connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) сначала сами уязвимости
                        DoBulkCopy(conn, tx, "vulnerabilities", dtVulns);

                        // 2) вытягиваем реальные id по bdu_code (батчами, иначе IN(…) слишком длинный)
                        var idMap = new Dictionary<string, int>();
                        const int batch = 1000;
                        for (int i = 0; i < bduOrder.Count; i += batch)
                        {
                            var part = bduOrder.Skip(i).Take(batch).ToList();
                            using (var cmd = new SQLiteCommand(
                                "SELECT id, bdu_code FROM vulnerabilities WHERE bdu_code IN (" +
                                string.Join(",", part.Select((_, j) => $"@p{j}")) + ")", conn, tx))
                            {
                                for (int j = 0; j < part.Count; j++)
                                    cmd.Parameters.AddWithValue($"@p{j}", part[j]);
                                using (var rd = cmd.ExecuteReader())
                                    while (rd.Read())
                                        idMap[rd.GetString(1)] = rd.GetInt32(0);
                            }
                        }

                        // 3) в буферных DataTable меняем временные ключи на реальные id
                        ReplaceTempKey(dtVulnProds, "vulnerability_id", bduOrder, idMap);
                        ReplaceTempKey(dtLinks, "vulnerability_id", bduOrder, idMap);
                        ReplaceTempKey(dtExtIds, "vulnerability_id", bduOrder, idMap);
                        ReplaceTempKey(dtMitigations, "vulnerability_id", bduOrder, idMap);
                        ReplaceTempKey(dtTesting, "vulnerability_id", bduOrder, idMap);
                        ReplaceTempKey(dtVulnCwes, "vulnerability_id", bduOrder, idMap);

                        // 4) теперь льём дочерние таблицы
                        DoBulkCopy(conn, tx, "vulnerability_products", dtVulnProds);
                        DoBulkCopy(conn, tx, "vulnerability_source_links", dtLinks);
                        DoBulkCopy(conn, tx, "vulnerability_external_ids", dtExtIds);
                        DoBulkCopy(conn, tx, "vulnerability_mitigations", dtMitigations);
                        DoBulkCopy(conn, tx, "vulnerability_testing_updates", dtTesting);
                        DoBulkCopy(conn, tx, "vulnerability_cwes", dtVulnCwes);

                        // 5) связь product ↔ product_type — INSERT OR IGNORE, чтобы существующие пары не дублировать
                        // PK = (product_id, product_type_id), дубли игнорируем через INSERT OR IGNORE
                        if (newProductTypeRels.Count > 0)
                        {
                            foreach (var rel in newProductTypeRels)
                            {
                                using (var cmd = new SQLiteCommand(
                                    "INSERT OR IGNORE INTO product_product_types(product_id, product_type_id) VALUES(@p, @t);",
                                    conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@p", rel.productId);
                                    cmd.Parameters.AddWithValue("@t", rel.typeId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void ReplaceTempKey(DataTable dt, string column, List<string> bduOrder, Dictionary<string, int> idMap)
        {
            if (dt.Rows.Count == 0) return;
            int idx = dt.Columns.IndexOf(column);
            foreach (DataRow r in dt.Rows)
            {
                int tempKey = Convert.ToInt32(r[idx]);
                if (tempKey >= 0) continue;
                int orderIndex = -tempKey - 1;
                if (orderIndex >= 0 && orderIndex < bduOrder.Count
                    && idMap.TryGetValue(bduOrder[orderIndex], out int realId))
                    r[idx] = realId;
            }
        }

        // аналог SqlBulkCopy для SQLite: один подготовленный INSERT и все строки в транзакции
        private static void DoBulkCopy(SQLiteConnection conn, SQLiteTransaction tx, string table, DataTable dt)
        {
            if (dt.Rows.Count == 0) return;

            var cols = new DataColumn[dt.Columns.Count];
            dt.Columns.CopyTo(cols, 0);

            string colList = string.Join(",", cols.Select(c => c.ColumnName));
            string paramList = string.Join(",", cols.Select((c, i) => "@p" + i));

            using (var cmd = new SQLiteCommand($"INSERT INTO {table}({colList}) VALUES({paramList});", conn, tx))
            {
                // Заранее создаём параметры (без значений) — переиспользуем на каждой строке.
                var pars = new SQLiteParameter[cols.Length];
                for (int i = 0; i < cols.Length; i++)
                    pars[i] = cmd.Parameters.Add("@p" + i, MapDbType(cols[i].DataType));

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < cols.Length; i++)
                    {
                        object v = row[i];
                        pars[i].Value = (v == null || v == DBNull.Value) ? DBNull.Value : v;
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // CLR-тип в DbType для параметров SQLite (важно для DateTime/decimal)
        private static DbType MapDbType(Type t)
        {
            if (t == typeof(int) || t == typeof(int?)) return DbType.Int32;
            if (t == typeof(long) || t == typeof(long?)) return DbType.Int64;
            if (t == typeof(string)) return DbType.String;
            if (t == typeof(bool) || t == typeof(bool?)) return DbType.Boolean;
            if (t == typeof(DateTime) || t == typeof(DateTime?)) return DbType.DateTime;
            if (t == typeof(decimal) || t == typeof(decimal?)) return DbType.Decimal;
            if (t == typeof(double) || t == typeof(double?)) return DbType.Double;
            if (t == typeof(float) || t == typeof(float?)) return DbType.Single;
            if (t == typeof(byte[])) return DbType.Binary;
            return DbType.Object;
        }

        // ---- Буферные DataTable, один к одному под колонки в БД ----
        private static DataTable CreateVulnsTable()
        {
            var dt = new DataTable("vulnerabilities");
            dt.Columns.Add("bdu_code", typeof(string));
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("description", typeof(string));
            dt.Columns.Add("discovery_date", typeof(DateTime));
            dt.Columns.Add("publication_date", typeof(DateTime));
            dt.Columns.Add("last_update_date", typeof(DateTime));
            dt.Columns.Add("cvss_2_0_vector", typeof(string));
            dt.Columns.Add("cvss_2_0_score", typeof(decimal));
            dt.Columns.Add("cvss_3_0_vector", typeof(string));
            dt.Columns.Add("cvss_3_0_score", typeof(decimal));
            dt.Columns.Add("cvss_4_0_vector", typeof(string));
            dt.Columns.Add("cvss_4_0_score", typeof(decimal));
            dt.Columns.Add("fix_info", typeof(string));
            dt.Columns.Add("other_info", typeof(string));
            dt.Columns.Add("exploitation_consequences", typeof(string));
            dt.Columns.Add("severity_text", typeof(string));
            dt.Columns.Add("vuln_class_id", typeof(int));
            dt.Columns.Add("severity_level_id", typeof(int));
            dt.Columns.Add("status_id", typeof(int));
            dt.Columns.Add("state_id", typeof(int));
            dt.Columns.Add("exploit_availability_id", typeof(int));
            dt.Columns.Add("exploitation_method_id", typeof(int));
            dt.Columns.Add("fix_method_id", typeof(int));
            dt.Columns.Add("incident_relation_id", typeof(int));
            dt.Columns.Add("cwe_id", typeof(int));
            foreach (DataColumn c in dt.Columns) c.AllowDBNull = true;
            dt.Columns["bdu_code"].AllowDBNull = false;
            return dt;
        }

        private static DataTable CreateVulnProdsTable()
        {
            var dt = new DataTable("vulnerability_products");
            dt.Columns.Add("vulnerability_id", typeof(int));
            dt.Columns.Add("product_id", typeof(int));
            dt.Columns.Add("product_version", typeof(string));
            dt.Columns.Add("os_platform_id", typeof(int));
            dt.Columns["product_version"].AllowDBNull = true;
            dt.Columns["os_platform_id"].AllowDBNull = true;
            return dt;
        }

        private static DataTable CreateLinksTable()
        {
            var dt = new DataTable("vulnerability_source_links");
            dt.Columns.Add("vulnerability_id", typeof(int));
            dt.Columns.Add("url", typeof(string));
            return dt;
        }

        private static DataTable CreateExtIdsTable()
        {
            var dt = new DataTable("vulnerability_external_ids");
            dt.Columns.Add("vulnerability_id", typeof(int));
            dt.Columns.Add("external_id", typeof(string));
            dt.Columns.Add("source", typeof(string));
            dt.Columns["source"].AllowDBNull = true;
            return dt;
        }

        private static DataTable CreateMitigationsTable()
        {
            var dt = new DataTable("vulnerability_mitigations");
            dt.Columns.Add("vulnerability_id", typeof(int));
            dt.Columns.Add("measure", typeof(string));
            return dt;
        }

        private static DataTable CreateTestingUpdatesTable()
        {
            var dt = new DataTable("vulnerability_testing_updates");
            dt.Columns.Add("vulnerability_id", typeof(int));
            dt.Columns.Add("update_identifier", typeof(string));
            dt.Columns.Add("update_name", typeof(string));
            dt.Columns["update_identifier"].AllowDBNull = true;
            dt.Columns["update_name"].AllowDBNull = true;
            return dt;
        }

        private static DataTable CreateVulnCwesTable()
        {
            var dt = new DataTable("vulnerability_cwes");
            dt.Columns.Add("vulnerability_id", typeof(int));
            dt.Columns.Add("cwe_id", typeof(int));
            return dt;
        }

        // ---- Парсеры ----
        // выдёргиваем из строки CVSS-вектор и число (балл)
        private static (string vector, decimal? score) ParseCvss(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return (null, null);
            var match = Regex.Match(raw, @"(\d+(?:[.,]\d+)?)");
            decimal? score = null;
            if (match.Success && decimal.TryParse(match.Value.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var s))
                score = s;
            return (raw.Trim(), score);
        }

        private static string ExtractSeverity(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var lo = raw.ToLowerInvariant();
            if (lo.Contains("крит")) return "Критический";
            if (lo.Contains("высок")) return "Высокий";
            if (lo.Contains("сред")) return "Средний";
            if (lo.Contains("низк")) return "Низкий";
            if (lo.Contains("информ")) return "Информационный";
            return null;
        }

        // в BDU «Уровень опасности» бывает слитым вроде «… составляет N)Высокий уровень…» —
        // перед каждым «<уровень> уровень опасности» вставляем перенос строки
        private static string NormalizeSeverityText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            string s = raw.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

            s = Regex.Replace(
                s,
                @"(?<!^)\s*(?=(?:Критический|Высокий|Средний|Низкий|Информационный|Базовый)\s+уровень\s+опасности)",
                "\n");

            // два+ переноса подряд схлопываем в один
            s = Regex.Replace(s, @"\n{2,}", "\n");
            return s.Trim();
        }

        // вытаскиваем все CWE-коды из строки, окружение любое — важен только сам «CWE-NNN»
        private static List<string> ExtractCweCodes(string raw)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return result;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in Regex.Matches(raw, @"CWE-\d+", RegexOptions.IgnoreCase))
            {
                string code = m.Value.ToUpperInvariant();
                if (seen.Add(code)) result.Add(code);
            }
            return result;
        }

        private static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return DateTime.TryParse(s, CultureInfo.GetCultureInfo("ru-RU"),
                DateTimeStyles.None, out var d) ? d
                : (DateTime.TryParse(s, out d) ? d : (DateTime?)null);
        }

        private static IEnumerable<string> SplitMulti(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return Enumerable.Empty<string>();
            return val.Split(new[] { ';', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim()).Where(s => s.Length > 0);
        }

        private static IEnumerable<string> SplitMultiLines(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return Enumerable.Empty<string>();
            return val.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim()).Where(s => s.Length > 0);
        }

        // разбираем блоки вида «CVE: CVE-2024-1234, CVE-2024-5678\nGHSA: …» → (источник, внешний_id)
        private static IEnumerable<(string source, string externalId)> ParseExternalIds(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) yield break;
            foreach (var line in raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                string source = null;
                string body = trimmed;
                int colon = trimmed.IndexOf(':');
                if (colon > 0 && colon < 16)
                {
                    string maybeSrc = trimmed.Substring(0, colon).Trim();
                    if (!maybeSrc.Contains(" ") && !maybeSrc.StartsWith("http"))
                    {
                        source = maybeSrc;
                        body = trimmed.Substring(colon + 1).Trim();
                    }
                }
                foreach (var code in body.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string c = code.Trim();
                    if (c.Length > 0) yield return (source, c.Length > 128 ? c.Substring(0, 128) : c);
                }
            }
        }

        // ---- Кэши справочников и GET-OR-ADD ----
        private HashSet<string> LoadHashSet(string sql)
        {
            var set = new HashSet<string>();
            using (var conn = new SQLiteConnection(_connStr))
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) set.Add(rd.GetString(0));
            }
            return set;
        }

        private Dictionary<string, int> LoadDict(string sql)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (var conn = new SQLiteConnection(_connStr))
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                    {
                        int id = rd.GetInt32(0);
                        string n = rd.GetString(1);
                        if (!dict.ContainsKey(n)) dict[n] = id;
                    }
            }
            return dict;
        }

        private Dictionary<(int, string), int> LoadProductDict()
        {
            var dict = new Dictionary<(int, string), int>();
            using (var conn = new SQLiteConnection(_connStr))
            // в SQLite вместо ISNULL пишется IFNULL/COALESCE
            using (var cmd = new SQLiteCommand("SELECT id, IFNULL(vendor_id,0), name FROM products", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        dict[(rd.GetInt32(1), rd.GetString(2))] = rd.GetInt32(0);
            }
            return dict;
        }

        private Dictionary<(int, int), bool> LoadProductTypeRels()
        {
            var dict = new Dictionary<(int, int), bool>();
            using (var conn = new SQLiteConnection(_connStr))
            using (var cmd = new SQLiteCommand("SELECT product_id, product_type_id FROM product_product_types", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        dict[(rd.GetInt32(0), rd.GetInt32(1))] = true;
            }
            return dict;
        }

        private int? GetOrAdd(Dictionary<string, int> cache, string table, string col, string value, int maxLen = 255)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            if (value.Length > maxLen) value = value.Substring(0, maxLen);
            if (cache.TryGetValue(value, out int id)) return id;

            using (var conn = new SQLiteConnection(_connStr))
            {
                conn.Open();
                // INSERT OR IGNORE опирается на UNIQUE по колонке-имени
                using (var cmd = new SQLiteCommand(
                    $"INSERT OR IGNORE INTO {table}({col}) VALUES(@v);", conn))
                {
                    cmd.Parameters.AddWithValue("@v", value);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SQLiteCommand(
                    $"SELECT id FROM {table} WHERE {col}=@v;", conn))
                {
                    cmd.Parameters.AddWithValue("@v", value);
                    id = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            cache[value] = id;
            return id;
        }

        // берём первое непустое из списка через ;,\n\r
        private static string FirstOf(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return raw.Split(new[] { ';', ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim())
                      .FirstOrDefault(s => s.Length > 0);
        }

        private int GetOrAddProduct(Dictionary<(int, string), int> cache, int? vendorId, string name)
        {
            if (name.Length > 500) name = name.Substring(0, 500);
            int vKey = vendorId ?? 0;
            var key = (vKey, name);
            if (cache.TryGetValue(key, out int id)) return id;

            using (var conn = new SQLiteConnection(_connStr))
            {
                conn.Open();
                // SQLite в UNIQUE(vendor_id, name) считает NULL!=NULL, поэтому дубли с NULL-вендором
                // ловим вручную через NOT EXISTS + IFNULL(.,0)=IFNULL(.,0)
                using (var cmd = new SQLiteCommand(
                    @"INSERT INTO products(name, vendor_id)
                      SELECT @n, @v
                      WHERE NOT EXISTS (SELECT 1 FROM products
                                         WHERE name=@n AND IFNULL(vendor_id,0)=IFNULL(@v,0));", conn))
                {
                    cmd.Parameters.AddWithValue("@n", name);
                    cmd.Parameters.AddWithValue("@v", (object)vendorId ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SQLiteCommand(
                    @"SELECT id FROM products
                       WHERE name=@n AND IFNULL(vendor_id,0)=IFNULL(@v,0);", conn))
                {
                    cmd.Parameters.AddWithValue("@n", name);
                    cmd.Parameters.AddWithValue("@v", (object)vendorId ?? DBNull.Value);
                    id = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            cache[key] = id;
            return id;
        }

        private int GetOrAddCwe(Dictionary<string, int> cache, string code, string description)
        {
            code = code.Trim();
            if (code.Length > 32) code = code.Substring(0, 32);
            if (description != null && description.Length > 1000) description = description.Substring(0, 1000);
            if (cache.TryGetValue(code, out int id)) return id;

            using (var conn = new SQLiteConnection(_connStr))
            {
                conn.Open();
                // SQLite-UPSERT по UNIQUE-индексу cwes.code, excluded.description — новое значение из INSERT'а
                using (var cmd = new SQLiteCommand(
                    @"INSERT INTO cwes(code, description) VALUES(@c, @d)
                      ON CONFLICT(code) DO UPDATE
                         SET description = COALESCE(cwes.description, excluded.description);", conn))
                {
                    cmd.Parameters.AddWithValue("@c", code);
                    cmd.Parameters.AddWithValue("@d", (object)description ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SQLiteCommand(
                    @"SELECT id FROM cwes WHERE code=@c;", conn))
                {
                    cmd.Parameters.AddWithValue("@c", code);
                    id = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            cache[code] = id;
            return id;
        }

        // ---- Поиск ячеек Excel по имени колонки ----
        private readonly Dictionary<string, int[]> _columnCache = new Dictionary<string, int[]>();

        // takeLast=true нужен для «Идентификатор»/«Наименование» в блоке «Обновления ПО»,
        // где эти же имена колонок идут вторыми повторными в правой части листа
        private string GetCell(ExcelWorksheet ws, int row, string colName,
                               bool takeLast = false, string[] aliases = null)
        {
            int col = FindColumn(ws, colName, takeLast);
            if (col < 0 && aliases != null)
            {
                foreach (var alt in aliases)
                {
                    col = FindColumn(ws, alt, takeLast);
                    if (col >= 0) break;
                }
            }
            if (col < 0) return null;
            return ws.Cells[row, col].Value?.ToString()?.Trim();
        }

        // нормализуем имя хедера: схлопываем любые пробельные в один, регистр опускаем
        private static string NormHeader(string s)
        {
            if (s == null) return "";
            var sb = new System.Text.StringBuilder(s.Length);
            bool prevSpace = false;
            foreach (char ch in s)
            {
                char c = ch;
                if (c == '\r' || c == '\n' || c == '\t') c = ' ';
                if (char.IsWhiteSpace(c))
                {
                    if (!prevSpace && sb.Length > 0) { sb.Append(' '); prevSpace = true; }
                }
                else { sb.Append(char.ToLowerInvariant(c)); prevSpace = false; }
            }
            if (sb.Length > 0 && sb[sb.Length - 1] == ' ') sb.Length--;
            return sb.ToString();
        }

        private int FindColumn(ExcelWorksheet ws, string colName, bool takeLast)
        {
            string key = ws.Name + "|" + NormHeader(colName);
            if (!_columnCache.TryGetValue(key, out var arr))
            {
                int colCount = ws.Dimension.End.Column;
                string target = NormHeader(colName);
                var matches = new List<int>();
                for (int c = 1; c <= colCount; c++)
                {
                    string header = NormHeader(ws.Cells[HeaderRow, c].Value?.ToString());
                    if (header == target) matches.Add(c);
                }
                arr = matches.ToArray();
                _columnCache[key] = arr;
            }
            if (arr.Length == 0) return -1;
            return takeLast ? arr[arr.Length - 1] : arr[0];
        }
    }
}
