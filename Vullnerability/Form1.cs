using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using Vullnerability.db;

namespace Vullnerability
{
    public partial class Form1 : Form
    {
        private readonly VulnDbContext db = new VulnDbContext();
        private int _pageSize = 100;
        private int _pageIndex = 0;
        private SortField _sortField = SortField.BduId;
        private bool _sortDescending = true;
        private List<Vulnerability> _recentVulns = new List<Vulnerability>();

        private enum SortField { BduId, DiscoveryDate, Severity }

        private const string ExcelUrl = "https://bdu.fstec.ru/files/documents/vullist.xlsx";

        public Form1()
        {
            InitializeComponent();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                                                  SecurityProtocolType.Tls11 |
                                                  SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(ValidateServerCertificate);

            ExcelPackage.License.SetNonCommercialPersonal("YourName");

            LoadDictionaries();
            SetupSortCombo();
            SetupPageSizeCombo();
            LoadData();
            InitRecentList();
            LoadRecentVulns();
            WireSearchEnter();
            // ArrangePagingButtons() больше не нужен — раскладку делает Designer
            // (panelBottomBar c Dock=Left/Fill/Right на трёх кнопках).
        }

        /// <summary>
        /// Чтобы Enter в поле поиска применял фильтр (как на bdu.fstec.ru).
        /// </summary>
        private void WireSearchEnter()
        {
            if (txtSearchName == null) return;
            txtSearchName.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    btnApplyFilter_Click(btnApplyFilter, EventArgs.Empty);
                }
            };
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        // ============================================================
        // СПРАВОЧНИКИ В ФИЛЬТРАХ
        // ============================================================
        /// <summary>
        /// Списки, нужные для всех комбобоксов фильтра. Готовятся один раз,
        /// применяются одним батчем — без «промежуточных» пустых состояний UI.
        /// </summary>
        private class DictionariesSnapshot
        {
            public string[] Vendors;
            public string[] ProductTypes;
            public string[] Products;
            public string[] Platforms;
            public string[] OsFamilies;
            public string[] Statuses;
            public string[] Classes;
            public string[] SeverityLevels;
            public string[] Cwes;
            public string[] ExploitMethods;
            public string[] FixMethods;
            public string[] Years;
            public string[] Versions;
            public string[] ExternalIds;
        }

        /// <summary>
        /// Семейства ОС без версий, которыми заполняется cmbOsName. Из этого
        /// списка в справочник UI попадают только те фамилии, которые реально встречаются в
        /// os_platforms (чтобы не показывать заведомо пустые опции).
        /// </summary>
        private static readonly string[] KnownOsFamilies = new[]
        {
            "Windows", "Linux", "macOS", "OS X", "iOS", "Android",
            "FreeBSD", "OpenBSD", "NetBSD", "Solaris", "AIX", "HP-UX",
            "Astra Linux", "Альт Линукс", "Альт", "ROSA", "RED OS",
            "Ubuntu", "Debian", "CentOS", "RHEL", "Red Hat", "SUSE", "Fedora", "Alpine",
            "VMware ESXi", "Cisco IOS", "Juniper Junos"
        };

        /// <summary>
        /// Полный сбор данных справочников из БД. Должно вызываться с выделенным для
        /// этого вызова VulnDbContext (DbContext не thread-safe).
        /// </summary>
        private static DictionariesSnapshot FetchDictionaries(VulnDbContext ctx)
        {
            var platforms = ctx.OsPlatforms.AsNoTracking()
                .OrderBy(p => p.Name).Select(p => p.Name).ToArray();

            // Семейства ОС без версий — берём из фиксированного списка только те,
            // что реально встречаются в os_platforms.name (case-insensitive substring).
            var osFamilies = KnownOsFamilies
                .Where(f => platforms.Any(p =>
                    !string.IsNullOrEmpty(p) &&
                    p.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(f => f)
                .ToArray();

            // Автодополнение для «Версия ПО» и «Другие ИД» — берём до 10 000 самых
            // частых значений (сортировка по count desc в SQL).
            var versions = ctx.VulnerabilityProducts.AsNoTracking()
                .Where(vp => vp.ProductVersion != null && vp.ProductVersion != "")
                .GroupBy(vp => vp.ProductVersion)
                .OrderByDescending(g => g.Count())
                .Take(10000)
                .Select(g => g.Key)
                .ToArray();

            var extIds = ctx.VulnerabilityExternalIds.AsNoTracking()
                .Where(e => e.ExternalId != null && e.ExternalId != "")
                .GroupBy(e => e.ExternalId)
                .OrderByDescending(g => g.Count())
                .Take(10000)
                .Select(g => g.Key)
                .ToArray();

            return new DictionariesSnapshot
            {
                Vendors = ctx.Vendors.AsNoTracking()
                                    .OrderBy(v => v.Name).Select(v => v.Name).ToArray(),
                ProductTypes = ctx.ProductTypes.AsNoTracking()
                                    .OrderBy(t => t.Name).Select(t => t.Name).ToArray(),
                Products = ctx.Products.AsNoTracking()
                                    .OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToArray(),
                Platforms = platforms,
                OsFamilies = osFamilies,
                Statuses = ctx.VulnStatuses.AsNoTracking()
                                    .OrderBy(s => s.Name).Select(s => s.Name).ToArray(),
                Classes = ctx.VulnClasses.AsNoTracking()
                                    .OrderBy(c => c.Name).Select(c => c.Name).ToArray(),
                SeverityLevels = ctx.SeverityLevels.AsNoTracking()
                                    .OrderBy(s => s.Id).Select(s => s.Name).ToArray(),
                Cwes = ctx.Cwes.AsNoTracking()
                                    .OrderBy(c => c.Code).Select(c => c.Code).ToArray(),
                ExploitMethods = ctx.ExploitationMethods.AsNoTracking()
                                    .OrderBy(e => e.Name).Select(e => e.Name).ToArray(),
                FixMethods = ctx.FixMethods.AsNoTracking()
                                    .OrderBy(r => r.Name).Select(r => r.Name).ToArray(),
                Years = ctx.Vulnerabilities.AsNoTracking()
                                    .Where(v => v.PublicationDate != null)
                                    .Select(v => v.PublicationDate.Value.Year)
                                    .Distinct().OrderBy(y => y)
                                    .ToArray()
                                    .Select(y => y.ToString()).ToArray(),
                Versions = versions,
                ExternalIds = extIds
            };
        }

        /// <summary>
        /// Применяет подготовленные данные к комбобоксам одним батчем.
        /// Сохраняет текущий выбор в комбобоксах во время обновления.
        /// </summary>
        private void ApplyDictionaries(DictionariesSnapshot d)
        {
            // Приостанавливаем любые лейаутные пересчёты на время рефила.
            this.SuspendLayout();
            try
            {
                FillCombo(cmbVendor, d.Vendors);
                FillCombo(cmbProductType, d.ProductTypes);
                FillCombo(cmbProduct, d.Products);
                FillCombo(cmbPlatform, d.Platforms);
                // cmbOsName — только семейства ОС без версий/разрядности.
                FillCombo(cmbOsName, d.OsFamilies);
                FillCombo(cmbStatus, d.Statuses);
                FillCombo(cmbClass, d.Classes);
                FillCombo(cmbDanger, d.SeverityLevels);
                FillCombo(cmbCweType, d.Cwes);
                FillCombo(cmbExploitMethod, d.ExploitMethods);
                FillCombo(cmbFixMethod, d.FixMethods);
                FillCombo(cmbYearAdded, d.Years);

                // Автодополнение для текстовых полей «Версия ПО» и «Другие ИД».
                AttachAutoComplete(txtVersion, d.Versions);
                AttachAutoComplete(txtOtherId, d.ExternalIds);
            }
            finally
            {
                this.ResumeLayout(false);
                // Единственная перерисовка панели фильтров.
                this.PerformLayout();
            }
        }

        /// <summary>
        /// Подключает к TextBox автодополнение по переданному списку значений.
        /// Под капотом WinForms AutoCompleteCustomSource — выпадашка предложений
        /// выводится с первого введённого символа.
        /// </summary>
        private static void AttachAutoComplete(TextBox tb, string[] values)
        {
            if (tb == null) return;
            var src = new AutoCompleteStringCollection();
            if (values != null && values.Length > 0)
                src.AddRange(values);
            tb.AutoCompleteCustomSource = src;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }

        /// <summary>
        /// Синхронная версия — используется в конструкторе формы. Для обновления после
        /// импорта используйте LoadDictionariesAsync().
        /// </summary>
        private void LoadDictionaries()
        {
            ApplyDictionaries(FetchDictionaries(db));
        }

        /// <summary>
        /// Асинхронный рефреш — сбор данных на фоновом потоке, применение на UI.
        /// </summary>
        private async Task LoadDictionariesAsync()
        {
            var snap = await Task.Run(() =>
            {
                using (var ctx = new VulnDbContext())
                    return FetchDictionaries(ctx);
            });
            ApplyDictionaries(snap);
        }

        /// <summary>
        /// Батчевое заполнение комбобокса: сохраняем текущее значение, BeginUpdate →
        /// AddRange → EndUpdate. Без промежуточной перерисовки и без «исчезающих» списков.
        /// </summary>
        private void FillCombo(ComboBox combo, string[] values)
        {
            if (combo == null) return;
            string previousText = combo.Text;

            combo.BeginUpdate();
            try
            {
                combo.Items.Clear();
                // Цельный массив: пустый элемент + все значения. AddRange одним вызовом.
                if (values != null && values.Length > 0)
                {
                    var arr = new object[values.Length + 1];
                    arr[0] = "";
                    Array.Copy(values, 0, arr, 1, values.Length);
                    combo.Items.AddRange(arr);
                }
                else
                {
                    combo.Items.Add("");
                }
                combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                combo.AutoCompleteSource = AutoCompleteSource.ListItems;

                // Восстанавливаем выбор, если он всё ещё есть в списке.
                if (!string.IsNullOrEmpty(previousText))
                {
                    int idx = combo.Items.IndexOf(previousText);
                    if (idx >= 0) combo.SelectedIndex = idx;
                }
            }
            finally
            {
                combo.EndUpdate();
            }
        }

        // ============================================================
        // СОРТИРОВКА / ПАГИНАЦИЯ
        // ============================================================
        // 3 пункта сортировки + переключение направления по повторному клику на тот же элемент.
        private static readonly (string Title, SortField Field)[] _sortItems =
        {
            ("По идентификатору",  SortField.BduId),
            ("По дате выявления",   SortField.DiscoveryDate),
            ("По критичности",      SortField.Severity),
        };

        private void SetupSortCombo()
        {
            cmbSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSort.Items.Clear();
            foreach (var item in _sortItems)
                cmbSort.Items.Add(item.Title);
            cmbSort.SelectedIndex = 0;
            UpdateSortComboText();
        }

        // Обновляем текст комбо с символом направления, чтобы пользователь видел: DESC ↓ / ASC ↑.
        private void UpdateSortComboText()
        {
            if (cmbSort.SelectedIndex < 0 || cmbSort.SelectedIndex >= _sortItems.Length) return;
            string baseTitle = _sortItems[cmbSort.SelectedIndex].Title;
            cmbSort.Text = baseTitle + (_sortDescending ? " ↓" : " ↑");
        }

        private void SetupPageSizeCombo()
        {
            // В Designer.cs набор Items уже задан, но содержит «20» и пересекается с нашим
            // списком — поэтому сначала чистим, потом ставим только три нужных значения.
            cmbPageSize.BeginUpdate();
            try
            {
                cmbPageSize.Items.Clear();
                cmbPageSize.Items.AddRange(new object[] { "10", "50", "100" });
                cmbPageSize.SelectedIndex = 2; // 100 по умолчанию
            }
            finally
            {
                cmbPageSize.EndUpdate();
            }
        }

        private void cmbSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = cmbSort.SelectedIndex;
            if (idx < 0 || idx >= _sortItems.Length) return;
            // При смене поля — начинаем с DESC по умолчанию («от новых к старым» / «от больших CVSS к меньшим»).
            // Тогл направления делается повторным кликом на тот же пункт (см. WndProc выше).
            _sortField = _sortItems[idx].Field;
            _sortDescending = true;
            _pageIndex = 0;
            RefreshCurrentPage();
            UpdateSortComboText();
        }

        private void cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            _pageSize = int.Parse(cmbPageSize.Text);
            _pageIndex = 0;
            RefreshCurrentPage();
        }



        // ============================================================
        // СПИСОК "ПОСЛЕДНИЕ"
        // ============================================================
        private void InitRecentList()
        {
            lstRecentVulns.DrawMode = DrawMode.OwnerDrawVariable;
            lstRecentVulns.MeasureItem += LstRecentVulns_MeasureItem;
            lstRecentVulns.DrawItem += LstRecentVulns_DrawItem;
            lstRecentVulns.DoubleClick += lstRecentVulns_DoubleClick;
        }

        // Паддинги элемента списка. Подобраны так, чтобы визуально было похоже на блок «Последние изменения»
        // на bdu.fstec.ru: дата вверху, описание ниже с переносом по словам, между элементами воздух.
        private const int RecentItemSidePad = 6;
        private const int RecentItemTopPad = 4;
        private const int RecentItemBottomPad = 8;
        private const int RecentDateGap = 2;

        private void LstRecentVulns_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _recentVulns.Count) return;
            var v = _recentVulns[e.Index];
            string description = SquashWhitespace(v.Description) ?? string.Empty;

            int contentWidth = Math.Max(50, lstRecentVulns.ClientSize.Width - RecentItemSidePad * 2);
            // Для даты (одна строка) — высота фиксированная, для описания — рендер word-wrap.
            var dateSize = e.Graphics.MeasureString("06.05.2025", lstRecentVulns.Font, contentWidth);
            var descSize = string.IsNullOrEmpty(description)
                ? new SizeF(0, 0)
                : e.Graphics.MeasureString(description, lstRecentVulns.Font, contentWidth);

            e.ItemHeight = (int)Math.Ceiling(dateSize.Height) + RecentDateGap
                           + (int)Math.Ceiling(descSize.Height)
                           + RecentItemTopPad + RecentItemBottomPad;
        }

        private void LstRecentVulns_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _recentVulns.Count) return;
            e.DrawBackground();

            var v = _recentVulns[e.Index];
            string date = (v.PublicationDate ?? v.DiscoveryDate ?? v.LastUpdateDate)?.ToString("dd.MM.yyyy") ?? "—";
            string description = SquashWhitespace(v.Description) ?? string.Empty;

            // 1) Дата сверху (более бледная строка-ориентир — как на скрине).
            int contentLeft = e.Bounds.Left + RecentItemSidePad;
            int contentTop = e.Bounds.Top + RecentItemTopPad;
            int contentWidth = e.Bounds.Width - RecentItemSidePad * 2;

            var dateSize = e.Graphics.MeasureString(date, e.Font, contentWidth);
            var dateRect = new RectangleF(contentLeft, contentTop, contentWidth, dateSize.Height);
            using (var dateBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                e.Graphics.DrawString(date, e.Font, dateBrush, dateRect);

            // 2) Описание ниже, выбивается по словам по ширине элемента.
            if (!string.IsNullOrEmpty(description))
            {
                float descTop = dateRect.Bottom + RecentDateGap;
                var descRect = new RectangleF(contentLeft, descTop, contentWidth,
                                               e.Bounds.Bottom - descTop);
                using (var fmt = new StringFormat(StringFormatFlags.LineLimit) { Trimming = StringTrimming.None })
                using (var brush = new SolidBrush(e.ForeColor == Color.Empty ? Color.White : e.ForeColor))
                    e.Graphics.DrawString(description, e.Font, brush, descRect, fmt);
            }

            e.DrawFocusRectangle();
        }

        // Сжимает все whitespace-последовательности (переносы/табуляции) в одинарный пробел —
        // в BDU описания иногда идут с лишними переводами строки в середине фразы.
        private static string SquashWhitespace(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return System.Text.RegularExpressions.Regex.Replace(s.Trim(), "\\s+", " ");
        }

        // Уровни опасности от самого опасного к самому безопасному.
        // Используется, чтобы из severity_text (где может быть несколько уровней по
        // разным версиям CVSS) выбрать «самый низкий» — т.е. наименее тревожный.
        private static readonly string[] SeverityOrder =
            { "крит", "высок", "сред", "низк", "информ" };

        private static string PickLowestSeverity(string severityText, string fallback)
        {
            // Ищем все упоминания уровней в severity_text. Берём тот, что ниже всего по шкале.
            int lowest = -1;
            if (!string.IsNullOrWhiteSpace(severityText))
            {
                var t = severityText.ToLowerInvariant();
                for (int i = 0; i < SeverityOrder.Length; i++)
                    if (t.Contains(SeverityOrder[i]) && i > lowest) lowest = i;
            }
            if (lowest >= 0) return SeverityOrder[lowest];

            // Если в тексте ничего не нашли — пытаемся распознать справочное имя.
            return (fallback ?? string.Empty).ToLowerInvariant();
        }

        private void LoadRecentVulns()
        {
            _recentVulns = db.Vulnerabilities
                .OrderByDescending(v =>
                    v.PublicationDate ?? v.DiscoveryDate ?? v.LastUpdateDate ?? new DateTime(1900, 1, 1))
                .Take(10)
                .ToList();

            ApplyRecentVulnsToList();
        }

        /// <summary>
        /// Асинхронный рефреш «Последних» — запрос на фоновом потоке.
        /// </summary>
        private async Task LoadRecentVulnsAsync()
        {
            var ids = await Task.Run(() =>
            {
                using (var ctx = new VulnDbContext())
                {
                    return ctx.Vulnerabilities.AsNoTracking()
                        .OrderByDescending(v =>
                            v.PublicationDate ?? v.DiscoveryDate ?? v.LastUpdateDate ?? new DateTime(1900, 1, 1))
                        .Take(10)
                        .Select(v => v.Id)
                        .ToList();
                }
            });

            // Подтягиваем верхние 10 из «osnovnogo» домашнего контекста — их хватит для UI.
            _recentVulns = db.Vulnerabilities.Where(v => ids.Contains(v.Id))
                .OrderByDescending(v =>
                    v.PublicationDate ?? v.DiscoveryDate ?? v.LastUpdateDate ?? new DateTime(1900, 1, 1))
                .ToList();

            ApplyRecentVulnsToList();
        }

        private void ApplyRecentVulnsToList()
        {
            // Сам текст элементов не используется при отрисовке (рисуем по _recentVulns), но НУЖЕН для Items.Count.
            // Описываем каждую запись как «дата + описание» — это же строка идёт в ToolTip / клавиатурный поиск.
            lstRecentVulns.BeginUpdate();
            try
            {
                lstRecentVulns.Items.Clear();
                foreach (var v in _recentVulns)
                {
                    string date = (v.PublicationDate ?? v.DiscoveryDate ?? v.LastUpdateDate)?.ToString("dd.MM.yyyy") ?? "—";
                    string description = SquashWhitespace(v.Description) ?? v.Name ?? string.Empty;
                    lstRecentVulns.Items.Add($"{date} — {description}");
                }
            }
            finally
            {
                lstRecentVulns.EndUpdate();
            }
        }

        private void lstRecentVulns_DoubleClick(object sender, EventArgs e)
        {
            if (lstRecentVulns.SelectedIndex < 0) return;
            var vuln = _recentVulns[lstRecentVulns.SelectedIndex];
            using (var form = new VullDetailsForm(vuln.Id)) form.ShowDialog();
        }

        private void dgvVulns_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvVulns.Rows.Count) return;
            var vuln = dgvVulns.Rows[e.RowIndex].Tag as Vulnerability;
            if (vuln == null) return;
            using (var form = new VullDetailsForm(vuln.Id)) form.ShowDialog();
        }

        // ============================================================
        // ОТРИСОВКА ТЕКУЩЕЙ СТРАНИЦЫ
        // ============================================================
        private void LoadData()
        {
            _pageIndex = 0;
            RefreshCurrentPage();
        }

        private void RefreshCurrentPage()
        {
            // Чтобы не тянуть из БД все совпавшие записи в память и режать по страницам на клиенте
            // — всё выполняется на сервере (Count + Skip + Take + AsNoTracking).
            var query = db.Vulnerabilities.AsNoTracking()
                          .Include(v => v.SeverityLevel)
                          .AsQueryable();
            query = ApplyFilters(query);
            int totalCount = query.Count();
            query = ApplySorting(query);

            var pageData = query
                .Skip(_pageIndex * _pageSize)
                .Take(_pageSize)
                .ToList();

            dgvVulns.SuspendLayout();
            dgvVulns.Rows.Clear();
            foreach (var vuln in pageData)
            {
                string date = (vuln.PublicationDate ?? vuln.DiscoveryDate ?? vuln.LastUpdateDate)?
                    .ToString("dd.MM.yyyy") ?? "—";
                string desc = !string.IsNullOrWhiteSpace(vuln.Name) ? vuln.Name : vuln.Description;
                int rowIndex = dgvVulns.Rows.Add("", vuln.BduCode, desc, date);
                var row = dgvVulns.Rows[rowIndex];
                row.Tag = vuln;

                // Если в severity_text перечислено несколько уровней (CVSS 2.0/3.0/4.0)
                // — берём самый «мягкий» (наименее опасный) и красим по нему.
                var sev = PickLowestSeverity(vuln.SeverityText, vuln.SeverityLevel?.Name);
                Color sevColor;
                if (sev.Contains("крит")) sevColor = Color.FromArgb(200, 50, 50);
                else if (sev.Contains("высок")) sevColor = Color.FromArgb(220, 120, 40);
                else if (sev.Contains("сред")) sevColor = Color.FromArgb(200, 180, 50);
                else if (sev.Contains("низк")) sevColor = Color.FromArgb(80, 160, 80);
                else if (sev.Contains("информ")) sevColor = Color.FromArgb(80, 130, 200);
                else sevColor = Color.FromArgb(80, 80, 80);
                row.Cells[0].Style.BackColor = sevColor;
                row.Cells[0].Style.SelectionBackColor = sevColor;
            }
            dgvVulns.ResumeLayout();

            int totalPages = (int)Math.Ceiling(totalCount / (double)_pageSize);
            lblPageInfo.Text =
                $"Страница {_pageIndex + 1} из {Math.Max(1, totalPages)} | " +
                $"Показано: {pageData.Count} | Всего: {totalCount}";
            btnPrevPage.Enabled = _pageIndex > 0;
            btnNextPage.Enabled = _pageIndex < totalPages - 1;
        }

        // ============================================================
        // ФИЛЬТРЫ
        // ============================================================
        private IQueryable<Vulnerability> ApplyFilters(IQueryable<Vulnerability> source)
        {
            // Универсальный поиск по «Имя/Поиск»: BDU-код, наименование, описание,
            // вендор и название продукта (через vulnerability_products → products → vendors).
            var text = txtSearchName?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                source = source.Where(v =>
                    (v.BduCode != null && v.BduCode.Contains(text)) ||
                    (v.Name != null && v.Name.Contains(text)) ||
                    (v.Description != null && v.Description.Contains(text)) ||
                    db.VulnerabilityProducts
                      .Where(vp => vp.VulnerabilityId == v.Id)
                      .Any(vp =>
                          (vp.Product != null && vp.Product.Name != null && vp.Product.Name.Contains(text)) ||
                          (vp.Product != null && vp.Product.Vendor != null && vp.Product.Vendor.Name != null && vp.Product.Vendor.Name.Contains(text))));
            }

            if (!string.IsNullOrWhiteSpace(cmbVendor?.Text))
            {
                var vendor = cmbVendor.Text.Trim();
                source = source.Where(v =>
                    db.VulnerabilityProducts
                      .Where(vp => vp.VulnerabilityId == v.Id)
                      .Any(vp => vp.Product.Vendor != null && vp.Product.Vendor.Name.Contains(vendor)));
            }

            if (!string.IsNullOrWhiteSpace(cmbProduct?.Text))
            {
                var product = cmbProduct.Text.Trim();
                source = source.Where(v =>
                    db.VulnerabilityProducts
                      .Where(vp => vp.VulnerabilityId == v.Id)
                      .Any(vp => vp.Product.Name.Contains(product)));
            }

            if (!string.IsNullOrWhiteSpace(cmbProductType?.Text))
            {
                var type = cmbProductType.Text.Trim();
                source = source.Where(v =>
                    db.VulnerabilityProducts
                      .Where(vp => vp.VulnerabilityId == v.Id)
                      .Any(vp => vp.Product.ProductTypes.Any(pt => pt.Name == type)));
            }

            if (!string.IsNullOrWhiteSpace(cmbPlatform?.Text))
            {
                var platform = cmbPlatform.Text.Trim();
                source = source.Where(v =>
                    db.VulnerabilityProducts
                      .Where(vp => vp.VulnerabilityId == v.Id)
                      .Any(vp => vp.OsPlatform != null && vp.OsPlatform.Name.Contains(platform)));
            }

            if (!string.IsNullOrWhiteSpace(cmbOsName?.Text)
                && (string.IsNullOrWhiteSpace(cmbPlatform?.Text)
                    || cmbOsName.Text != cmbPlatform.Text))
            {
                var os = cmbOsName.Text.Trim();
                source = source.Where(v =>
                    db.VulnerabilityProducts
                      .Where(vp => vp.VulnerabilityId == v.Id)
                      .Any(vp => vp.OsPlatform != null && vp.OsPlatform.Name.Contains(os)));
            }

            if (!string.IsNullOrWhiteSpace(txtVersion?.Text))
            {
                var version = txtVersion.Text.Trim();
                source = source.Where(v =>
                    db.VulnerabilityProducts
                      .Where(vp => vp.VulnerabilityId == v.Id)
                      .Any(vp => vp.ProductVersion != null && vp.ProductVersion.Contains(version)));
            }

            if (!string.IsNullOrWhiteSpace(cmbStatus?.Text))
                source = source.Where(v => v.Status.Name == cmbStatus.Text);

            if (!string.IsNullOrWhiteSpace(cmbClass?.Text))
                source = source.Where(v => v.VulnClass.Name == cmbClass.Text);

            if (!string.IsNullOrWhiteSpace(cmbCweType?.Text))
                source = source.Where(v => v.Cwe.Code == cmbCweType.Text);

            if (!string.IsNullOrWhiteSpace(cmbYearAdded?.Text)
                && int.TryParse(cmbYearAdded.Text, out int year))
                source = source.Where(v =>
                    v.PublicationDate.HasValue && v.PublicationDate.Value.Year == year);

            if (!string.IsNullOrWhiteSpace(cmbDanger?.Text))
                source = source.Where(v => v.SeverityLevel.Name.Contains(cmbDanger.Text));

            if (!string.IsNullOrWhiteSpace(cmbExploitMethod?.Text))
                source = source.Where(v => v.ExploitationMethod.Name == cmbExploitMethod.Text);

            if (!string.IsNullOrWhiteSpace(cmbFixMethod?.Text))
                source = source.Where(v => v.FixMethod.Name == cmbFixMethod.Text);

            if (!string.IsNullOrWhiteSpace(txtOtherId?.Text))
            {
                var otherId = txtOtherId.Text.Trim();
                source = source.Where(v =>
                    db.VulnerabilityExternalIds
                      .Where(e => e.VulnerabilityId == v.Id)
                      .Any(e => e.ExternalId.Contains(otherId)));
            }

            if (chkHasIncidents.Checked)
                source = source.Where(v =>
                    v.IncidentRelation != null && v.IncidentRelation.Name == "Да");

            // Фильтр по дате — раньше был in-memory; теперь идёт SQL-выражением
            // (DiscoveryDate или PublicationDate в [from; to) — EF6 отлично транслирует в WHERE).
            if (chkUseDate.Checked)
            {
                var from = dtDateFrom.Value.Date;
                var to = dtDateTo.Value.Date.AddDays(1);
                source = source.Where(v =>
                    (v.DiscoveryDate.HasValue && v.DiscoveryDate >= from && v.DiscoveryDate < to) ||
                    (v.PublicationDate.HasValue && v.PublicationDate >= from && v.PublicationDate < to));
            }

            return source;
        }

        // ApplyInMemoryFilters больше не нужен — все фильтры переехали в ApplyFilters на сервер (прямой
        // SQL-фильтр + Skip/Take в SQL = пагинация на BigInt без перегона данных в память приложения).

        // ============================================================
        // СОРТИРОВКА
        // ============================================================
        private IQueryable<Vulnerability> ApplySorting(IQueryable<Vulnerability> source)
        {
            switch (_sortField)
            {
                case SortField.BduId:
                    return _sortDescending
                        ? source.OrderByDescending(v => v.BduCode)
                        : source.OrderBy(v => v.BduCode);

                case SortField.DiscoveryDate:
                    return _sortDescending
                        ? source.OrderByDescending(v => v.DiscoveryDate ?? v.PublicationDate ?? SqlDateTime.MinValue.Value)
                        : source.OrderBy(v => v.DiscoveryDate ?? v.PublicationDate ?? SqlDateTime.MaxValue.Value);

                case SortField.Severity:
                default:
                    // Двухуровневая сортировка как на bdu.fstec.ru:
                    //   1) ПЕРВИЧНЫЙ ключ — severity_level_id (справочник засеян в порядке
                    //      Критический=1 → Высокий=2 → Средний=3 → Низкий=4 → Информационный=5,
                    //      см. 01_schema.sql). Записи без severity_level_id уезжают в самый низ.
                    //   2) ВТОРИЧНЫЙ ключ — MAX(CVSS 2.0, 3.1, 4.0) (число из «скобок» в severity_text).
                    //      Так внутри одного уровня уязвимости отсортированы от более опасных к менее.
                    // Без первичного ключа («Критический», «Средний» и т.д.) перемешивались, потому что
                    // у некоторых записей CVSS-колонки NULL/0, а уровень при этом проставлен — и наоборот.
                    if (_sortDescending)
                        return source
                            .OrderBy(v => v.SeverityLevelId ?? int.MaxValue)
                            .ThenByDescending(v =>
                                (v.Cvss4_0_Score ?? 0m) >= (v.Cvss3_0_Score ?? 0m)
                                    && (v.Cvss4_0_Score ?? 0m) >= (v.Cvss2_0_Score ?? 0m)
                                    ? (v.Cvss4_0_Score ?? 0m)
                                    : ((v.Cvss3_0_Score ?? 0m) >= (v.Cvss2_0_Score ?? 0m)
                                        ? (v.Cvss3_0_Score ?? 0m)
                                        : (v.Cvss2_0_Score ?? 0m)));

                    return source
                        .OrderByDescending(v => v.SeverityLevelId ?? 0)
                        .ThenBy(v =>
                            (v.Cvss4_0_Score ?? 0m) >= (v.Cvss3_0_Score ?? 0m)
                                && (v.Cvss4_0_Score ?? 0m) >= (v.Cvss2_0_Score ?? 0m)
                                ? (v.Cvss4_0_Score ?? 0m)
                                : ((v.Cvss3_0_Score ?? 0m) >= (v.Cvss2_0_Score ?? 0m)
                                    ? (v.Cvss3_0_Score ?? 0m)
                                    : (v.Cvss2_0_Score ?? 0m)));
            }
        }

        // ============================================================
        // ПАГИНАЦИЯ
        // ============================================================
        private void btnNextPage_Click(object sender, EventArgs e)
        {
            var baseQuery = ApplyFilters(db.Vulnerabilities.AsNoTracking().AsQueryable());
            int totalCount = baseQuery.Count();
            int maxPage = Math.Max(0, (int)Math.Ceiling((double)totalCount / _pageSize) - 1);
            if (_pageIndex < maxPage)
            {
                _pageIndex++;
                RefreshCurrentPage();
            }
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (_pageIndex > 0)
            {
                _pageIndex--;
                RefreshCurrentPage();
            }
        }

        private void btnApplyFilter_Click(object sender, EventArgs e)
        {
            _pageIndex = 0;
            RefreshCurrentPage();
        }

        private void btnResetFilter_Click(object sender, EventArgs e)
        {
            txtSearchName?.Clear();
            txtVersion?.Clear();
            txtOtherId?.Clear();

            // ApplyFilters читает cmb.Text (не SelectedItem), поэтому SelectedIndex=-1
            // в ComboBox со стилем DropDown (autocomplete) не очищает текстовое поле —
            // надо явно сбрасывать и SelectedIndex, и Text.
            foreach (ComboBox cmb in new[]
            {
                cmbVendor, cmbProduct, cmbProductType, cmbStatus, cmbClass,
                cmbDanger, cmbYearAdded, cmbCweType, cmbExploitMethod, cmbFixMethod,
                cmbPlatform, cmbOsName
            })
            {
                if (cmb == null) continue;
                cmb.SelectedIndex = -1;
                cmb.Text = string.Empty;
            }

            chkUseDate.Checked = false;
            chkHasIncidents.Checked = false;

            // Даты приводим в исходное состояние (сегодня / сегодня), чтобы при следующем
            // включении chkUseDate пользователь не увидел случайно оставленный диапазон.
            var today = DateTime.Today;
            if (dtDateFrom != null) dtDateFrom.Value = today;
            if (dtDateTo != null) dtDateTo.Value = today;

            _pageIndex = 0;
            RefreshCurrentPage();
        }

        // ============================================================
        // ИМПОРТ ИЗ БДУ
        // ============================================================
        /// <summary>
        /// Все элементы панели фильтра, которые нужно блокировать на время импорта.
        /// </summary>
        private Control[] FilterControls() => new Control[]
        {
            cmbVendor, cmbProduct, cmbProductType, cmbStatus, cmbClass,
            cmbDanger, cmbYearAdded, cmbCweType, cmbExploitMethod, cmbFixMethod,
            cmbPlatform, cmbOsName, cmbSort, cmbPageSize,
            chkUseDate, chkHasIncidents
        };

        private void SetFilterUiEnabled(bool enabled)
        {
            foreach (var c in FilterControls())
                if (c != null) c.Enabled = enabled;
        }

        private async void btnUpdateFromBdu_Click(object sender, EventArgs e)
        {
            // Блокируем UI фильтров, пока идёт импорт — и ожидающий курсор вместо
            // «исчезающих/появляющихся» комбобоксов.
            btnUpdateFromBdu.Enabled = false;
            btnUpdateFromBdu.Text = "Обновление...";
            SetFilterUiEnabled(false);
            this.UseWaitCursor = true;

            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), "vullist_bdu.xlsx");
                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    await client.DownloadFileTaskAsync(new Uri(ExcelUrl), tempFile);
                }

                // Импорт — полностью на фоновом потоке (отдельный коннекшн).
                string connStr = db.Database.Connection.ConnectionString;
                var stats = await Task.Run(() =>
                {
                    var importer = new ExcelImporter(connStr);
                    return importer.ImportFromExcel(tempFile);
                });

                MessageBox.Show(
                    $"Импорт завершён!\nДобавлено: {stats.AddedVulns}\nПропущено (уже есть): {stats.SkippedVulns}",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Все постимпортные обновления тоже асинхронные: справочники и список
                // «Последние» — в фоне; применение одним батчем без промежуточных состояний.
                await LoadDictionariesAsync();
                await LoadRecentVulnsAsync();
                RefreshCurrentPage();
            }
            catch (WebException ex)
            {
                MessageBox.Show($"Ошибка сети:\n{ex.Message}", "Ошибка загрузки",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.UseWaitCursor = false;
                SetFilterUiEnabled(true);
                btnUpdateFromBdu.Enabled = true;
                btnUpdateFromBdu.Text = "Обновить из БДУ";
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            db.Dispose();
            base.OnFormClosed(e);
        }
    }
}