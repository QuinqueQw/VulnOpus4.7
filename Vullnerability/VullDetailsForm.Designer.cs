using System.Windows.Forms;

namespace Vullnerability
{
    partial class VullDetailsForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelTop;
        private Button btnBack;
        private Label lblTitle;

        private Panel panelScroll;
        public TableLayoutPanel table;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            this.panelScroll = new System.Windows.Forms.Panel();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.panelTop.SuspendLayout();
            this.panelScroll.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Controls.Add(this.btnBack);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1000, 45);
            this.panelTop.TabIndex = 1;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(100, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(900, 45);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Детали уязвимости";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnBack
            // 
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBack.ForeColor = System.Drawing.SystemColors.Control;
            this.btnBack.Location = new System.Drawing.Point(0, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(100, 45);
            this.btnBack.TabIndex = 1;
            this.btnBack.Text = "← Назад";
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // panelScroll
            // 
            this.panelScroll.AutoScroll = true;
            this.panelScroll.Controls.Add(this.table);
            this.panelScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelScroll.Location = new System.Drawing.Point(0, 45);
            this.panelScroll.Name = "panelScroll";
            this.panelScroll.Padding = new System.Windows.Forms.Padding(10);
            this.panelScroll.Size = new System.Drawing.Size(1000, 655);
            this.panelScroll.TabIndex = 0;
            // 
            // table
            // 
            this.table.AutoSize = true;
            this.table.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.table.ColumnCount = 2;
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.table.Dock = System.Windows.Forms.DockStyle.Top;
            this.table.Location = new System.Drawing.Point(10, 10);
            this.table.Name = "table";
            this.table.Size = new System.Drawing.Size(980, 1);
            this.table.TabIndex = 0;
            // 
            // VullDetailsForm
            // 
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.panelScroll);
            this.Controls.Add(this.panelTop);
            this.Name = "VullDetailsForm";
            this.Text = "Детали уязвимости";
            this.panelTop.ResumeLayout(false);
            this.panelScroll.ResumeLayout(false);
            this.panelScroll.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}