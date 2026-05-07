using System;
using System.Drawing;
using System.Windows.Forms;
using Vullnerability.db;

namespace Vullnerability
{
    public partial class VullCard : UserControl
    {
        private readonly Vulnerability _vuln;

        public VullCard(Vulnerability vuln)
        {
            InitializeComponent();
            _vuln = vuln;

            lblBduId.Text = _vuln.BduCode;
            lblVullName.Text = _vuln.Name;

            string dateStr = _vuln.DiscoveryDate?.ToString("dd.MM.yyyy")
                          ?? _vuln.PublicationDate?.ToString("dd.MM.yyyy") ?? "—";
            lblDiscoveryDate.Text = dateStr;

            // Цвет по критичности — берём максимальный из CVSS-оценок (decimal в новой схеме).
            decimal? score = _vuln.Cvss4_0_Score ?? _vuln.Cvss3_0_Score ?? _vuln.Cvss2_0_Score;
            UpdateSeverityColor(score);

            this.Click += VullCard_Click;
            foreach (Control c in this.Controls)
                c.Click += VullCard_Click;
        }

        private void UpdateSeverityColor(decimal? score)
        {
            if (!score.HasValue) return;

            Color backColor;
            if (score >= 9.0m) backColor = Color.FromArgb(255, 100, 100); // Критическая
            else if (score >= 7.0m) backColor = Color.FromArgb(255, 200, 100); // Высокая
            else if (score >= 4.0m) backColor = Color.FromArgb(255, 255, 150); // Средняя
            else backColor = Color.White;

            this.BackColor = backColor;
        }

        private void VullCard_Click(object sender, EventArgs e)
        {
            using (var details = new VullDetailsForm(_vuln.Id))
            {
                details.ShowDialog();
            }
        }
    }
}