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

            // берём максимальную из CVSS-оценок и красим карточку по ней
            decimal? score = _vuln.Cvss4_0_Score ?? _vuln.Cvss3_0_Score ?? _vuln.Cvss2_0_Score;
           

            // клик по любому контролу внутри = клик по карточке
            this.Click += VullCard_Click;
            foreach (Control c in this.Controls)
                c.Click += VullCard_Click;
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
