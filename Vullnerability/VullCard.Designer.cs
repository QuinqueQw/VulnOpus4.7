using System.Drawing;  // ✅ Обязательно!
using System.Windows.Forms;

namespace Vullnerability
{
    partial class VullCard
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblBduId;
        private Label lblVullName;
        private Label lblDiscoveryDate;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblBduId = new Label();
            this.lblVullName = new Label();
            this.lblDiscoveryDate = new Label();
            this.SuspendLayout();
            
            // lblBduId
            this.lblBduId.BackColor = Color.FromArgb(40, 40, 40);
            this.lblBduId.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblBduId.ForeColor = Color.White;
            this.lblBduId.Location = new Point(12, 12);
            this.lblBduId.Name = "lblBduId";
            this.lblBduId.Size = new Size(220, 42);
            this.lblBduId.TabIndex = 0;
            this.lblBduId.Text = "BDU:2023-00001";
            this.lblBduId.TextAlign = ContentAlignment.MiddleCenter;
            
            // lblVullName
            this.lblVullName.Font = new Font("Segoe UI", 9.5F);
            this.lblVullName.Location = new Point(240, 12);
            this.lblVullName.Name = "lblVullName";
            this.lblVullName.Size = new Size(620, 42);
            this.lblVullName.TabIndex = 1;
            this.lblVullName.Text = "Уязвимость переполнения буфера...";
            this.lblVullName.TextAlign = ContentAlignment.MiddleLeft;
            
            // lblDiscoveryDate
            this.lblDiscoveryDate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblDiscoveryDate.ForeColor = Color.DarkBlue;
            this.lblDiscoveryDate.Location = new Point(868, 12);
            this.lblDiscoveryDate.Name = "lblDiscoveryDate";
            this.lblDiscoveryDate.Size = new Size(220, 42);
            this.lblDiscoveryDate.TabIndex = 2;
            this.lblDiscoveryDate.Text = "15.03.2023";
            this.lblDiscoveryDate.TextAlign = ContentAlignment.MiddleCenter;
            
            // VullCard
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(this.lblDiscoveryDate);
            this.Controls.Add(this.lblVullName);
            this.Controls.Add(this.lblBduId);
            this.Cursor = Cursors.Hand;
            this.Margin = new Padding(4, 4, 4, 8);
            this.Name = "VullCard";
            this.Size = new Size(1100, 66);
            this.ResumeLayout(false);
        }
    }
}