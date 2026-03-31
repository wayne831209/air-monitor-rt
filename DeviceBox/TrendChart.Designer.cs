namespace DeviceBox
{
    partial class TrendChart
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelChartArea = new System.Windows.Forms.Panel();
            this.panelGanttChart = new System.Windows.Forms.Panel();
            this.panelDeviceLabels = new System.Windows.Forms.Panel();
            this.panelTimeAxisContainer = new System.Windows.Forms.Panel();
            this.panelTimeAxis = new System.Windows.Forms.Panel();
            this.panelCorner = new System.Windows.Forms.Panel();
            this.labelCorner = new System.Windows.Forms.Label();
            this.panelLegend = new System.Windows.Forms.Panel();
            this.labelLegendTime = new System.Windows.Forms.Label();
            this.labelLegendStop = new System.Windows.Forms.Label();
            this.labelLegendRun = new System.Windows.Forms.Label();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panelChartArea.SuspendLayout();
            this.panelTimeAxisContainer.SuspendLayout();
            this.panelCorner.SuspendLayout();
            this.panelLegend.SuspendLayout();
            this.panelTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panel1.Controls.Add(this.panelChartArea);
            this.panel1.Controls.Add(this.panelTimeAxisContainer);
            this.panel1.Controls.Add(this.panelLegend);
            this.panel1.Controls.Add(this.panelTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(10);
            this.panel1.Size = new System.Drawing.Size(1200, 600);
            this.panel1.TabIndex = 0;
            // 
            // panelChartArea
            // 
            this.panelChartArea.Controls.Add(this.panelGanttChart);
            this.panelChartArea.Controls.Add(this.panelDeviceLabels);
            this.panelChartArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelChartArea.Location = new System.Drawing.Point(10, 130);
            this.panelChartArea.Name = "panelChartArea";
            this.panelChartArea.Size = new System.Drawing.Size(1180, 460);
            this.panelChartArea.TabIndex = 3;
            // 
            // panelGanttChart
            // 
            this.panelGanttChart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(38)))));
            this.panelGanttChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGanttChart.Location = new System.Drawing.Point(180, 0);
            this.panelGanttChart.Name = "panelGanttChart";
            this.panelGanttChart.Size = new System.Drawing.Size(1000, 460);
            this.panelGanttChart.TabIndex = 1;
            // 
            // panelDeviceLabels
            // 
            this.panelDeviceLabels.AutoScroll = true;
            this.panelDeviceLabels.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(55)))));
            this.panelDeviceLabels.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelDeviceLabels.Location = new System.Drawing.Point(0, 0);
            this.panelDeviceLabels.Name = "panelDeviceLabels";
            this.panelDeviceLabels.Size = new System.Drawing.Size(180, 460);
            this.panelDeviceLabels.TabIndex = 0;
            // 
            // panelTimeAxisContainer
            // 
            this.panelTimeAxisContainer.Controls.Add(this.panelTimeAxis);
            this.panelTimeAxisContainer.Controls.Add(this.panelCorner);
            this.panelTimeAxisContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTimeAxisContainer.Location = new System.Drawing.Point(10, 95);
            this.panelTimeAxisContainer.Name = "panelTimeAxisContainer";
            this.panelTimeAxisContainer.Size = new System.Drawing.Size(1180, 35);
            this.panelTimeAxisContainer.TabIndex = 2;
            // 
            // panelTimeAxis
            // 
            this.panelTimeAxis.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panelTimeAxis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTimeAxis.Location = new System.Drawing.Point(180, 0);
            this.panelTimeAxis.Name = "panelTimeAxis";
            this.panelTimeAxis.Size = new System.Drawing.Size(1000, 35);
            this.panelTimeAxis.TabIndex = 1;
            // 
            // panelCorner
            // 
            this.panelCorner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panelCorner.Controls.Add(this.labelCorner);
            this.panelCorner.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelCorner.Location = new System.Drawing.Point(0, 0);
            this.panelCorner.Name = "panelCorner";
            this.panelCorner.Size = new System.Drawing.Size(180, 35);
            this.panelCorner.TabIndex = 0;
            // 
            // labelCorner
            // 
            this.labelCorner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCorner.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelCorner.ForeColor = System.Drawing.Color.White;
            this.labelCorner.Location = new System.Drawing.Point(0, 0);
            this.labelCorner.Name = "labelCorner";
            this.labelCorner.Size = new System.Drawing.Size(180, 35);
            this.labelCorner.TabIndex = 0;
            this.labelCorner.Text = "設備名稱";
            this.labelCorner.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelLegend
            // 
            this.panelLegend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.panelLegend.Controls.Add(this.labelLegendTime);
            this.panelLegend.Controls.Add(this.labelLegendStop);
            this.panelLegend.Controls.Add(this.labelLegendRun);
            this.panelLegend.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLegend.Location = new System.Drawing.Point(10, 60);
            this.panelLegend.Name = "panelLegend";
            this.panelLegend.Size = new System.Drawing.Size(1180, 35);
            this.panelLegend.TabIndex = 1;
            // 
            // labelLegendTime
            // 
            this.labelLegendTime.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelLegendTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.labelLegendTime.Location = new System.Drawing.Point(280, 0);
            this.labelLegendTime.Name = "labelLegendTime";
            this.labelLegendTime.Size = new System.Drawing.Size(120, 35);
            this.labelLegendTime.TabIndex = 2;
            this.labelLegendTime.Text = "| 當前時間";
            this.labelLegendTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelLegendStop
            // 
            this.labelLegendStop.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelLegendStop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(85)))));
            this.labelLegendStop.Location = new System.Drawing.Point(150, 0);
            this.labelLegendStop.Name = "labelLegendStop";
            this.labelLegendStop.Size = new System.Drawing.Size(120, 35);
            this.labelLegendStop.TabIndex = 1;
            this.labelLegendStop.Text = "■ 停止";
            this.labelLegendStop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelLegendRun
            // 
            this.labelLegendRun.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelLegendRun.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(180)))), ((int)(((byte)(80)))));
            this.labelLegendRun.Location = new System.Drawing.Point(20, 0);
            this.labelLegendRun.Name = "labelLegendRun";
            this.labelLegendRun.Size = new System.Drawing.Size(120, 35);
            this.labelLegendRun.TabIndex = 0;
            this.labelLegendRun.Text = "■ 運轉中";
            this.labelLegendRun.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelTitle
            // 
            this.panelTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panelTitle.Controls.Add(this.labelTitle);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitle.Location = new System.Drawing.Point(10, 10);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(1180, 50);
            this.panelTitle.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTitle.Font = new System.Drawing.Font("微軟正黑體", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(1180, 50);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "設備排程推移圖";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TrendChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(1200, 600);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.Name = "TrendChart";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "設備排程推移圖";
            this.panel1.ResumeLayout(false);
            this.panelChartArea.ResumeLayout(false);
            this.panelTimeAxisContainer.ResumeLayout(false);
            this.panelCorner.ResumeLayout(false);
            this.panelLegend.ResumeLayout(false);
            this.panelTitle.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Panel panelLegend;
        private System.Windows.Forms.Label labelLegendRun;
        private System.Windows.Forms.Label labelLegendStop;
        private System.Windows.Forms.Label labelLegendTime;
        private System.Windows.Forms.Panel panelTimeAxisContainer;
        private System.Windows.Forms.Panel panelTimeAxis;
        private System.Windows.Forms.Panel panelCorner;
        private System.Windows.Forms.Label labelCorner;
        private System.Windows.Forms.Panel panelChartArea;
        private System.Windows.Forms.Panel panelGanttChart;
        private System.Windows.Forms.Panel panelDeviceLabels;
    }
}