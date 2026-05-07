using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Vullnerability.db;

namespace Vullnerability
{
    public partial class VullDetailsForm : Form
    {
        private readonly VulnDbContext _db = new VulnDbContext();
        private Vulnerability _vuln;
        private readonly int _vulnId;

        // Вычисляемая ширина для word-wrap. Колонка-метка 220 + padding/scrollbar ≈ 240.
        private const int FormWidth = 1000;
        private const int LabelColumn = 220;
        private const int RightPad = 40;          // 10 padding + 17 scrollbar + 13 запас
        private int ValueColumnWidth => Math.Max(200, FormWidth - LabelColumn - RightPad);

        public class SoftwareInfo
        {
            public string Vendor { get; set; } = "";
            public string Software { get; set; } = "";
            public string Version { get; set; } = "";
            public string Platform { get; set; } = "";
            public string ProductType { get; set; } = "";
        }

        public VullDetailsForm(int vulnId)
        {
            InitializeComponent();
            _vulnId = vulnId;
            BuildTable();
            LoadData(vulnId);
        }

        private void BuildTable()
        {
            table.BackColor = Color.FromArgb(30, 30, 30);
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            table.RowCount = 0;
            table.RowStyles.Clear();

            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font(lblTitle.Font.FontFamily, 12, FontStyle.Bold);

            btnBack.BackColor = Color.FromArgb(50, 50, 55);
            btnBack.ForeColor = Color.White;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;

            this.BackColor = Color.FromArgb(45, 45, 48);
            panelTop.BackColor = Color.FromArgb(45, 45, 48);
            panelScroll.BackColor = Color.FromArgb(30, 30, 30);
        }

        // ============================================================
        // ОСНОВНАЯ ЗАГРУЗКА
        // ============================================================
        private void LoadData(int vulnId)
        {
            _vuln = _db.Vulnerabilities
                .Include(v => v.VulnClass)
                .Include(v => v.Cwe)
                .Include(v => v.Status)
                .Include(v => v.State)
                .Include(v => v.ExploitAvailability)
                .Include(v => v.ExploitationMethod)
                .Include(v => v.FixMethod)
                .Include(v => v.SeverityLevel)
                .Include(v => v.IncidentRelation)
                .Include(v => v.VulnerabilityCwes.Select(vc => vc.Cwe))
                .FirstOrDefault(v => v.Id == vulnId);

            if (_vuln == null)
            {
                MessageBox.Show("Запись не найдена", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            lblTitle.Text = $"BDU: {_vuln.BduCode}";

            // ---- Уязвимое ПО (M:N + версия + платформа + тип ПО). Грузим на клиент чтобы избежать
            // багов EF6 c outer join + chained nav через nullable FK.
            var rawProducts = _db.VulnerabilityProducts
                .Include(vp => vp.Product)
                .Include(vp => vp.Product.Vendor)
                .Include(vp => vp.Product.ProductTypes)
                .Include(vp => vp.OsPlatform)
                .Where(vp => vp.VulnerabilityId == vulnId)
                .ToList();

            // Если у продукта несколько типов ПО — в таблице это несколько строк, в каждой ровно один тип
            // (как на bdu.fstec.ru). Поэтому используем SelectMany: одна строка vulnerability_products
            // разворачивается в N строк по числу типов (минимум одну — даже если типов нет вовсе).
            var softwareData = rawProducts
                .SelectMany(vp =>
                {
                    string vendor = vp.Product?.Vendor?.Name ?? "";
                    string software = StripVendorPrefix(vp.Product?.Name, vp.Product?.Vendor?.Name);
                    string version = ExtractVersionOnly(vp.ProductVersion);
                    // Полная строка осталась в Platform (нужна для отдельного блока «Операционные системы
                    // и аппаратные платформы»). В колонке «Архитектура» выводим только её хвост — см. AddSoftwareRow.
                    string platform = vp.OsPlatform?.Name ?? "";

                    var types = vp.Product?.ProductTypes?
                                .Select(t => t.Name)
                                .Where(n => !string.IsNullOrWhiteSpace(n))
                                .Distinct()
                                .OrderBy(n => n)
                                .ToList() ?? new List<string>();

                    if (types.Count == 0)
                    {
                        // Нет ни одного типа — всё равно выводим строку (с пустым «Тип ПО»),
                        // чтобы не потерять упоминание продукта.
                        return new[]
                        {
                            new SoftwareInfo
                            {
                                Vendor = vendor, Software = software, Version = version,
                                Platform = platform, ProductType = ""
                            }
                        };
                    }

                    return types.Select(t => new SoftwareInfo
                    {
                        Vendor = vendor,
                        Software = software,
                        Version = version,
                        Platform = platform,
                        ProductType = t
                    });
                })
                .GroupBy(s => $"{s.Vendor}|{s.Software}|{s.Version}|{s.Platform}|{s.ProductType}")
                .Select(g => g.First())
                .OrderBy(s => s.Vendor).ThenBy(s => s.Software)
                                       .ThenBy(s => s.Version)
                                       .ThenBy(s => s.ProductType)
                .ToList();

            // ---- Ссылки 1:N ----
            var links = _db.VulnerabilitySourceLinks
                .Where(r => r.VulnerabilityId == vulnId)
                .Select(r => r.Url)
                .ToList();

            // ---- Внешние идентификаторы 1:N ----
            var extIds = _db.VulnerabilityExternalIds
                .Where(e => e.VulnerabilityId == vulnId)
                .Select(e => new { e.Source, e.ExternalId })
                .ToList();

            // Каждый идентификатор на своей строке, в формате «Источник: ID» (как на bdu.fstec.ru).
            // Дополнительная пустая строка между группами разных источников — для читаемости.
            string extIdText = string.Join("\r\n\r\n",
                extIds
                    .GroupBy(e => e.Source ?? "—")
                    .OrderBy(g => g.Key)
                    .Select(g => string.Join("\r\n",
                        g.Select(x => x.ExternalId)
                         .Where(id => !string.IsNullOrWhiteSpace(id))
                         .Distinct()
                         .OrderBy(id => id)
                         .Select(id => $"{g.Key}: {id}"))));

            // ---- Меры по устранению 1:N ----
            var mitigations = _db.VulnerabilityMitigations
                .Where(m => m.VulnerabilityId == vulnId)
                .Select(m => m.Measure)
                .ToList();

            // ---- Рекомендуемые обновления 1:N ----
            var testingUpdates = _db.VulnerabilityTestingUpdates
                .Where(t => t.VulnerabilityId == vulnId)
                .Select(t => new { t.UpdateIdentifier, t.UpdateName })
                .ToList();

            string testingText = string.Join("\r\n",
                testingUpdates.Select(t =>
                    $"{(string.IsNullOrWhiteSpace(t.UpdateIdentifier) ? "" : t.UpdateIdentifier + " — ")}{t.UpdateName}"));

            // ---- Платформы для отдельной строки «Операционные системы и аппаратные платформы».
            // Берём полные имена «Vendor Product - Architecture» из таблицы os_platforms — ровно
            // как на bdu.fstec.ru, по одной записи в строку. Сортируем стабильно для красивого вывода.
            var platforms = softwareData.Select(s => s.Platform)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            // ============================================================
            // НАПОЛНЕНИЕ ТАБЛИЦЫ
            // ============================================================
            AddTextRow("Описание уязвимости", _vuln.Description);
            AddSoftwareRow(softwareData);
            AddTextRow("Операционные системы и аппаратные платформы", string.Join("\r\n", platforms));
            AddTextRow("Тип ошибки", GetCweText());
            AddTextRow("Класс уязвимости", _vuln.VulnClass?.Name);
            AddTextRow("Дата выявления", FmtDate(_vuln.DiscoveryDate));
            AddTextRow("Базовый вектор уязвимости", GetCvssText());
            AddTextRow("Уровень опасности уязвимости", GetSeverityText());
            AddTextRow("Возможные меры по устранению",
                       mitigations.Count > 0 ? string.Join("\r\n", mitigations) : null);
            AddTextRow("Статус уязвимости", _vuln.Status?.Name);
            AddTextRow("Наличие эксплойта", _vuln.ExploitAvailability?.Name);
            AddTextRow("Способ эксплуатации", _vuln.ExploitationMethod?.Name);
            AddTextRow("Способ устранения", _vuln.FixMethod?.Name);
            AddTextRow("Информация об устранении", _vuln.FixInfo);
            AddLinksRow("Ссылки на источники", links);
            AddTextRow("Идентификаторы других систем описаний уязвимости", extIdText);
            AddTextRow("Связь с инцидентами ИБ", _vuln.IncidentRelation?.Name);
            AddTextRow("Последствия эксплуатации уязвимости", _vuln.ExploitationConsequences);
            AddTextRow("Дата публикации", FmtDate(_vuln.PublicationDate));
            AddTextRow("Дата последнего обновления", FmtDate(_vuln.LastUpdateDate));
            AddTextRow("Рекомендуемые обновления для тестирования", testingText);
            AddTextRow("Прочая информация", _vuln.OtherInfo);
        }

        // ============================================================
        // СТРОКИ ТАБЛИЦЫ
        // ============================================================

        /// <summary>
        /// Универсальная строка «метка + текст». Высота вычисляется по фактическому
        /// размеру текста с учётом переноса слов в столбце.
        /// </summary>
        private void AddTextRow(string fieldName, string value)
        {
            string text = string.IsNullOrWhiteSpace(value) ? "Данные уточняются" : value;

            int rowIndex = table.RowCount;
            table.RowCount++;

            // Считаем высоту текста с переносом по ширине колонки значения
            using (var font = new Font(this.Font.FontFamily, 9))
            using (var headerFont = new Font(this.Font.FontFamily, 9, FontStyle.Bold))
            {
                Size txtSize = TextRenderer.MeasureText(text, font,
                    new Size(ValueColumnWidth - 14, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                Size hdrSize = TextRenderer.MeasureText(fieldName, headerFont,
                    new Size(LabelColumn - 16, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

                int rowHeight = Math.Max(34, Math.Max(txtSize.Height, hdrSize.Height) + 16);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));

                table.Controls.Add(MakeFieldLabel(fieldName), 0, rowIndex);
                table.Controls.Add(MakeValueBox(text), 1, rowIndex);
            }
        }

        private void AddSoftwareRow(List<SoftwareInfo> softwareList)
        {
            int rowIndex = table.RowCount;
            table.RowCount++;

            int rowsCount = Math.Max(1, softwareList.Count);
            int dgvHeight = 32 + rowsCount * 26 + 4;
            int rowHeight = Math.Max(80, Math.Min(dgvHeight, 400));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));

            table.Controls.Add(MakeFieldLabel("Уязвимое ПО"), 0, rowIndex);

            var dgv = new DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                ScrollBars = ScrollBars.Vertical
            };

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(38, 38, 42);
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 60, 65);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.GridColor = Color.FromArgb(70, 70, 70);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 55);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font.FontFamily, 8, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            dgv.Columns.Add("Vendor", "Вендор");
            dgv.Columns.Add("Software", "Наименование ПО");
            dgv.Columns.Add("Version", "Версия ПО");
            dgv.Columns.Add("ProductType", "Тип ПО");
            dgv.Columns.Add("Platform", "Архитектура (Платформа)");

            // Пропорции колонок как на bdu.fstec.ru: «Наименование ПО» и «Тип ПО» широкие,
            // «Версия» — узкая (там часто прочерк).
            dgv.Columns["Vendor"].FillWeight = 15;
            dgv.Columns["Software"].FillWeight = 35;
            dgv.Columns["Version"].FillWeight = 10;
            dgv.Columns["ProductType"].FillWeight = 20;
            dgv.Columns["Platform"].FillWeight = 20;

            // В колонку «Архитектура» кладём только хвост после « — » («32-bit» / «64-bit» / «x86 (x32 / IA-32)»).
            // Сама строка «Microsoft Corp Windows XP - 32-bit» как была в os_platforms.name так и остаётся —
            // она полностью выводится в блоке «Операционные системы и аппаратные платформы».
            foreach (var item in softwareList)
                dgv.Rows.Add(
                    item.Vendor,
                    item.Software,
                    item.Version,
                    item.ProductType,
                    ExtractArchitecture(item.Platform));

            table.Controls.Add(dgv, 1, rowIndex);
        }

        private void AddLinksRow(string fieldName, List<string> links)
        {
            int rowIndex = table.RowCount;
            table.RowCount++;

            int linkCount = Math.Max(1, links.Count);
            int rowHeight = Math.Max(40, linkCount * 18 + 16);
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));

            table.Controls.Add(MakeFieldLabel(fieldName), 0, rowIndex);

            var rtb = new RichTextBox
            {
                ReadOnly = true,
                BackColor = Color.FromArgb(38, 38, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font(this.Font.FontFamily, 9),
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                DetectUrls = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            rtb.LinkClicked += (s, e) =>
            {
                try { Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true }); }
                catch { }
            };

            if (links.Count == 0)
                rtb.AppendText("Данные уточняются");
            else
                foreach (var link in links)
                    rtb.AppendText(link + "\r\n");

            table.Controls.Add(rtb, 1, rowIndex);
        }

        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЕ ФАБРИКИ
        // ============================================================
        private Label MakeFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White,
                Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 6, 8, 6),
                AutoSize = false,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
        }

        /// <summary>
        /// Многострочный TextBox без рамки/полос прокрутки — текст полностью вписан
        /// в высоту строки, выделяется и копируется.
        /// </summary>
        private TextBox MakeValueBox(string text)
        {
            return new TextBox
            {
                Text = text,
                Multiline = true,
                ReadOnly = true,
                WordWrap = true,
                ScrollBars = ScrollBars.None,
                BackColor = Color.FromArgb(38, 38, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font(this.Font.FontFamily, 9),
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                TabStop = false
            };
        }

        // ============================================================
        // УТИЛИТЫ ОТОБРАЖЕНИЯ
        // ============================================================
        private string GetCweText()
        {
            // 1) Собираем все CWE из junction-таблицы vulnerability_cwes
            //    (в BDU часто несколько CWE на одну уязвимость).
            var cwes = _vuln?.VulnerabilityCwes?
                            .Where(vc => vc.Cwe != null)
                            .Select(vc => vc.Cwe)
                            .Distinct()
                            .ToList();

            // 2) Если junction пуст (старые данные), фоллбэком берём единичный _vuln.Cwe.
            if ((cwes == null || cwes.Count == 0) && _vuln?.Cwe != null)
                cwes = new List<Cwe> { _vuln.Cwe };

            if (cwes == null || cwes.Count == 0) return null;

            return string.Join("\r\n",
                cwes.OrderBy(c => c.Code)
                    .Select(c => string.IsNullOrWhiteSpace(c.Description)
                                 ? c.Code
                                 : $"{c.Code} — {c.Description}"));
        }

        private string GetSeverityText()
        {
            // Предпочтительно — полный текст из BDU (может включать оценки по CVSS 2.0/3.1/4.0).
            // В Excel часто две-три фразы «… уровень опасности (… составляет N)» идут одной строкой
            // без переноса — нормализуем, чтобы каждая строка отображалась отдельно.
            if (!string.IsNullOrWhiteSpace(_vuln?.SeverityText))
                return NormalizeSeverityText(_vuln.SeverityText);
            return _vuln?.SeverityLevel?.Name;
        }

        // В БДУ несколько уровней опасности часто склеены без переноса, например
        // «...составляет 8.5)Высокий уровень опасности (...составляет 5.5)».
        // WinForms TextBox в Multiline=true показывает перенос только по \r\n,
        // одинарный \n игнорирует — поэтому сразу формируем CRLF.
        private static string NormalizeSeverityText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            string s = raw.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

            // Перед каждым «<уровень> уровень опасности» (кроме самого начала) ставим перевод строки.
            s = Regex.Replace(
                s,
                @"(?<!^)\s*(?=(?:Критический|Высокий|Средний|Низкий|Информационный|Базовый)\s+уровень\s+опасности)",
                "\n");

            // Схлопываем подряд идущие переводы и переходим на CRLF для WinForms.
            s = Regex.Replace(s, @"\n{2,}", "\n");
            return s.Replace("\n", "\r\n").Trim();
        }

        private string GetCvssText()
        {
            var lines = new List<string>();
            if (!string.IsNullOrWhiteSpace(_vuln?.Cvss2_0_Vector) || _vuln?.Cvss2_0_Score != null)
                lines.Add($"CVSS 2.0: {_vuln.Cvss2_0_Vector} ({FormatScore(_vuln.Cvss2_0_Score)})");
            if (!string.IsNullOrWhiteSpace(_vuln?.Cvss3_0_Vector) || _vuln?.Cvss3_0_Score != null)
                lines.Add($"CVSS 3.0: {_vuln.Cvss3_0_Vector} ({FormatScore(_vuln.Cvss3_0_Score)})");
            if (!string.IsNullOrWhiteSpace(_vuln?.Cvss4_0_Vector) || _vuln?.Cvss4_0_Score != null)
                lines.Add($"CVSS 4.0: {_vuln.Cvss4_0_Vector} ({FormatScore(_vuln.Cvss4_0_Score)})");
            return lines.Count > 0 ? string.Join("\r\n", lines) : null;
        }

        private static string FormatScore(decimal? score) =>
            score?.ToString("0.0", CultureInfo.InvariantCulture) ?? "—";

        private static string FmtDate(DateTime? d) => d?.ToString("dd.MM.yyyy") ?? "—";

        /// <summary>
        /// Из строки вида «Microsoft Corp Windows XP - 32-bit» вырезает хвост «32-bit» (архитектура).
        /// Поддерживает форматы НБДУ: «32-bit», «64-bit», «x86 (x32 / IA-32)», «IA-64», «ARM», «x64» и т.п.
        /// Если в имени нет сепаратора « — » / « - » — возвращает исходную строку как есть.
        /// </summary>
        private static string ExtractArchitecture(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName)) return "";
            string s = platformName.Trim();

            // Сначала ищем разделитель « — » (тире em-dash) или « - » (дефис hyphen) — это формат БДУ
            // «Vendor Product - Architecture» или «Vendor Product — Architecture».
            int dashIdx = s.LastIndexOf(" — ", StringComparison.Ordinal);
            int hyphenIdx = s.LastIndexOf(" - ", StringComparison.Ordinal);
            int idx = Math.Max(dashIdx, hyphenIdx);
            int sepLen = (idx == dashIdx) ? 3 : 3; // « — » и « - » оба в 3 символа в .NET
            if (idx > 0)
            {
                string tail = s.Substring(idx + sepLen).Trim();
                // Адекватная длина архитектуры — не больше 80 символов; иначе это очевидно не «xxx-bit»,
                // а кусок названия ПО — в этом случае показываем всю исходную строку.
                if (tail.Length > 0 && tail.Length <= 80) return tail;
            }
            return s;
        }

        /// <summary>
        /// В БДУ ячейка «Версия ПО» хранит формат «&lt;версия&gt; (&lt;имя ОС&gt;)», например
        /// «- (Windows XP)» или «1.0 (Windows 7)». В колонке «Версия ПО» нужна только сама версия,
        /// контекст ОС уже виден в колонках «Наименование ПО» / «Архитектура».
        /// Возвращает «-» если версии нет (типичный случай в БДУ — прочерк).
        /// </summary>
        private static string ExtractVersionOnly(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "-";
            string s = raw.Trim();

            // Отрезаем «(... имя ОС ...)» и всё что после.
            int openParen = s.IndexOf('(');
            if (openParen > 0)
                s = s.Substring(0, openParen).Trim();

            // Удаляем висячий разделитель типа « —» / « -» в конце, если такой остался.
            s = s.TrimEnd('—', '-', ' ', '\t');
            if (string.IsNullOrWhiteSpace(s)) return "-";

            // Если кроме одиночного «-» / «—» больше ничего нет — нормализуем к «-».
            if (s == "—") return "-";
            return s;
        }

        /// <summary>
        /// Бывает, в products.name имя ПО хранится вместе с вендором («Microsoft Corp Windows XP»).
        /// Чтобы в колонке «Наименование ПО» была только «Windows XP» — срежем префикс вендора.
        /// Если vendor пустой или не совпадает — возвращаем исходную строку как есть.
        /// </summary>
        private static string StripVendorPrefix(string productName, string vendorName)
        {
            if (string.IsNullOrWhiteSpace(productName)) return "";
            string p = productName.Trim();
            if (string.IsNullOrWhiteSpace(vendorName)) return p;
            string v = vendorName.Trim();
            if (p.Length > v.Length + 1 && p.StartsWith(v + " ", StringComparison.OrdinalIgnoreCase))
                return p.Substring(v.Length + 1).Trim();
            return p;
        }

        private void btnBack_Click(object sender, EventArgs e) => this.Close();

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _db.Dispose();
            base.OnFormClosed(e);
        }
    }
}
