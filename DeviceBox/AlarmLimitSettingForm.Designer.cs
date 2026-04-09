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
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLimits)).BeginInit();
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
            this.panel1.Location = new System.Drawing.Point(0, 370);
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
            // AlarmLimitSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(38)))));
            this.ClientSize = new System.Drawing.Size(580, 425);
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
            ((System.ComponentModel.ISupportInitialize)(this.dgvLimits)).EndInit();
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
    }
}
