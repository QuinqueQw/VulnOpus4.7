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
        }

        // чтобы Enter в поле поиска сразу применял фильтр
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

        // ---- Справочники в фильтрах ----

        // списки для комбобоксов
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

        // семейства ОС, из них в cmbOsName попадут только те, что реально есть в БД
        private static readonly string[] KnownOsFamilies = new[]
        {
            "Windows", "Linux", "macOS", "OS X", "iOS", "Android",
            "FreeBSD", "OpenBSD", "NetBSD", "Solaris", "AIX", "HP-UX",
            "Astra Linux", "Альт Линукс", "Альт", "ROSA", "RED OS",
            "Ubuntu", "Debian", "CentOS", "RHEL", "Red Hat", "SUSE", "Fedora", "Alpine",
            "VMware ESXi", "Cisco IOS", "Juniper Junos"
        };

        // DbContext не thread-safe, поэтому под фоновый вызов нужен свой ctx
        private static DictionariesSnapshot FetchDictionaries(VulnDbContext ctx)
        {
            var platforms = ctx.OsPlatforms.AsNoTracking()
                .OrderBy(p => p.Name).Select(p => p.Name).ToArray();

            // из списка ОС оставляем только те, что встречаются в os_platforms.name
            var osFamilies = KnownOsFamilies
                .Where(f => platforms.Any(p =>
                    !string.IsNullOrEmpty(p) &&
                    p.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(f => f)
                .ToArray();

            // для автодополнения «Версия» и «Другие ИД» берём топ-10000 частых значений
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

        // заполняем все комбобоксы разом (внутри SuspendLayout)
        private void ApplyDictionaries(DictionariesSnapshot d)
        {
            this.SuspendLayout();
            try
            {
                FillCombo(cmbVendor, d.Vendors);
                FillCombo(cmbProductType, d.ProductTypes);
                FillCombo(cmbProduct, d.Products);
                FillCombo(cmbPlatform, d.Platforms);
                // в cmbOsName кладём только семейства, без версий
                FillCombo(cmbOsName, d.OsFamilies);
                FillCombo(cmbStatus, d.Statuses);
                FillCombo(cmbClass, d.Classes);
                FillCombo(cmbDanger, d.SeverityLevels);
                FillCombo(cmbCweType, d.Cwes);
                FillCombo(cmbExploitMethod, d.ExploitMethods);
                FillCombo(cmbFixMethod, d.FixMethods);
                FillCombo(cmbYearAdded, d.Years);

                AttachAutoComplete(txtVersion, d.Versions);
                AttachAutoComplete(txtOtherId, d.ExternalIds);
            }
            finally
            {
                this.ResumeLayout(false);
                this.PerformLayout();
            }
        }

        // выпадающее автодополнение для TextBox
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

        // синхронный вариант — из конструктора
        private void LoadDictionaries()
        {
            ApplyDictionaries(FetchDictionaries(db));
        }

        // асинхронный вариант — после импорта
        private async Task LoadDictionariesAsync()
        {
            var snap = await Task.Run(() =>
            {
                using (var ctx = new VulnDbContext())
                    return FetchDictionaries(ctx);
            });
            ApplyDictionaries(snap);
        }

        // перезаливаем комбобокс одним батчем, старый выбор сохраняем
        private void FillCombo(ComboBox combo, string[] values)
        {
            if (combo == null) return;
            string previousText = combo.Text;

            combo.BeginUpdate();
            try
            {
                combo.Items.Clear();
                // первым идёт пустой элемент (сброс фильтра)
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

        // ---- Сортировка и пагинация ----

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
        }


        private void SetupPageSizeCombo()
        {
            cmbPageSize.BeginUpdate();
            try
            {
                // Устанавливаем кол-во выводимых записе на странице
                cmbPageSize.Items.Clear();
                cmbPageSize.Items.AddRange(new object[] { "10", "50", "100" });
                cmbPageSize.SelectedIndex = 2;
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
            // при смене поля всегда начинаем с DESC
            _sortField = _sortItems[idx].Field;
            _sortDescending = true;
            _pageIndex = 0;
            RefreshCurrentPage();
        }

        private void cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            _pageSize = int.Parse(cmbPageSize.Text);
            _pageIndex = 0;
            RefreshCurrentPage();
        }



        // ---- Список «Последние» ----

        private void InitRecentList()
        {
            lstRecentVulns.DrawMode = DrawMode.OwnerDrawVariable;
            lstRecentVulns.MeasureItem += LstRecentVulns_MeasureItem;
            lstRecentVulns.DrawItem += LstRecentVulns_DrawItem;
            lstRecentVulns.DoubleClick += lstRecentVulns_DoubleClick;
        }

        // паддинги в элементе списка
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
            // дата — одна строка, описание — с переносом по словам
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

            // дата сверху — более бледным цветом
            int contentLeft = e.Bounds.Left + RecentItemSidePad;
            int contentTop = e.Bounds.Top + RecentItemTopPad;
            int contentWidth = e.Bounds.Width - RecentItemSidePad * 2;

            var dateSize = e.Graphics.MeasureString(date, e.Font, contentWidth);
            var dateRect = new RectangleF(contentLeft, contentTop, contentWidth, dateSize.Height);
            using (var dateBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                e.Graphics.DrawString(date, e.Font, dateBrush, dateRect);

            // описание ниже с переносом по словам
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

        // в BDU описания иногда с лишними переносами — выравниваем в одну строку
        private static string SquashWhitespace(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return System.Text.RegularExpressions.Regex.Replace(s.Trim(), "\\s+", " ");
        }

        // от самого опасного к самому безопасному
        private static readonly string[] SeverityOrder =
            { "крит", "высок", "сред", "низк" };

        // в severity_text бывает несколько уровней (по CVSS 2/3/4) — берём самый безопасный
        private static string PickLowestSeverity(string severityText, string fallback)
        {
            int lowest = -1;
            if (!string.IsNullOrWhiteSpace(severityText))
            {
                var t = severityText.ToLowerInvariant();
                for (int i = 0; i < SeverityOrder.Length; i++)
                    if (t.Contains(SeverityOrder[i]) && i > lowest) lowest = i;
            }
            if (lowest >= 0) return SeverityOrder[lowest];

            // в тексте ничего не нашли — отдаём справочное имя
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

        // фоновый рефреш списка «Последние» (после импорта)
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

            // те же 10 записей перезагружаем уже в основной контекст — для UI этого хватит
            _recentVulns = db.Vulnerabilities.Where(v => ids.Contains(v.Id))
                .OrderByDescending(v =>
                    v.PublicationDate ?? v.DiscoveryDate ?? v.LastUpdateDate ?? new DateTime(1900, 1, 1))
                .ToList();

            ApplyRecentVulnsToList();
        }

        private void ApplyRecentVulnsToList()
        {
            // рисуем по _recentVulns, но Items.Count должен быть правильным — поэтому добавляем строки
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

        // ---- Отрисовка текущей страницы ----

        private void LoadData()
        {
            _pageIndex = 0;
            RefreshCurrentPage();
        }

        private void RefreshCurrentPage()
        {
            // пагинацию и фильтры делаем на стороне БД, в память тянем только страницу
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

                // левая полоска — цвет по самому безопасному из уровней
                var sev = PickLowestSeverity(vuln.SeverityText, vuln.SeverityLevel?.Name);
                Color sevColor;
                if (sev.Contains("крит")) sevColor = Color.FromArgb(200, 50, 50);
                else if (sev.Contains("высок")) sevColor = Color.FromArgb(220, 120, 40);
                else if (sev.Contains("сред")) sevColor = Color.FromArgb(200, 180, 50);
                else if (sev.Contains("низк")) sevColor = Color.FromArgb(80, 160, 80);
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

        // ---- Фильтры ----

        private IQueryable<Vulnerability> ApplyFilters(IQueryable<Vulnerability> source)
        {
            // общий поиск — по коду, имени, описанию, вендору и продукту
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

            // диапазон дат — ищем по DiscoveryDate или PublicationDate
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

        // ---- Сортировка ----

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

        // ---- Пагинация ----

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

            // в DropDown ComboBox сброс SelectedIndex не очищает Text, сбрасываем оба
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

            // даты выставляем на сегодня, чтобы при следующем включении chkUseDate не вылез старый диапазон
            var today = DateTime.Today;
            if (dtDateFrom != null) dtDateFrom.Value = today;
            if (dtDateTo != null) dtDateTo.Value = today;

            _pageIndex = 0;
            RefreshCurrentPage();
        }

        // ---- Импорт из БДУ ----

        // элементы фильтра, которые на время импорта отключаем
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
            // блокируем фильтры и кнопку на время импорта
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

                // импорт делаем в фоне и через отдельный коннекшн
                string connStr = db.Database.Connection.ConnectionString;
                var stats = await Task.Run(() =>
                {
                    var importer = new ExcelImporter(connStr);
                    return importer.ImportFromExcel(tempFile);
                });

                MessageBox.Show(
                    $"Импорт завершён!\nДобавлено: {stats.AddedVulns}\nПропущено (уже есть): {stats.SkippedVulns}",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // после импорта рефрешим справочники и список «Последние»
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
