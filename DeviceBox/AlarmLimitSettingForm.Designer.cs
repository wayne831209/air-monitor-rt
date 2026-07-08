namespace DeviceBox
{
    partial class AlarmLimitSettingForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblFactoryName = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.dgvLimits = new System.Windows.Forms.DataGridView();
            this.colFactoryName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUpperLimit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLowerLimit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.grpNotification = new System.Windows.Forms.GroupBox();
            this.chkTeamsEnabled = new System.Windows.Forms.CheckBox();
            this.lblEmailList = new System.Windows.Forms.Label();
            this.lstEmails = new System.Windows.Forms.ListBox();
            this.btnAddEmail = new System.Windows.Forms.Button();
            this.btnRemoveEmail = new System.Windows.Forms.Button();
            this.lblCooldown = new System.Windows.Forms.Label();
            this.nudCooldown = new System.Windows.Forms.NumericUpDown();
            this.lblCooldownUnit = new System.Windows.Forms.Label();
            this.lblDelay = new System.Windows.Forms.Label();
            this.nudDelay = new System.Windows.Forms.NumericUpDown();
            this.lblDelayUnit = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.grpNotification.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLimits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCooldown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFactoryName
            // 
            this.lblFactoryName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.lblFactoryName.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFactoryName.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold);
            this.lblFactoryName.ForeColor = System.Drawing.Color.White;
            this.lblFactoryName.Location = new System.Drawing.Point(0, 0);
            this.lblFactoryName.Name = "lblFactoryName";
            this.lblFactoryName.Size = new System.Drawing.Size(580, 50);
            this.lblFactoryName.TabIndex = 0;
            this.lblFactoryName.Text = "全部設備上下限設定";
            this.lblFactoryName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("微軟正黑體", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblTitle.Location = new System.Drawing.Point(20, 55);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(540, 35);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "上下限設定";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dgvLimits
            // 
            this.dgvLimits.AllowUserToAddRows = false;
            this.dgvLimits.AllowUserToDeleteRows = false;
            this.dgvLimits.AllowUserToResizeRows = false;
            this.dgvLimits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLimits.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(38)))));
            this.dgvLimits.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvLimits.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            System.Windows.Forms.DataGridViewCellStyle columnHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            columnHeaderStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            columnHeaderStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            columnHeaderStyle.Font = new System.Drawing.Font("微軟正黑體", 11F, System.Drawing.FontStyle.Bold);
            columnHeaderStyle.ForeColor = System.Drawing.Color.White;
            columnHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            columnHeaderStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dgvLimits.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
            this.dgvLimits.ColumnHeadersHeight = 35;
            this.dgvLimits.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvLimits.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFactoryName,
            this.colUpperLimit,
            this.colLowerLimit});
            System.Windows.Forms.DataGridViewCellStyle defaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            defaultCellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(55)))));
            defaultCellStyle.Font = new System.Drawing.Font("微軟正黑體", 11F);
            defaultCellStyle.ForeColor = System.Drawing.Color.White;
            defaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            defaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dgvLimits.DefaultCellStyle = defaultCellStyle;
            this.dgvLimits.EnableHeadersVisualStyles = false;
            this.dgvLimits.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(75)))));
            this.dgvLimits.Location = new System.Drawing.Point(20, 95);
            this.dgvLimits.Name = "dgvLimits";
            this.dgvLimits.RowHeadersVisible = false;
            this.dgvLimits.RowTemplate.Height = 30;
            this.dgvLimits.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvLimits.Size = new System.Drawing.Size(540, 260);
            this.dgvLimits.TabIndex = 2;
            // 
            // colFactoryName
            // 
            this.colFactoryName.HeaderText = "設備名稱";
            this.colFactoryName.Name = "colFactoryName";
            this.colFactoryName.ReadOnly = true;
            this.colFactoryName.Width = 200;
            // 
            // colUpperLimit
            // 
            this.colUpperLimit.HeaderText = "上限值";
            this.colUpperLimit.Name = "colUpperLimit";
            this.colUpperLimit.Width = 160;
            // 
            // colLowerLimit
            // 
            this.colLowerLimit.HeaderText = "下限值";
            this.colLowerLimit.Name = "colLowerLimit";
            this.colLowerLimit.Width = 160;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnClear);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 660);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(580, 55);
            this.panel1.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold);
            this.btnOK.ForeColor = System.Drawing.Color.White;
            this.btnOK.Location = new System.Drawing.Point(120, 10);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "確定";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(85)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(240, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(80)))), ((int)(((byte)(0)))));
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold);
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.Location = new System.Drawing.Point(360, 10);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 35);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "清除";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // grpNotification
            // 
            this.grpNotification.Controls.Add(this.lblDelayUnit);
            this.grpNotification.Controls.Add(this.nudDelay);
            this.grpNotification.Controls.Add(this.lblDelay);
            this.grpNotification.Controls.Add(this.lblCooldownUnit);
            this.grpNotification.Controls.Add(this.nudCooldown);
            this.grpNotification.Controls.Add(this.lblCooldown);
            this.grpNotification.Controls.Add(this.btnRemoveEmail);
            this.grpNotification.Controls.Add(this.btnAddEmail);
            this.grpNotification.Controls.Add(this.lstEmails);
            this.grpNotification.Controls.Add(this.lblEmailList);
            this.grpNotification.Controls.Add(this.chkTeamsEnabled);
            this.grpNotification.Font = new System.Drawing.Font("微軟正黑體", 11F, System.Drawing.FontStyle.Bold);
            this.grpNotification.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpNotification.Location = new System.Drawing.Point(20, 365);
            this.grpNotification.Name = "grpNotification";
            this.grpNotification.Size = new System.Drawing.Size(540, 280);
            this.grpNotification.TabIndex = 4;
            this.grpNotification.TabStop = false;
            this.grpNotification.Text = "推播設定";
            // 
            // chkTeamsEnabled
            // 
            this.chkTeamsEnabled.AutoSize = true;
            this.chkTeamsEnabled.Font = new System.Drawing.Font("微軟正黑體", 11F);
            this.chkTeamsEnabled.Location = new System.Drawing.Point(20, 30);
            this.chkTeamsEnabled.Name = "chkTeamsEnabled";
            this.chkTeamsEnabled.Size = new System.Drawing.Size(123, 23);
            this.chkTeamsEnabled.TabIndex = 0;
            this.chkTeamsEnabled.Text = "啟用推播通知";
            this.chkTeamsEnabled.UseVisualStyleBackColor = true;
            // 
            // lblEmailList
            // 
            this.lblEmailList.AutoSize = true;
            this.lblEmailList.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.lblEmailList.Location = new System.Drawing.Point(20, 60);
            this.lblEmailList.Name = "lblEmailList";
            this.lblEmailList.Size = new System.Drawing.Size(107, 18);
            this.lblEmailList.TabIndex = 1;
            this.lblEmailList.Text = "推播人員清單:";
            // 
            // lstEmails
            // 
            this.lstEmails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(55)))));
            this.lstEmails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstEmails.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.lstEmails.ForeColor = System.Drawing.Color.White;
            this.lstEmails.FormattingEnabled = true;
            this.lstEmails.ItemHeight = 18;
            this.lstEmails.Location = new System.Drawing.Point(20, 85);
            this.lstEmails.Name = "lstEmails";
            this.lstEmails.Size = new System.Drawing.Size(380, 110);
            this.lstEmails.TabIndex = 2;
            // 
            // btnAddEmail
            // 
            this.btnAddEmail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnAddEmail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddEmail.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            this.btnAddEmail.ForeColor = System.Drawing.Color.White;
            this.btnAddEmail.Location = new System.Drawing.Point(420, 85);
            this.btnAddEmail.Name = "btnAddEmail";
            this.btnAddEmail.Size = new System.Drawing.Size(100, 35);
            this.btnAddEmail.TabIndex = 3;
            this.btnAddEmail.Text = "新增";
            this.btnAddEmail.UseVisualStyleBackColor = false;
            this.btnAddEmail.Click += new System.EventHandler(this.btnAddEmail_Click);
            // 
            // btnRemoveEmail
            // 
            this.btnRemoveEmail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(80)))), ((int)(((byte)(0)))));
            this.btnRemoveEmail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveEmail.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            this.btnRemoveEmail.ForeColor = System.Drawing.Color.White;
            this.btnRemoveEmail.Location = new System.Drawing.Point(420, 135);
            this.btnRemoveEmail.Name = "btnRemoveEmail";
            this.btnRemoveEmail.Size = new System.Drawing.Size(100, 35);
            this.btnRemoveEmail.TabIndex = 4;
            this.btnRemoveEmail.Text = "刪除";
            this.btnRemoveEmail.UseVisualStyleBackColor = false;
            this.btnRemoveEmail.Click += new System.EventHandler(this.btnRemoveEmail_Click);
            // 
            // lblCooldown
            // 
            this.lblCooldown.AutoSize = true;
            this.lblCooldown.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.lblCooldown.Location = new System.Drawing.Point(20, 210);
            this.lblCooldown.Name = "lblCooldown";
            this.lblCooldown.Size = new System.Drawing.Size(107, 18);
            this.lblCooldown.TabIndex = 5;
            this.lblCooldown.Text = "推播間隔時間:";
            // 
            // nudCooldown
            // 
            this.nudCooldown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(55)))));
            this.nudCooldown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nudCooldown.Font = new System.Drawing.Font("微軟正黑體", 11F);
            this.nudCooldown.ForeColor = System.Drawing.Color.White;
            this.nudCooldown.Location = new System.Drawing.Point(140, 207);
            this.nudCooldown.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            this.nudCooldown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudCooldown.Name = "nudCooldown";
            this.nudCooldown.Size = new System.Drawing.Size(80, 27);
            this.nudCooldown.TabIndex = 6;
            this.nudCooldown.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // lblCooldownUnit
            // 
            this.lblCooldownUnit.AutoSize = true;
            this.lblCooldownUnit.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.lblCooldownUnit.Location = new System.Drawing.Point(225, 210);
            this.lblCooldownUnit.Name = "lblCooldownUnit";
            this.lblCooldownUnit.Size = new System.Drawing.Size(37, 18);
            this.lblCooldownUnit.TabIndex = 7;
            this.lblCooldownUnit.Text = "分鐘";
            // 
            // lblDelay
            // 
            this.lblDelay.AutoSize = true;
            this.lblDelay.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.lblDelay.Location = new System.Drawing.Point(20, 245);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Size = new System.Drawing.Size(107, 18);
            this.lblDelay.TabIndex = 8;
            this.lblDelay.Text = "超限延遲時間:";
            // 
            // nudDelay
            // 
            this.nudDelay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(55)))));
            this.nudDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nudDelay.Font = new System.Drawing.Font("微軟正黑體", 11F);
            this.nudDelay.ForeColor = System.Drawing.Color.White;
            this.nudDelay.Location = new System.Drawing.Point(140, 242);
            this.nudDelay.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            this.nudDelay.Name = "nudDelay";
            this.nudDelay.Size = new System.Drawing.Size(80, 27);
            this.nudDelay.TabIndex = 9;
            this.nudDelay.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // lblDelayUnit
            // 
            this.lblDelayUnit.AutoSize = true;
            this.lblDelayUnit.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.lblDelayUnit.Location = new System.Drawing.Point(225, 245);
            this.lblDelayUnit.Name = "lblDelayUnit";
            this.lblDelayUnit.Size = new System.Drawing.Size(107, 18);
            this.lblDelayUnit.TabIndex = 10;
            this.lblDelayUnit.Text = "分鐘 (0=立即)";
            // 
            // AlarmLimitSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(38)))));
            this.ClientSize = new System.Drawing.Size(580, 715);
            this.Controls.Add(this.grpNotification);
            this.Controls.Add(this.dgvLimits);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblFactoryName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AlarmLimitSettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "上下限設定";
            this.panel1.ResumeLayout(false);
            this.grpNotification.ResumeLayout(false);
            this.grpNotification.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLimits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCooldown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblFactoryName;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.DataGridView dgvLimits;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFactoryName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpperLimit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLowerLimit;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox grpNotification;
        private System.Windows.Forms.CheckBox chkTeamsEnabled;
        private System.Windows.Forms.Label lblEmailList;
        private System.Windows.Forms.ListBox lstEmails;
        private System.Windows.Forms.Button btnAddEmail;
        private System.Windows.Forms.Button btnRemoveEmail;
        private System.Windows.Forms.Label lblCooldown;
        private System.Windows.Forms.NumericUpDown nudCooldown;
        private System.Windows.Forms.Label lblCooldownUnit;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.NumericUpDown nudDelay;
        private System.Windows.Forms.Label lblDelayUnit;
    }
}
