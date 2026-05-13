namespace Vullnerability
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelFilters = new System.Windows.Forms.Panel();
            this.chkUseDate = new System.Windows.Forms.CheckBox();
            this.btnApplyFilter = new System.Windows.Forms.Button();
            this.btnResetFilter = new System.Windows.Forms.Button();
            this.cmbOsName = new System.Windows.Forms.ComboBox();
            this.lblOsName = new System.Windows.Forms.Label();
            this.cmbFixMethod = new System.Windows.Forms.ComboBox();
            this.lblFixMethod = new System.Windows.Forms.Label();
            this.cmbExploitMethod = new System.Windows.Forms.ComboBox();
            this.lblExploitMethod = new System.Windows.Forms.Label();
            this.txtOtherId = new System.Windows.Forms.TextBox();
            this.lblOtherId = new System.Windows.Forms.Label();
            this.cmbCweType = new System.Windows.Forms.ComboBox();
            this.lblCweType = new System.Windows.Forms.Label();
            this.cmbDanger = new System.Windows.Forms.ComboBox();
            this.lblDanger = new System.Windows.Forms.Label();
            this.cmbClass = new System.Windows.Forms.ComboBox();
            this.lblClass = new System.Windows.Forms.Label();
            this.cmbYearAdded = new System.Windows.Forms.ComboBox();
            this.lblYearAdded = new System.Windows.Forms.Label();
            this.chkHasIncidents = new System.Windows.Forms.CheckBox();
            this.dtDateTo = new System.Windows.Forms.DateTimePicker();
            this.lblDateTo = new System.Windows.Forms.Label();
            this.dtDateFrom = new System.Windows.Forms.DateTimePicker();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtVersion = new System.Windows.Forms.TextBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.cmbPlatform = new System.Windows.Forms.ComboBox();
            this.lblPlatform = new System.Windows.Forms.Label();
            this.cmbProduct = new System.Windows.Forms.ComboBox();
            this.lblProduct = new System.Windows.Forms.Label();
            this.cmbProductType = new System.Windows.Forms.ComboBox();
            this.lblProductType = new System.Windows.Forms.Label();
            this.cmbVendor = new System.Windows.Forms.ComboBox();
            this.lblVendor = new System.Windows.Forms.Label();
            this.txtSearchName = new System.Windows.Forms.TextBox();
            this.lblSearchName = new System.Windows.Forms.Label();
            this.lblFilterTitle = new System.Windows.Forms.Label();
            this.panelCenter = new System.Windows.Forms.Panel();
            this.panelVullsWrap = new System.Windows.Forms.Panel();
            this.dgvVulns = new System.Windows.Forms.DataGridView();
            this.colSeverity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBduId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelTableHeader = new System.Windows.Forms.Panel();
            this.lblColDate = new System.Windows.Forms.Label();
            this.lblColDesc = new System.Windows.Forms.Label();
            this.lblColId = new System.Windows.Forms.Label();
            this.panelTopBar = new System.Windows.Forms.Panel();
            this.cmbSort = new System.Windows.Forms.ComboBox();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.cmbPageSize = new System.Windows.Forms.ComboBox();
            this.lblPageSizeLabel = new System.Windows.Forms.Label();
            this.panelBottomBar = new System.Windows.Forms.Panel();
            this.btnUpdateFromBdu = new System.Windows.Forms.Button();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.panelRight = new System.Windows.Forms.Panel();
            this.lstRecentVulns = new System.Windows.Forms.ListBox();
            this.lblLastChanges = new System.Windows.Forms.Label();
            this.panelFilters.SuspendLayout();
            this.panelCenter.SuspendLayout();
            this.panelVullsWrap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVulns)).BeginInit();
            this.panelTableHeader.SuspendLayout();
            this.panelTopBar.SuspendLayout();
            this.panelBottomBar.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelFilters
            // 
            this.panelFilters.AutoScroll = true;
            this.panelFilters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelFilters.Controls.Add(this.chkUseDate);
            this.panelFilters.Controls.Add(this.btnApplyFilter);
            this.panelFilters.Controls.Add(this.btnResetFilter);
            this.panelFilters.Controls.Add(this.cmbOsName);
            this.panelFilters.Controls.Add(this.lblOsName);
            this.panelFilters.Controls.Add(this.cmbFixMethod);
            this.panelFilters.Controls.Add(this.lblFixMethod);
            this.panelFilters.Controls.Add(this.cmbExploitMethod);
            this.panelFilters.Controls.Add(this.lblExploitMethod);
            this.panelFilters.Controls.Add(this.txtOtherId);
            this.panelFilters.Controls.Add(this.lblOtherId);
            this.panelFilters.Controls.Add(this.cmbCweType);
            this.panelFilters.Controls.Add(this.lblCweType);
            this.panelFilters.Controls.Add(this.cmbDanger);
            this.panelFilters.Controls.Add(this.lblDanger);
            this.panelFilters.Controls.Add(this.cmbClass);
            this.panelFilters.Controls.Add(this.lblClass);
            this.panelFilters.Controls.Add(this.cmbYearAdded);
            this.panelFilters.Controls.Add(this.lblYearAdded);
            this.panelFilters.Controls.Add(this.chkHasIncidents);
            this.panelFilters.Controls.Add(this.dtDateTo);
            this.panelFilters.Controls.Add(this.lblDateTo);
            this.panelFilters.Controls.Add(this.dtDateFrom);
            this.panelFilters.Controls.Add(this.cmbStatus);
            this.panelFilters.Controls.Add(this.lblStatus);
            this.panelFilters.Controls.Add(this.txtVersion);
            this.panelFilters.Controls.Add(this.lblVersion);
            this.panelFilters.Controls.Add(this.cmbPlatform);
            this.panelFilters.Controls.Add(this.lblPlatform);
            this.panelFilters.Controls.Add(this.cmbProduct);
            this.panelFilters.Controls.Add(this.lblProduct);
            this.panelFilters.Controls.Add(this.cmbProductType);
            this.panelFilters.Controls.Add(this.lblProductType);
            this.panelFilters.Controls.Add(this.cmbVendor);
            this.panelFilters.Controls.Add(this.lblVendor);
            this.panelFilters.Controls.Add(this.txtSearchName);
            this.panelFilters.Controls.Add(this.lblSearchName);
            this.panelFilters.Controls.Add(this.lblFilterTitle);
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelFilters.Location = new System.Drawing.Point(0, 0);
            this.panelFilters.Name = "panelFilters";
            this.panelFilters.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.panelFilters.Size = new System.Drawing.Size(390, 991);
            this.panelFilters.TabIndex = 0;
            // 
            // chkUseDate
            // 
            this.chkUseDate.BackColor = System.Drawing.Color.Transparent;
            this.chkUseDate.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.chkUseDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.chkUseDate.Location = new System.Drawing.Point(8, 359);
            this.chkUseDate.Name = "chkUseDate";
            this.chkUseDate.Size = new System.Drawing.Size(368, 20);
            this.chkUseDate.TabIndex = 44;
            this.chkUseDate.Text = "Диапазон дат";
            this.chkUseDate.UseVisualStyleBackColor = false;
            // 
            // btnApplyFilter
            // 
            this.btnApplyFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(55)))));
            this.btnApplyFilter.FlatAppearance.BorderSize = 0;
            this.btnApplyFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyFilter.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnApplyFilter.ForeColor = System.Drawing.Color.White;
            this.btnApplyFilter.Location = new System.Drawing.Point(128, 807);
            this.btnApplyFilter.Name = "btnApplyFilter";
            this.btnApplyFilter.Size = new System.Drawing.Size(248, 30);
            this.btnApplyFilter.TabIndex = 43;
            this.btnApplyFilter.Text = "Применить";
            this.btnApplyFilter.UseVisualStyleBackColor = false;
            this.btnApplyFilter.Click += new System.EventHandler(this.btnApplyFilter_Click);
            // 
            // btnResetFilter
            // 
            this.btnResetFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btnResetFilter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnResetFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetFilter.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnResetFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.btnResetFilter.Location = new System.Drawing.Point(8, 807);
            this.btnResetFilter.Name = "btnResetFilter";
            this.btnResetFilter.Size = new System.Drawing.Size(110, 30);
            this.btnResetFilter.TabIndex = 42;
            this.btnResetFilter.Text = "Сброс";
            this.btnResetFilter.UseVisualStyleBackColor = false;
            this.btnResetFilter.Click += new System.EventHandler(this.btnResetFilter_Click);
            // 
            // cmbOsName
            // 
            this.cmbOsName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbOsName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbOsName.BackColor = System.Drawing.Color.White;
            this.cmbOsName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbOsName.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbOsName.ForeColor = System.Drawing.Color.Black;
            this.cmbOsName.Location = new System.Drawing.Point(8, 771);
            this.cmbOsName.Name = "cmbOsName";
            this.cmbOsName.Size = new System.Drawing.Size(368, 21);
            this.cmbOsName.TabIndex = 41;
            // 
            // lblOsName
            // 
            this.lblOsName.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblOsName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblOsName.Location = new System.Drawing.Point(8, 753);
            this.lblOsName.Name = "lblOsName";
            this.lblOsName.Size = new System.Drawing.Size(368, 16);
            this.lblOsName.TabIndex = 40;
            this.lblOsName.Text = "Операционная система";
            // 
            // cmbFixMethod
            // 
            this.cmbFixMethod.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbFixMethod.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbFixMethod.BackColor = System.Drawing.Color.White;
            this.cmbFixMethod.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbFixMethod.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbFixMethod.ForeColor = System.Drawing.Color.Black;
            this.cmbFixMethod.Location = new System.Drawing.Point(8, 725);
            this.cmbFixMethod.Name = "cmbFixMethod";
            this.cmbFixMethod.Size = new System.Drawing.Size(368, 21);
            this.cmbFixMethod.TabIndex = 39;
            // 
            // lblFixMethod
            // 
            this.lblFixMethod.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblFixMethod.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblFixMethod.Location = new System.Drawing.Point(8, 707);
            this.lblFixMethod.Name = "lblFixMethod";
            this.lblFixMethod.Size = new System.Drawing.Size(368, 16);
            this.lblFixMethod.TabIndex = 38;
            this.lblFixMethod.Text = "Способ устранения";
            // 
            // cmbExploitMethod
            // 
            this.cmbExploitMethod.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbExploitMethod.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbExploitMethod.BackColor = System.Drawing.Color.White;
            this.cmbExploitMethod.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbExploitMethod.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbExploitMethod.ForeColor = System.Drawing.Color.Black;
            this.cmbExploitMethod.Location = new System.Drawing.Point(8, 679);
            this.cmbExploitMethod.Name = "cmbExploitMethod";
            this.cmbExploitMethod.Size = new System.Drawing.Size(368, 21);
            this.cmbExploitMethod.TabIndex = 37;
            // 
            // lblExploitMethod
            // 
            this.lblExploitMethod.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblExploitMethod.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblExploitMethod.Location = new System.Drawing.Point(8, 661);
            this.lblExploitMethod.Name = "lblExploitMethod";
            this.lblExploitMethod.Size = new System.Drawing.Size(368, 16);
            this.lblExploitMethod.TabIndex = 36;
            this.lblExploitMethod.Text = "Способ эксплуатации";
            // 
            // txtOtherId
            // 
            this.txtOtherId.BackColor = System.Drawing.Color.White;
            this.txtOtherId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOtherId.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtOtherId.ForeColor = System.Drawing.Color.Black;
            this.txtOtherId.Location = new System.Drawing.Point(8, 633);
            this.txtOtherId.Name = "txtOtherId";
            this.txtOtherId.Size = new System.Drawing.Size(368, 23);
            this.txtOtherId.TabIndex = 35;
            // 
            // lblOtherId
            // 
            this.lblOtherId.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblOtherId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblOtherId.Location = new System.Drawing.Point(8, 615);
            this.lblOtherId.Name = "lblOtherId";
            this.lblOtherId.Size = new System.Drawing.Size(368, 16);
            this.lblOtherId.TabIndex = 34;
            this.lblOtherId.Text = "Другие системы идентификации";
            // 
            // cmbCweType
            // 
            this.cmbCweType.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbCweType.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbCweType.BackColor = System.Drawing.Color.White;
            this.cmbCweType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbCweType.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbCweType.ForeColor = System.Drawing.Color.Black;
            this.cmbCweType.Location = new System.Drawing.Point(8, 587);
            this.cmbCweType.Name = "cmbCweType";
            this.cmbCweType.Size = new System.Drawing.Size(368, 21);
            this.cmbCweType.TabIndex = 33;
            // 
            // lblCweType
            // 
            this.lblCweType.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblCweType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblCweType.Location = new System.Drawing.Point(8, 569);
            this.lblCweType.Name = "lblCweType";
            this.lblCweType.Size = new System.Drawing.Size(368, 16);
            this.lblCweType.TabIndex = 32;
            this.lblCweType.Text = "Идентификатор типа ошибки";
            // 
            // cmbDanger
            // 
            this.cmbDanger.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbDanger.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbDanger.BackColor = System.Drawing.Color.White;
            this.cmbDanger.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDanger.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbDanger.ForeColor = System.Drawing.Color.Black;
            this.cmbDanger.Location = new System.Drawing.Point(8, 540);
            this.cmbDanger.Name = "cmbDanger";
            this.cmbDanger.Size = new System.Drawing.Size(368, 21);
            this.cmbDanger.TabIndex = 27;
            // 
            // lblDanger
            // 
            this.lblDanger.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblDanger.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblDanger.Location = new System.Drawing.Point(8, 522);
            this.lblDanger.Name = "lblDanger";
            this.lblDanger.Size = new System.Drawing.Size(368, 16);
            this.lblDanger.TabIndex = 26;
            this.lblDanger.Text = "Уровень опасности";
            // 
            // cmbClass
            // 
            this.cmbClass.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbClass.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbClass.BackColor = System.Drawing.Color.White;
            this.cmbClass.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbClass.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbClass.ForeColor = System.Drawing.Color.Black;
            this.cmbClass.Location = new System.Drawing.Point(8, 494);
            this.cmbClass.Name = "cmbClass";
            this.cmbClass.Size = new System.Drawing.Size(368, 21);
            this.cmbClass.TabIndex = 25;
            // 
            // lblClass
            // 
            this.lblClass.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblClass.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblClass.Location = new System.Drawing.Point(8, 476);
            this.lblClass.Name = "lblClass";
            this.lblClass.Size = new System.Drawing.Size(368, 16);
            this.lblClass.TabIndex = 24;
            this.lblClass.Text = "Класс уязвимости";
            // 
            // cmbYearAdded
            // 
            this.cmbYearAdded.BackColor = System.Drawing.Color.White;
            this.cmbYearAdded.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbYearAdded.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbYearAdded.ForeColor = System.Drawing.Color.Black;
            this.cmbYearAdded.Location = new System.Drawing.Point(8, 448);
            this.cmbYearAdded.Name = "cmbYearAdded";
            this.cmbYearAdded.Size = new System.Drawing.Size(140, 21);
            this.cmbYearAdded.TabIndex = 23;
            // 
            // lblYearAdded
            // 
            this.lblYearAdded.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblYearAdded.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblYearAdded.Location = new System.Drawing.Point(8, 430);
            this.lblYearAdded.Name = "lblYearAdded";
            this.lblYearAdded.Size = new System.Drawing.Size(368, 16);
            this.lblYearAdded.TabIndex = 22;
            this.lblYearAdded.Text = "Год добавления";
            // 
            // chkHasIncidents
            // 
            this.chkHasIncidents.BackColor = System.Drawing.Color.Transparent;
            this.chkHasIncidents.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.chkHasIncidents.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.chkHasIncidents.Location = new System.Drawing.Point(8, 407);
            this.chkHasIncidents.Name = "chkHasIncidents";
            this.chkHasIncidents.Size = new System.Drawing.Size(368, 20);
            this.chkHasIncidents.TabIndex = 20;
            this.chkHasIncidents.Text = "Уязвимости, связанные с инцидентами ИБ";
            this.chkHasIncidents.UseVisualStyleBackColor = false;
            // 
            // dtDateTo
            // 
            this.dtDateTo.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dtDateTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtDateTo.Location = new System.Drawing.Point(204, 379);
            this.dtDateTo.Name = "dtDateTo";
            this.dtDateTo.Size = new System.Drawing.Size(172, 23);
            this.dtDateTo.TabIndex = 19;
            // 
            // lblDateTo
            // 
            this.lblDateTo.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblDateTo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblDateTo.Location = new System.Drawing.Point(180, 382);
            this.lblDateTo.Name = "lblDateTo";
            this.lblDateTo.Size = new System.Drawing.Size(22, 16);
            this.lblDateTo.TabIndex = 18;
            this.lblDateTo.Text = "по";
            // 
            // dtDateFrom
            // 
            this.dtDateFrom.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dtDateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtDateFrom.Location = new System.Drawing.Point(8, 379);
            this.dtDateFrom.Name = "dtDateFrom";
            this.dtDateFrom.Size = new System.Drawing.Size(168, 23);
            this.dtDateFrom.TabIndex = 17;
            // 
            // cmbStatus
            // 
            this.cmbStatus.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbStatus.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbStatus.BackColor = System.Drawing.Color.White;
            this.cmbStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbStatus.ForeColor = System.Drawing.Color.Black;
            this.cmbStatus.Location = new System.Drawing.Point(8, 332);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Size = new System.Drawing.Size(368, 21);
            this.cmbStatus.TabIndex = 14;
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblStatus.Location = new System.Drawing.Point(8, 314);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(368, 16);
            this.lblStatus.TabIndex = 13;
            this.lblStatus.Text = "Статус уязвимости";
            // 
            // txtVersion
            // 
            this.txtVersion.BackColor = System.Drawing.Color.White;
            this.txtVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtVersion.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtVersion.ForeColor = System.Drawing.Color.Black;
            this.txtVersion.Location = new System.Drawing.Point(8, 286);
            this.txtVersion.Name = "txtVersion";
            this.txtVersion.Size = new System.Drawing.Size(368, 23);
            this.txtVersion.TabIndex = 12;
            // 
            // lblVersion
            // 
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblVersion.Location = new System.Drawing.Point(8, 268);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(368, 16);
            this.lblVersion.TabIndex = 11;
            this.lblVersion.Text = "Версия ПО";
            // 
            // cmbPlatform
            // 
            this.cmbPlatform.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbPlatform.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbPlatform.BackColor = System.Drawing.Color.White;
            this.cmbPlatform.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbPlatform.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbPlatform.ForeColor = System.Drawing.Color.Black;
            this.cmbPlatform.Location = new System.Drawing.Point(8, 240);
            this.cmbPlatform.Name = "cmbPlatform";
            this.cmbPlatform.Size = new System.Drawing.Size(368, 21);
            this.cmbPlatform.TabIndex = 10;
            // 
            // lblPlatform
            // 
            this.lblPlatform.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPlatform.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblPlatform.Location = new System.Drawing.Point(8, 222);
            this.lblPlatform.Name = "lblPlatform";
            this.lblPlatform.Size = new System.Drawing.Size(368, 16);
            this.lblPlatform.TabIndex = 9;
            this.lblPlatform.Text = "Аппаратная платформа";
            // 
            // cmbProduct
            // 
            this.cmbProduct.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbProduct.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbProduct.BackColor = System.Drawing.Color.White;
            this.cmbProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbProduct.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbProduct.ForeColor = System.Drawing.Color.Black;
            this.cmbProduct.Location = new System.Drawing.Point(8, 194);
            this.cmbProduct.Name = "cmbProduct";
            this.cmbProduct.Size = new System.Drawing.Size(368, 21);
            this.cmbProduct.TabIndex = 8;
            // 
            // lblProduct
            // 
            this.lblProduct.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblProduct.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblProduct.Location = new System.Drawing.Point(8, 176);
            this.lblProduct.Name = "lblProduct";
            this.lblProduct.Size = new System.Drawing.Size(368, 16);
            this.lblProduct.TabIndex = 7;
            this.lblProduct.Text = "Программное обеспечение";
            // 
            // cmbProductType
            // 
            this.cmbProductType.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbProductType.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbProductType.BackColor = System.Drawing.Color.White;
            this.cmbProductType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbProductType.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbProductType.ForeColor = System.Drawing.Color.Black;
            this.cmbProductType.Location = new System.Drawing.Point(8, 148);
            this.cmbProductType.Name = "cmbProductType";
            this.cmbProductType.Size = new System.Drawing.Size(368, 21);
            this.cmbProductType.TabIndex = 6;
            // 
            // lblProductType
            // 
            this.lblProductType.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblProductType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblProductType.Location = new System.Drawing.Point(8, 130);
            this.lblProductType.Name = "lblProductType";
            this.lblProductType.Size = new System.Drawing.Size(368, 16);
            this.lblProductType.TabIndex = 5;
            this.lblProductType.Text = "Тип ПО";
            // 
            // cmbVendor
            // 
            this.cmbVendor.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbVendor.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbVendor.BackColor = System.Drawing.Color.White;
            this.cmbVendor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbVendor.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbVendor.ForeColor = System.Drawing.Color.Black;
            this.cmbVendor.Location = new System.Drawing.Point(8, 102);
            this.cmbVendor.Name = "cmbVendor";
            this.cmbVendor.Size = new System.Drawing.Size(368, 21);
            this.cmbVendor.TabIndex = 4;
            // 
            // lblVendor
            // 
            this.lblVendor.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblVendor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblVendor.Location = new System.Drawing.Point(8, 84);
            this.lblVendor.Name = "lblVendor";
            this.lblVendor.Size = new System.Drawing.Size(368, 16);
            this.lblVendor.TabIndex = 3;
            this.lblVendor.Text = "Производитель ПО";
            // 
            // txtSearchName
            // 
            this.txtSearchName.BackColor = System.Drawing.Color.White;
            this.txtSearchName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearchName.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtSearchName.ForeColor = System.Drawing.Color.Black;
            this.txtSearchName.Location = new System.Drawing.Point(8, 56);
            this.txtSearchName.Name = "txtSearchName";
            this.txtSearchName.Size = new System.Drawing.Size(368, 23);
            this.txtSearchName.TabIndex = 2;
            // 
            // lblSearchName
            // 
            this.lblSearchName.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblSearchName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblSearchName.Location = new System.Drawing.Point(8, 38);
            this.lblSearchName.Name = "lblSearchName";
            this.lblSearchName.Size = new System.Drawing.Size(368, 16);
            this.lblSearchName.TabIndex = 1;
            this.lblSearchName.Text = "Контекстный поиск по названию уязвимости";
            // 
            // lblFilterTitle
            // 
            this.lblFilterTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblFilterTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(180)))), ((int)(((byte)(100)))));
            this.lblFilterTitle.Location = new System.Drawing.Point(0, 8);
            this.lblFilterTitle.Name = "lblFilterTitle";
            this.lblFilterTitle.Size = new System.Drawing.Size(368, 22);
            this.lblFilterTitle.TabIndex = 0;
            this.lblFilterTitle.Text = "ФИЛЬТРАЦИЯ";
            this.lblFilterTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelCenter
            // 
            this.panelCenter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.panelCenter.Controls.Add(this.panelVullsWrap);
            this.panelCenter.Controls.Add(this.panelBottomBar);
            this.panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCenter.Location = new System.Drawing.Point(390, 0);
            this.panelCenter.Name = "panelCenter";
            this.panelCenter.Size = new System.Drawing.Size(1242, 991);
            this.panelCenter.TabIndex = 1;
            // 
            // panelVullsWrap
            // 
            this.panelVullsWrap.Controls.Add(this.dgvVulns);
            this.panelVullsWrap.Controls.Add(this.panelTableHeader);
            this.panelVullsWrap.Controls.Add(this.panelTopBar);
            this.panelVullsWrap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelVullsWrap.Location = new System.Drawing.Point(0, 0);
            this.panelVullsWrap.Name = "panelVullsWrap";
            this.panelVullsWrap.Size = new System.Drawing.Size(1242, 957);
            this.panelVullsWrap.TabIndex = 0;
            // 
            // dgvVulns
            // 
            this.dgvVulns.AllowUserToAddRows = false;
            this.dgvVulns.AllowUserToDeleteRows = false;
            this.dgvVulns.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.dgvVulns.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvVulns.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvVulns.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.dgvVulns.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvVulns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVulns.ColumnHeadersVisible = false;
            this.dgvVulns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSeverity,
            this.colBduId,
            this.colDesc,
            this.colDate});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(22)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            dataGridViewCellStyle5.Padding = new System.Windows.Forms.Padding(4, 6, 4, 6);
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(70)))), ((int)(((byte)(40)))));
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvVulns.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvVulns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVulns.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dgvVulns.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.dgvVulns.Location = new System.Drawing.Point(0, 66);
            this.dgvVulns.MultiSelect = false;
            this.dgvVulns.Name = "dgvVulns";
            this.dgvVulns.ReadOnly = true;
            this.dgvVulns.RowHeadersVisible = false;
            this.dgvVulns.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvVulns.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvVulns.Size = new System.Drawing.Size(1242, 891);
            this.dgvVulns.TabIndex = 0;
            this.dgvVulns.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVulns_CellDoubleClick);
            // 
            // colSeverity
            // 
            this.colSeverity.HeaderText = "";
            this.colSeverity.MinimumWidth = 6;
            this.colSeverity.Name = "colSeverity";
            this.colSeverity.ReadOnly = true;
            this.colSeverity.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colSeverity.Width = 6;
            // 
            // colBduId
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(180)))), ((int)(((byte)(100)))));
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(8, 6, 4, 6);
            this.colBduId.DefaultCellStyle = dataGridViewCellStyle2;
            this.colBduId.HeaderText = "Идентификатор";
            this.colBduId.MinimumWidth = 100;
            this.colBduId.Name = "colBduId";
            this.colBduId.ReadOnly = true;
            this.colBduId.Width = 148;
            // 
            // colDesc
            // 
            this.colDesc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.colDesc.DefaultCellStyle = dataGridViewCellStyle3;
            this.colDesc.HeaderText = "Наименование / описание уязвимости";
            this.colDesc.MinimumWidth = 300;
            this.colDesc.Name = "colDesc";
            this.colDesc.ReadOnly = true;
            // 
            // colDate
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.colDate.DefaultCellStyle = dataGridViewCellStyle4;
            this.colDate.HeaderText = "Дата";
            this.colDate.MinimumWidth = 70;
            this.colDate.Name = "colDate";
            this.colDate.ReadOnly = true;
            this.colDate.Width = 90;
            // 
            // panelTableHeader
            // 
            this.panelTableHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.panelTableHeader.Controls.Add(this.lblColDate);
            this.panelTableHeader.Controls.Add(this.lblColDesc);
            this.panelTableHeader.Controls.Add(this.lblColId);
            this.panelTableHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTableHeader.Location = new System.Drawing.Point(0, 38);
            this.panelTableHeader.Name = "panelTableHeader";
            this.panelTableHeader.Size = new System.Drawing.Size(1242, 28);
            this.panelTableHeader.TabIndex = 1;
            // 
            // lblColDate
            // 
            this.lblColDate.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblColDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.lblColDate.Location = new System.Drawing.Point(742, 6);
            this.lblColDate.Name = "lblColDate";
            this.lblColDate.Size = new System.Drawing.Size(80, 18);
            this.lblColDate.TabIndex = 2;
            this.lblColDate.Text = "Дата";
            // 
            // lblColDesc
            // 
            this.lblColDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblColDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.lblColDesc.Location = new System.Drawing.Point(155, 6);
            this.lblColDesc.Name = "lblColDesc";
            this.lblColDesc.Size = new System.Drawing.Size(580, 18);
            this.lblColDesc.TabIndex = 1;
            this.lblColDesc.Text = "Наименование / описание уязвимости";
            // 
            // lblColId
            // 
            this.lblColId.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblColId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.lblColId.Location = new System.Drawing.Point(8, 6);
            this.lblColId.Name = "lblColId";
            this.lblColId.Size = new System.Drawing.Size(140, 18);
            this.lblColId.TabIndex = 0;
            this.lblColId.Text = "Идентификатор";
            // 
            // panelTopBar
            // 
            this.panelTopBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.panelTopBar.Controls.Add(this.cmbSort);
            this.panelTopBar.Controls.Add(this.lblPageInfo);
            this.panelTopBar.Controls.Add(this.cmbPageSize);
            this.panelTopBar.Controls.Add(this.lblPageSizeLabel);
            this.panelTopBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTopBar.Location = new System.Drawing.Point(0, 0);
            this.panelTopBar.Name = "panelTopBar";
            this.panelTopBar.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.panelTopBar.Size = new System.Drawing.Size(1242, 38);
            this.panelTopBar.TabIndex = 0;
            // 
            // cmbSort
            // 
            this.cmbSort.BackColor = System.Drawing.Color.White;
            this.cmbSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbSort.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbSort.ForeColor = System.Drawing.Color.Black;
            this.cmbSort.Location = new System.Drawing.Point(1038, 9);
            this.cmbSort.Name = "cmbSort";
            this.cmbSort.Size = new System.Drawing.Size(160, 21);
            this.cmbSort.TabIndex = 1;
            this.cmbSort.SelectedIndexChanged += new System.EventHandler(this.cmbSort_SelectedIndexChanged);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPageInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.lblPageInfo.Location = new System.Drawing.Point(195, 11);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(380, 18);
            this.lblPageInfo.TabIndex = 0;
            this.lblPageInfo.Text = "Элементы с 1 по 50 из 0";
            // 
            // cmbPageSize
            // 
            this.cmbPageSize.BackColor = System.Drawing.Color.White;
            this.cmbPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPageSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbPageSize.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmbPageSize.ForeColor = System.Drawing.Color.Black;
            this.cmbPageSize.Items.AddRange(new object[] {
            "10",
            "50",
            "100"});
            this.cmbPageSize.Location = new System.Drawing.Point(100, 8);
            this.cmbPageSize.Name = "cmbPageSize";
            this.cmbPageSize.Size = new System.Drawing.Size(85, 21);
            this.cmbPageSize.TabIndex = 0;
            this.cmbPageSize.SelectedIndexChanged += new System.EventHandler(this.cmbPageSize_SelectedIndexChanged);
            // 
            // lblPageSizeLabel
            // 
            this.lblPageSizeLabel.AutoSize = true;
            this.lblPageSizeLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPageSizeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.lblPageSizeLabel.Location = new System.Drawing.Point(8, 12);
            this.lblPageSizeLabel.Name = "lblPageSizeLabel";
            this.lblPageSizeLabel.Size = new System.Drawing.Size(80, 15);
            this.lblPageSizeLabel.TabIndex = 0;
            this.lblPageSizeLabel.Text = "Выводить по:";
            // 
            // panelBottomBar
            // 
            this.panelBottomBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.panelBottomBar.Controls.Add(this.btnUpdateFromBdu);
            this.panelBottomBar.Controls.Add(this.btnPrevPage);
            this.panelBottomBar.Controls.Add(this.btnNextPage);
            this.panelBottomBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottomBar.Location = new System.Drawing.Point(0, 957);
            this.panelBottomBar.Name = "panelBottomBar";
            this.panelBottomBar.Size = new System.Drawing.Size(1242, 34);
            this.panelBottomBar.TabIndex = 4;
            // 
            // btnUpdateFromBdu
            // 
            this.btnUpdateFromBdu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(55)))));
            this.btnUpdateFromBdu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnUpdateFromBdu.FlatAppearance.BorderSize = 0;
            this.btnUpdateFromBdu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdateFromBdu.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnUpdateFromBdu.ForeColor = System.Drawing.Color.White;
            this.btnUpdateFromBdu.Location = new System.Drawing.Point(44, 0);
            this.btnUpdateFromBdu.Name = "btnUpdateFromBdu";
            this.btnUpdateFromBdu.Size = new System.Drawing.Size(1154, 34);
            this.btnUpdateFromBdu.TabIndex = 1;
            this.btnUpdateFromBdu.Text = "Обновить из БДУ";
            this.btnUpdateFromBdu.UseVisualStyleBackColor = false;
            this.btnUpdateFromBdu.Click += new System.EventHandler(this.btnUpdateFromBdu_Click);
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.BackColor = System.Drawing.Color.White;
            this.btnPrevPage.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPrevPage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.btnPrevPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevPage.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnPrevPage.ForeColor = System.Drawing.Color.Black;
            this.btnPrevPage.Location = new System.Drawing.Point(0, 0);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(44, 34);
            this.btnPrevPage.TabIndex = 2;
            this.btnPrevPage.Text = "<<";
            this.btnPrevPage.UseVisualStyleBackColor = false;
            this.btnPrevPage.Click += new System.EventHandler(this.btnPrevPage_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.BackColor = System.Drawing.Color.White;
            this.btnNextPage.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnNextPage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.btnNextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextPage.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnNextPage.ForeColor = System.Drawing.Color.Black;
            this.btnNextPage.Location = new System.Drawing.Point(1198, 0);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(44, 34);
            this.btnNextPage.TabIndex = 3;
            this.btnNextPage.Text = ">>";
            this.btnNextPage.UseVisualStyleBackColor = false;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // panelRight
            // 
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.panelRight.Controls.Add(this.lstRecentVulns);
            this.panelRight.Controls.Add(this.lblLastChanges);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRight.Location = new System.Drawing.Point(1632, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(270, 991);
            this.panelRight.TabIndex = 2;
            // 
            // lstRecentVulns
            // 
            this.lstRecentVulns.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.lstRecentVulns.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstRecentVulns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstRecentVulns.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstRecentVulns.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstRecentVulns.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lstRecentVulns.FormattingEnabled = true;
            this.lstRecentVulns.ItemHeight = 44;
            this.lstRecentVulns.Location = new System.Drawing.Point(0, 30);
            this.lstRecentVulns.Name = "lstRecentVulns";
            this.lstRecentVulns.Size = new System.Drawing.Size(270, 961);
            this.lstRecentVulns.TabIndex = 0;
            // 
            // lblLastChanges
            // 
            this.lblLastChanges.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblLastChanges.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLastChanges.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblLastChanges.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(180)))), ((int)(((byte)(100)))));
            this.lblLastChanges.Location = new System.Drawing.Point(0, 0);
            this.lblLastChanges.Name = "lblLastChanges";
            this.lblLastChanges.Size = new System.Drawing.Size(270, 30);
            this.lblLastChanges.TabIndex = 0;
            this.lblLastChanges.Text = "ПОСЛЕДНИЕ ИЗМЕНЕНИЯ";
            this.lblLastChanges.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.ClientSize = new System.Drawing.Size(1902, 991);
            this.Controls.Add(this.panelCenter);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelFilters);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(1918, 1030);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Справочник уязвимостей";
            this.panelFilters.ResumeLayout(false);
            this.panelFilters.PerformLayout();
            this.panelCenter.ResumeLayout(false);
            this.panelVullsWrap.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvVulns)).EndInit();
            this.panelTableHeader.ResumeLayout(false);
            this.panelTopBar.ResumeLayout(false);
            this.panelTopBar.PerformLayout();
            this.panelBottomBar.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel panelFilters;
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelTopBar;
        private System.Windows.Forms.Panel panelTableHeader;
        private System.Windows.Forms.Panel panelVullsWrap;
        private System.Windows.Forms.Label lblPageSizeLabel;
        private System.Windows.Forms.ComboBox cmbPageSize;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.ComboBox cmbSort;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Label lblColId;
        private System.Windows.Forms.Label lblColDesc;
        private System.Windows.Forms.Label lblColDate;
        private System.Windows.Forms.DataGridView dgvVulns;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSeverity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBduId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDesc;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
        private System.Windows.Forms.Button btnUpdateFromBdu;
        private System.Windows.Forms.Label lblLastChanges;
        private System.Windows.Forms.ListBox lstRecentVulns;
        private System.Windows.Forms.Label lblFilterTitle;
        private System.Windows.Forms.Label lblSearchName;
        private System.Windows.Forms.TextBox txtSearchName;
        private System.Windows.Forms.Label lblVendor;
        private System.Windows.Forms.ComboBox cmbVendor;
        private System.Windows.Forms.Label lblProductType;
        private System.Windows.Forms.ComboBox cmbProductType;
        private System.Windows.Forms.Label lblProduct;
        private System.Windows.Forms.ComboBox cmbProduct;
        private System.Windows.Forms.Label lblPlatform;
        private System.Windows.Forms.ComboBox cmbPlatform;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.TextBox txtVersion;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.DateTimePicker dtDateFrom;
        private System.Windows.Forms.Label lblDateTo;
        private System.Windows.Forms.DateTimePicker dtDateTo;
        private System.Windows.Forms.CheckBox chkHasIncidents;
        private System.Windows.Forms.Label lblYearAdded;
        private System.Windows.Forms.ComboBox cmbYearAdded;
        private System.Windows.Forms.Label lblClass;
        private System.Windows.Forms.ComboBox cmbClass;
        private System.Windows.Forms.Label lblDanger;
        private System.Windows.Forms.ComboBox cmbDanger;
        private System.Windows.Forms.Label lblCweType;
        private System.Windows.Forms.ComboBox cmbCweType;
        private System.Windows.Forms.Label lblOtherId;
        private System.Windows.Forms.TextBox txtOtherId;
        private System.Windows.Forms.Label lblExploitMethod;
        private System.Windows.Forms.ComboBox cmbExploitMethod;
        private System.Windows.Forms.Label lblFixMethod;
        private System.Windows.Forms.ComboBox cmbFixMethod;
        private System.Windows.Forms.Label lblOsName;
        private System.Windows.Forms.ComboBox cmbOsName;
        private System.Windows.Forms.Button btnResetFilter;
        private System.Windows.Forms.Button btnApplyFilter;
        private System.Windows.Forms.Panel panelBottomBar;
        private System.Windows.Forms.CheckBox chkUseDate;
    }
}