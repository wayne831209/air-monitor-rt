namespace DeviceBox
{
    partial class DeviceTrendChartForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelCharts = new System.Windows.Forms.Panel();
            this.chartCombined = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panelDeviceFilter = new System.Windows.Forms.Panel();
            this.cmbFactory = new System.Windows.Forms.ComboBox();
            this.clbDevices = new System.Windows.Forms.CheckedListBox();
            this.panelAlarmLimits = new System.Windows.Forms.Panel();
            this.clbAlarmLimits = new System.Windows.Forms.CheckedListBox();
            this.labelAlarmLimits = new System.Windows.Forms.Label();
            this.btnDeselectAll = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.labelDeviceFilter = new System.Windows.Forms.Label();
            this.panelToolbar = new System.Windows.Forms.Panel();
            this.btnQuery = new System.Windows.Forms.Button();
            this.btnResetZoom = new System.Windows.Forms.Button();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.labelTo = new System.Windows.Forms.Label();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.labelDateRange = new System.Windows.Forms.Label();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panelMain.SuspendLayout();
            this.panelCharts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartCombined)).BeginInit();
            this.panelDeviceFilter.SuspendLayout();
            this.panelAlarmLimits.SuspendLayout();
            this.panelToolbar.SuspendLayout();
            this.panelTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelMain.Controls.Add(this.panelCharts);
            this.panelMain.Controls.Add(this.panelDeviceFilter);
            this.panelMain.Controls.Add(this.panelToolbar);
            this.panelMain.Controls.Add(this.panelTitle);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(10);
            this.panelMain.Size = new System.Drawing.Size(1300, 750);
            this.panelMain.TabIndex = 0;
            // 
            // panelCharts
            // 
            this.panelCharts.Controls.Add(this.chartCombined);
            this.panelCharts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCharts.Location = new System.Drawing.Point(190, 100);
            this.panelCharts.Name = "panelCharts";
            this.panelCharts.Size = new System.Drawing.Size(1100, 640);
            this.panelCharts.TabIndex = 3;
            // 
            // chartCombined
            // 
            chartArea1.Name = "ChartAreaPressure";
            chartArea2.Name = "ChartAreaTemp";
            chartArea3.Name = "ChartAreaDemand";
            this.chartCombined.ChartAreas.Add(chartArea1);
            this.chartCombined.ChartAreas.Add(chartArea2);
            this.chartCombined.ChartAreas.Add(chartArea3);
            this.chartCombined.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Alignment = System.Drawing.StringAlignment.Far;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend1.Name = "LegendMain";
            this.chartCombined.Legends.Add(legend1);
            this.chartCombined.Location = new System.Drawing.Point(0, 0);
            this.chartCombined.Name = "chartCombined";
            this.chartCombined.Size = new System.Drawing.Size(1100, 640);
            this.chartCombined.TabIndex = 0;
            this.chartCombined.Text = "綜合曲線圖";
            // 
            // panelDeviceFilter
            // 
            this.panelDeviceFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(50)))));
            this.panelDeviceFilter.Controls.Add(this.clbDevices);
            this.panelDeviceFilter.Controls.Add(this.cmbFactory);
            this.panelDeviceFilter.Controls.Add(this.panelAlarmLimits);
            this.panelDeviceFilter.Controls.Add(this.btnDeselectAll);
            this.panelDeviceFilter.Controls.Add(this.btnSelectAll);
            this.panelDeviceFilter.Controls.Add(this.labelDeviceFilter);
            this.panelDeviceFilter.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelDeviceFilter.Location = new System.Drawing.Point(10, 100);
            this.panelDeviceFilter.Name = "panelDeviceFilter";
            this.panelDeviceFilter.Size = new System.Drawing.Size(180, 640);
            this.panelDeviceFilter.TabIndex = 1;
            // 
            // cmbFactory
            // 
            this.cmbFactory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(55)))));
            this.cmbFactory.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbFactory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFactory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbFactory.Font = new System.Drawing.Font("微軟正黑體", 10.5F);
            this.cmbFactory.ForeColor = System.Drawing.Color.White;
            this.cmbFactory.FormattingEnabled = true;
            this.cmbFactory.Location = new System.Drawing.Point(0, 100);
            this.cmbFactory.Name = "cmbFactory";
            this.cmbFactory.Size = new System.Drawing.Size(180, 28);
            this.cmbFactory.TabIndex = 5;
            this.cmbFactory.SelectedIndexChanged += new System.EventHandler(this.cmbFactory_SelectedIndexChanged);
            // 
            // clbDevices
            // 
            this.clbDevices.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(55)))));
            this.clbDevices.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.clbDevices.CheckOnClick = true;
            this.clbDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbDevices.Font = new System.Drawing.Font("微軟正黑體", 10.5F);
            this.clbDevices.ForeColor = System.Drawing.Color.White;
            this.clbDevices.FormattingEnabled = true;
            this.clbDevices.Location = new System.Drawing.Point(0, 128);
            this.clbDevices.Name = "clbDevices";
            this.clbDevices.Size = new System.Drawing.Size(180, 312);
            this.clbDevices.TabIndex = 3;
            // 
            // panelAlarmLimits
            // 
            this.panelAlarmLimits.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(50)))));
            this.panelAlarmLimits.Controls.Add(this.clbAlarmLimits);
            this.panelAlarmLimits.Controls.Add(this.labelAlarmLimits);
            this.panelAlarmLimits.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelAlarmLimits.Location = new System.Drawing.Point(0, 440);
            this.panelAlarmLimits.Name = "panelAlarmLimits";
            this.panelAlarmLimits.Size = new System.Drawing.Size(180, 200);
            this.panelAlarmLimits.TabIndex = 4;
            // 
            // clbAlarmLimits
            // 
            this.clbAlarmLimits.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(55)))));
            this.clbAlarmLimits.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.clbAlarmLimits.CheckOnClick = true;
            this.clbAlarmLimits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbAlarmLimits.Font = new System.Drawing.Font("微軟正黑體", 10.5F);
            this.clbAlarmLimits.ForeColor = System.Drawing.Color.White;
            this.clbAlarmLimits.FormattingEnabled = true;
            this.clbAlarmLimits.Location = new System.Drawing.Point(0, 30);
            this.clbAlarmLimits.Name = "clbAlarmLimits";
            this.clbAlarmLimits.Size = new System.Drawing.Size(180, 170);
            this.clbAlarmLimits.TabIndex = 1;
            this.clbAlarmLimits.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbAlarmLimits_ItemCheck);
            // 
            // labelAlarmLimits
            // 
            this.labelAlarmLimits.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(180)))));
            this.labelAlarmLimits.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelAlarmLimits.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold);
            this.labelAlarmLimits.ForeColor = System.Drawing.Color.White;
            this.labelAlarmLimits.Location = new System.Drawing.Point(0, 0);
            this.labelAlarmLimits.Name = "labelAlarmLimits";
            this.labelAlarmLimits.Size = new System.Drawing.Size(180, 30);
            this.labelAlarmLimits.TabIndex = 0;
            this.labelAlarmLimits.Text = "上下限顯示";
            this.labelAlarmLimits.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnDeselectAll
            // 
            this.btnDeselectAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(85)))));
            this.btnDeselectAll.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDeselectAll.FlatAppearance.BorderSize = 0;
            this.btnDeselectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeselectAll.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnDeselectAll.ForeColor = System.Drawing.Color.White;
            this.btnDeselectAll.Location = new System.Drawing.Point(0, 65);
            this.btnDeselectAll.Name = "btnDeselectAll";
            this.btnDeselectAll.Size = new System.Drawing.Size(180, 35);
            this.btnDeselectAll.TabIndex = 2;
            this.btnDeselectAll.Text = "取消全選";
            this.btnDeselectAll.UseVisualStyleBackColor = false;
            this.btnDeselectAll.Click += new System.EventHandler(this.btnDeselectAll_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnSelectAll.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSelectAll.FlatAppearance.BorderSize = 0;
            this.btnSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectAll.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnSelectAll.ForeColor = System.Drawing.Color.White;
            this.btnSelectAll.Location = new System.Drawing.Point(0, 35);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(180, 30);
            this.btnSelectAll.TabIndex = 1;
            this.btnSelectAll.Text = "全選";
            this.btnSelectAll.UseVisualStyleBackColor = false;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // labelDeviceFilter
            // 
            this.labelDeviceFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(180)))));
            this.labelDeviceFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelDeviceFilter.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold);
            this.labelDeviceFilter.ForeColor = System.Drawing.Color.White;
            this.labelDeviceFilter.Location = new System.Drawing.Point(0, 0);
            this.labelDeviceFilter.Name = "labelDeviceFilter";
            this.labelDeviceFilter.Size = new System.Drawing.Size(180, 35);
            this.labelDeviceFilter.TabIndex = 0;
            this.labelDeviceFilter.Text = "設備篩選";
            this.labelDeviceFilter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelToolbar
            // 
            this.panelToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.panelToolbar.Controls.Add(this.btnResetZoom);
            this.panelToolbar.Controls.Add(this.btnQuery);
            this.panelToolbar.Controls.Add(this.dtpEnd);
            this.panelToolbar.Controls.Add(this.labelTo);
            this.panelToolbar.Controls.Add(this.dtpStart);
            this.panelToolbar.Controls.Add(this.labelDateRange);
            this.panelToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelToolbar.Location = new System.Drawing.Point(10, 60);
            this.panelToolbar.Name = "panelToolbar";
            this.panelToolbar.Size = new System.Drawing.Size(1280, 40);
            this.panelToolbar.TabIndex = 2;
            // 
            // btnQuery
            // 
            this.btnQuery.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnQuery.FlatAppearance.BorderSize = 0;
            this.btnQuery.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuery.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold);
            this.btnQuery.ForeColor = System.Drawing.Color.White;
            this.btnQuery.Location = new System.Drawing.Point(560, 5);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(90, 30);
            this.btnQuery.TabIndex = 4;
            this.btnQuery.Text = "查詢";
            this.btnQuery.UseVisualStyleBackColor = false;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // btnResetZoom
            // 
            this.btnResetZoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(85)))));
            this.btnResetZoom.FlatAppearance.BorderSize = 0;
            this.btnResetZoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetZoom.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnResetZoom.ForeColor = System.Drawing.Color.White;
            this.btnResetZoom.Location = new System.Drawing.Point(660, 5);
            this.btnResetZoom.Name = "btnResetZoom";
            this.btnResetZoom.Size = new System.Drawing.Size(100, 30);
            this.btnResetZoom.TabIndex = 5;
            this.btnResetZoom.Text = "重置縮放";
            this.btnResetZoom.UseVisualStyleBackColor = false;
            this.btnResetZoom.Click += new System.EventHandler(this.btnResetZoom_Click);
            // 
            // dtpEnd
            // 
            this.dtpEnd.CalendarFont = new System.Drawing.Font("微軟正黑體", 9.75F);
            this.dtpEnd.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpEnd.Font = new System.Drawing.Font("微軟正黑體", 11.25F);
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEnd.Location = new System.Drawing.Point(350, 6);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(200, 27);
            this.dtpEnd.TabIndex = 3;
            // 
            // labelTo
            // 
            this.labelTo.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold);
            this.labelTo.ForeColor = System.Drawing.Color.White;
            this.labelTo.Location = new System.Drawing.Point(320, 8);
            this.labelTo.Name = "labelTo";
            this.labelTo.Size = new System.Drawing.Size(30, 25);
            this.labelTo.TabIndex = 2;
            this.labelTo.Text = "~";
            this.labelTo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dtpStart
            // 
            this.dtpStart.CalendarFont = new System.Drawing.Font("微軟正黑體", 9.75F);
            this.dtpStart.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpStart.Font = new System.Drawing.Font("微軟正黑體", 11.25F);
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStart.Location = new System.Drawing.Point(110, 6);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(200, 27);
            this.dtpStart.TabIndex = 1;
            // 
            // labelDateRange
            // 
            this.labelDateRange.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold);
            this.labelDateRange.ForeColor = System.Drawing.Color.White;
            this.labelDateRange.Location = new System.Drawing.Point(10, 8);
            this.labelDateRange.Name = "labelDateRange";
            this.labelDateRange.Size = new System.Drawing.Size(100, 25);
            this.labelDateRange.TabIndex = 0;
            this.labelDateRange.Text = "日期範圍:";
            this.labelDateRange.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelTitle
            // 
            this.panelTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panelTitle.Controls.Add(this.labelTitle);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitle.Location = new System.Drawing.Point(10, 10);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(1280, 50);
            this.panelTitle.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTitle.Font = new System.Drawing.Font("微軟正黑體", 20.25F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(1280, 50);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "設備壓力 / 溫度曲線圖";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DeviceTrendChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(1300, 750);
            this.Controls.Add(this.panelMain);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.Name = "DeviceTrendChartForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "設備壓力 / 溫度 / 需量曲線圖";
            this.panelMain.ResumeLayout(false);
            this.panelCharts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartCombined)).EndInit();
            this.panelDeviceFilter.ResumeLayout(false);
            this.panelAlarmLimits.ResumeLayout(false);
            this.panelToolbar.ResumeLayout(false);
            this.panelTitle.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Panel panelToolbar;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Button btnResetZoom;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.Label labelDateRange;
        private System.Windows.Forms.Panel panelDeviceFilter;
        private System.Windows.Forms.CheckedListBox clbDevices;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnDeselectAll;
        private System.Windows.Forms.Label labelDeviceFilter;
        private System.Windows.Forms.Panel panelCharts;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCombined;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panelAlarmLimits;
        private System.Windows.Forms.CheckedListBox clbAlarmLimits;
        private System.Windows.Forms.Label labelAlarmLimits;
        private System.Windows.Forms.ComboBox cmbFactory;
    }
}
