namespace DeviceBox
{
    partial class ScheduleEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelFormTitle = new System.Windows.Forms.Label();
            this.panelEnable = new System.Windows.Forms.Panel();
            this.labelEnable = new System.Windows.Forms.Label();
            this.panelDevice = new System.Windows.Forms.Panel();
            this.comboBoxDevice = new System.Windows.Forms.ComboBox();
            this.labelDevice = new System.Windows.Forms.Label();
            this.comboBoxFactory = new System.Windows.Forms.ComboBox();
            this.labelFactory = new System.Windows.Forms.Label();
            this.labelDeviceSection = new System.Windows.Forms.Label();
            this.panelTime = new System.Windows.Forms.Panel();
            this.labelDuration = new System.Windows.Forms.Label();
            this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.labelEnd = new System.Windows.Forms.Label();
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.labelStart = new System.Windows.Forms.Label();
            this.labelTimeSection = new System.Windows.Forms.Label();
            this.panelDays = new System.Windows.Forms.Panel();
            this.labelDaysSection = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonConfirm = new System.Windows.Forms.Button();
            this.panelEnable.SuspendLayout();
            this.panelDevice.SuspendLayout();
            this.panelTime.SuspendLayout();
            this.panelDays.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelFormTitle
            // 
            this.labelFormTitle.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelFormTitle.ForeColor = System.Drawing.Color.White;
            this.labelFormTitle.Location = new System.Drawing.Point(20, 15);
            this.labelFormTitle.Name = "labelFormTitle";
            this.labelFormTitle.Size = new System.Drawing.Size(360, 40);
            this.labelFormTitle.TabIndex = 0;
            this.labelFormTitle.Text = "新增排程";
            // 
            // panelEnable
            // 
            this.panelEnable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelEnable.Controls.Add(this.labelEnable);
            this.panelEnable.Location = new System.Drawing.Point(20, 60);
            this.panelEnable.Name = "panelEnable";
            this.panelEnable.Size = new System.Drawing.Size(360, 60);
            this.panelEnable.TabIndex = 1;
            // 
            // labelEnable
            // 
            this.labelEnable.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelEnable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelEnable.Location = new System.Drawing.Point(15, 8);
            this.labelEnable.Name = "labelEnable";
            this.labelEnable.Size = new System.Drawing.Size(100, 20);
            this.labelEnable.TabIndex = 0;
            this.labelEnable.Text = "啟用排程";
            // 
            // panelDevice
            // 
            this.panelDevice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelDevice.Controls.Add(this.comboBoxDevice);
            this.panelDevice.Controls.Add(this.labelDevice);
            this.panelDevice.Controls.Add(this.comboBoxFactory);
            this.panelDevice.Controls.Add(this.labelFactory);
            this.panelDevice.Controls.Add(this.labelDeviceSection);
            this.panelDevice.Location = new System.Drawing.Point(20, 130);
            this.panelDevice.Name = "panelDevice";
            this.panelDevice.Size = new System.Drawing.Size(360, 110);
            this.panelDevice.TabIndex = 2;
            // 
            // comboBoxDevice
            // 
            this.comboBoxDevice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(65)))));
            this.comboBoxDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDevice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxDevice.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.comboBoxDevice.ForeColor = System.Drawing.Color.White;
            this.comboBoxDevice.FormattingEnabled = true;
            this.comboBoxDevice.Location = new System.Drawing.Point(80, 70);
            this.comboBoxDevice.Name = "comboBoxDevice";
            this.comboBoxDevice.Size = new System.Drawing.Size(260, 25);
            this.comboBoxDevice.TabIndex = 4;
            // 
            // labelDevice
            // 
            this.labelDevice.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.labelDevice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelDevice.Location = new System.Drawing.Point(15, 73);
            this.labelDevice.Name = "labelDevice";
            this.labelDevice.Size = new System.Drawing.Size(60, 25);
            this.labelDevice.TabIndex = 3;
            this.labelDevice.Text = "設備";
            // 
            // comboBoxFactory
            // 
            this.comboBoxFactory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(65)))));
            this.comboBoxFactory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFactory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxFactory.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.comboBoxFactory.ForeColor = System.Drawing.Color.White;
            this.comboBoxFactory.FormattingEnabled = true;
            this.comboBoxFactory.Location = new System.Drawing.Point(80, 35);
            this.comboBoxFactory.Name = "comboBoxFactory";
            this.comboBoxFactory.Size = new System.Drawing.Size(260, 25);
            this.comboBoxFactory.TabIndex = 2;
            // 
            // labelFactory
            // 
            this.labelFactory.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.labelFactory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelFactory.Location = new System.Drawing.Point(15, 38);
            this.labelFactory.Name = "labelFactory";
            this.labelFactory.Size = new System.Drawing.Size(60, 25);
            this.labelFactory.TabIndex = 1;
            this.labelFactory.Text = "工廠";
            // 
            // labelDeviceSection
            // 
            this.labelDeviceSection.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelDeviceSection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelDeviceSection.Location = new System.Drawing.Point(15, 8);
            this.labelDeviceSection.Name = "labelDeviceSection";
            this.labelDeviceSection.Size = new System.Drawing.Size(100, 20);
            this.labelDeviceSection.TabIndex = 0;
            this.labelDeviceSection.Text = "選擇設備";
            // 
            // panelTime
            // 
            this.panelTime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelTime.Controls.Add(this.labelDuration);
            this.panelTime.Controls.Add(this.dateTimePickerEnd);
            this.panelTime.Controls.Add(this.labelEnd);
            this.panelTime.Controls.Add(this.dateTimePickerStart);
            this.panelTime.Controls.Add(this.labelStart);
            this.panelTime.Controls.Add(this.labelTimeSection);
            this.panelTime.Location = new System.Drawing.Point(20, 250);
            this.panelTime.Name = "panelTime";
            this.panelTime.Size = new System.Drawing.Size(360, 110);
            this.panelTime.TabIndex = 3;
            // 
            // labelDuration
            // 
            this.labelDuration.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.labelDuration.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(199)))), ((int)(((byte)(89)))));
            this.labelDuration.Location = new System.Drawing.Point(220, 50);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(120, 30);
            this.labelDuration.TabIndex = 5;
            this.labelDuration.Text = "運轉 9.0 小時";
            this.labelDuration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.CalendarForeColor = System.Drawing.Color.White;
            this.dateTimePickerEnd.CustomFormat = "HH:mm";
            this.dateTimePickerEnd.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.dateTimePickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEnd.Location = new System.Drawing.Point(80, 68);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.ShowUpDown = true;
            this.dateTimePickerEnd.Size = new System.Drawing.Size(120, 33);
            this.dateTimePickerEnd.TabIndex = 4;
            // 
            // labelEnd
            // 
            this.labelEnd.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.labelEnd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelEnd.Location = new System.Drawing.Point(15, 73);
            this.labelEnd.Name = "labelEnd";
            this.labelEnd.Size = new System.Drawing.Size(60, 25);
            this.labelEnd.TabIndex = 3;
            this.labelEnd.Text = "結束";
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.CalendarForeColor = System.Drawing.Color.White;
            this.dateTimePickerStart.CustomFormat = "HH:mm";
            this.dateTimePickerStart.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.dateTimePickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStart.Location = new System.Drawing.Point(80, 30);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.ShowUpDown = true;
            this.dateTimePickerStart.Size = new System.Drawing.Size(120, 33);
            this.dateTimePickerStart.TabIndex = 2;
            // 
            // labelStart
            // 
            this.labelStart.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.labelStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelStart.Location = new System.Drawing.Point(15, 38);
            this.labelStart.Name = "labelStart";
            this.labelStart.Size = new System.Drawing.Size(60, 25);
            this.labelStart.TabIndex = 1;
            this.labelStart.Text = "開始";
            // 
            // labelTimeSection
            // 
            this.labelTimeSection.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelTimeSection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelTimeSection.Location = new System.Drawing.Point(15, 8);
            this.labelTimeSection.Name = "labelTimeSection";
            this.labelTimeSection.Size = new System.Drawing.Size(100, 20);
            this.labelTimeSection.TabIndex = 0;
            this.labelTimeSection.Text = "時間設定";
            // 
            // panelDays
            // 
            this.panelDays.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelDays.Controls.Add(this.labelDaysSection);
            this.panelDays.Location = new System.Drawing.Point(20, 370);
            this.panelDays.Name = "panelDays";
            this.panelDays.Size = new System.Drawing.Size(360, 90);
            this.panelDays.TabIndex = 4;
            // 
            // labelDaysSection
            // 
            this.labelDaysSection.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelDaysSection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelDaysSection.Location = new System.Drawing.Point(15, 8);
            this.labelDaysSection.Name = "labelDaysSection";
            this.labelDaysSection.Size = new System.Drawing.Size(100, 20);
            this.labelDaysSection.TabIndex = 0;
            this.labelDaysSection.Text = "重複";
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.buttonCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonCancel.FlatAppearance.BorderSize = 0;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(20, 480);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(170, 45);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = false;
            // 
            // buttonConfirm
            // 
            this.buttonConfirm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.buttonConfirm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonConfirm.FlatAppearance.BorderSize = 0;
            this.buttonConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConfirm.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.buttonConfirm.ForeColor = System.Drawing.Color.White;
            this.buttonConfirm.Location = new System.Drawing.Point(210, 480);
            this.buttonConfirm.Name = "buttonConfirm";
            this.buttonConfirm.Size = new System.Drawing.Size(170, 45);
            this.buttonConfirm.TabIndex = 6;
            this.buttonConfirm.Text = "確認";
            this.buttonConfirm.UseVisualStyleBackColor = false;
            // 
            // ScheduleEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(400, 550);
            this.Controls.Add(this.buttonConfirm);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.panelDays);
            this.Controls.Add(this.panelTime);
            this.Controls.Add(this.panelDevice);
            this.Controls.Add(this.panelEnable);
            this.Controls.Add(this.labelFormTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScheduleEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "排程編輯";
            this.panelEnable.ResumeLayout(false);
            this.panelDevice.ResumeLayout(false);
            this.panelTime.ResumeLayout(false);
            this.panelDays.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelFormTitle;
        private System.Windows.Forms.Panel panelEnable;
        private System.Windows.Forms.Label labelEnable;
        private System.Windows.Forms.Panel panelDevice;
        private System.Windows.Forms.ComboBox comboBoxDevice;
        private System.Windows.Forms.Label labelDevice;
        private System.Windows.Forms.ComboBox comboBoxFactory;
        private System.Windows.Forms.Label labelFactory;
        private System.Windows.Forms.Label labelDeviceSection;
        private System.Windows.Forms.Panel panelTime;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.DateTimePicker dateTimePickerEnd;
        private System.Windows.Forms.Label labelEnd;
        private System.Windows.Forms.DateTimePicker dateTimePickerStart;
        private System.Windows.Forms.Label labelStart;
        private System.Windows.Forms.Label labelTimeSection;
        private System.Windows.Forms.Panel panelDays;
        private System.Windows.Forms.Label labelDaysSection;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonConfirm;
    }
}
