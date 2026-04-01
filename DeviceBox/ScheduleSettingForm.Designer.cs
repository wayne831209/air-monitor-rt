namespace DeviceBox
{
    partial class ScheduleSettingForm
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
            this.panelTitle = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.panelAddButton = new System.Windows.Forms.Panel();
            this.buttonViewWeekly = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.panelScheduleList = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.buttonSave = new System.Windows.Forms.Button();
            this.panelTitle.SuspendLayout();
            this.panelAddButton.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTitle
            // 
            this.panelTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panelTitle.Controls.Add(this.labelTitle);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitle.Location = new System.Drawing.Point(0, 0);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(500, 60);
            this.panelTitle.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTitle.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(500, 60);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "排程設定";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelAddButton
            // 
            this.panelAddButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelAddButton.Controls.Add(this.buttonViewWeekly);
            this.panelAddButton.Controls.Add(this.buttonAdd);
            this.panelAddButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAddButton.Location = new System.Drawing.Point(0, 60);
            this.panelAddButton.Name = "panelAddButton";
            this.panelAddButton.Padding = new System.Windows.Forms.Padding(15, 10, 15, 5);
            this.panelAddButton.Size = new System.Drawing.Size(500, 50);
            this.panelAddButton.TabIndex = 1;
            // 
            // buttonViewWeekly
            // 
            this.buttonViewWeekly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.buttonViewWeekly.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonViewWeekly.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonViewWeekly.FlatAppearance.BorderSize = 0;
            this.buttonViewWeekly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonViewWeekly.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.buttonViewWeekly.ForeColor = System.Drawing.Color.White;
            this.buttonViewWeekly.Location = new System.Drawing.Point(355, 10);
            this.buttonViewWeekly.Name = "buttonViewWeekly";
            this.buttonViewWeekly.Size = new System.Drawing.Size(130, 35);
            this.buttonViewWeekly.TabIndex = 1;
            this.buttonViewWeekly.Text = "排程檢視";
            this.buttonViewWeekly.UseVisualStyleBackColor = false;
            // 
            // buttonAdd
            // 
            this.buttonAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.buttonAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonAdd.FlatAppearance.BorderSize = 0;
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAdd.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.buttonAdd.ForeColor = System.Drawing.Color.White;
            this.buttonAdd.Location = new System.Drawing.Point(15, 10);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(120, 35);
            this.buttonAdd.TabIndex = 0;
            this.buttonAdd.Text = "+ 新增排程";
            this.buttonAdd.UseVisualStyleBackColor = false;
            // 
            // panelScheduleList
            // 
            this.panelScheduleList.AutoScroll = true;
            this.panelScheduleList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelScheduleList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelScheduleList.Location = new System.Drawing.Point(0, 110);
            this.panelScheduleList.Name = "panelScheduleList";
            this.panelScheduleList.Padding = new System.Windows.Forms.Padding(10);
            this.panelScheduleList.Size = new System.Drawing.Size(500, 530);
            this.panelScheduleList.TabIndex = 2;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelBottom.Controls.Add(this.buttonSave);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 640);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelBottom.Size = new System.Drawing.Size(500, 60);
            this.panelBottom.TabIndex = 3;
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(199)))), ((int)(((byte)(89)))));
            this.buttonSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonSave.FlatAppearance.BorderSize = 0;
            this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSave.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.buttonSave.ForeColor = System.Drawing.Color.White;
            this.buttonSave.Location = new System.Drawing.Point(15, 10);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(470, 40);
            this.buttonSave.TabIndex = 0;
            this.buttonSave.Text = "儲存設定";
            this.buttonSave.UseVisualStyleBackColor = false;
            // 
            // ScheduleSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(500, 700);
            this.Controls.Add(this.panelScheduleList);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelAddButton);
            this.Controls.Add(this.panelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ScheduleSettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "排程設定";
            this.panelTitle.ResumeLayout(false);
            this.panelAddButton.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Panel panelAddButton;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonViewWeekly;
        private System.Windows.Forms.Panel panelScheduleList;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonSave;
    }
}
