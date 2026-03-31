namespace DeviceBox
{
    partial class ModeEditForm
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
            this.panelBasic = new System.Windows.Forms.Panel();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.labelBasicSection = new System.Windows.Forms.Label();
            this.panelScheduleTitle = new System.Windows.Forms.Panel();
            this.buttonAddSchedule = new System.Windows.Forms.Button();
            this.labelScheduleSection = new System.Windows.Forms.Label();
            this.panelScheduleList = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonConfirm = new System.Windows.Forms.Button();
            this.panelBasic.SuspendLayout();
            this.panelScheduleTitle.SuspendLayout();
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
            this.labelFormTitle.Text = "編輯模式";
            // 
            // panelBasic
            // 
            this.panelBasic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelBasic.Controls.Add(this.textBoxDescription);
            this.panelBasic.Controls.Add(this.labelDescription);
            this.panelBasic.Controls.Add(this.textBoxName);
            this.panelBasic.Controls.Add(this.labelName);
            this.panelBasic.Controls.Add(this.labelBasicSection);
            this.panelBasic.Location = new System.Drawing.Point(20, 60);
            this.panelBasic.Name = "panelBasic";
            this.panelBasic.Size = new System.Drawing.Size(420, 110);
            this.panelBasic.TabIndex = 1;
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(65)))));
            this.textBoxDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxDescription.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.textBoxDescription.ForeColor = System.Drawing.Color.White;
            this.textBoxDescription.Location = new System.Drawing.Point(80, 70);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(320, 27);
            this.textBoxDescription.TabIndex = 4;
            // 
            // labelDescription
            // 
            this.labelDescription.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.labelDescription.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelDescription.Location = new System.Drawing.Point(15, 73);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(60, 25);
            this.labelDescription.TabIndex = 3;
            this.labelDescription.Text = "描述";
            // 
            // textBoxName
            // 
            this.textBoxName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(65)))));
            this.textBoxName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxName.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.textBoxName.ForeColor = System.Drawing.Color.White;
            this.textBoxName.Location = new System.Drawing.Point(80, 35);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(200, 27);
            this.textBoxName.TabIndex = 2;
            // 
            // labelName
            // 
            this.labelName.Font = new System.Drawing.Font("微軟正黑體", 10F);
            this.labelName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelName.Location = new System.Drawing.Point(15, 38);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(60, 25);
            this.labelName.TabIndex = 1;
            this.labelName.Text = "名稱";
            // 
            // labelBasicSection
            // 
            this.labelBasicSection.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelBasicSection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelBasicSection.Location = new System.Drawing.Point(15, 8);
            this.labelBasicSection.Name = "labelBasicSection";
            this.labelBasicSection.Size = new System.Drawing.Size(100, 20);
            this.labelBasicSection.TabIndex = 0;
            this.labelBasicSection.Text = "基本資訊";
            // 
            // panelScheduleTitle
            // 
            this.panelScheduleTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelScheduleTitle.Controls.Add(this.buttonAddSchedule);
            this.panelScheduleTitle.Controls.Add(this.labelScheduleSection);
            this.panelScheduleTitle.Location = new System.Drawing.Point(20, 180);
            this.panelScheduleTitle.Name = "panelScheduleTitle";
            this.panelScheduleTitle.Size = new System.Drawing.Size(420, 40);
            this.panelScheduleTitle.TabIndex = 2;
            // 
            // buttonAddSchedule
            // 
            this.buttonAddSchedule.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.buttonAddSchedule.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonAddSchedule.FlatAppearance.BorderSize = 0;
            this.buttonAddSchedule.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAddSchedule.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.buttonAddSchedule.ForeColor = System.Drawing.Color.White;
            this.buttonAddSchedule.Location = new System.Drawing.Point(320, 5);
            this.buttonAddSchedule.Name = "buttonAddSchedule";
            this.buttonAddSchedule.Size = new System.Drawing.Size(90, 30);
            this.buttonAddSchedule.TabIndex = 1;
            this.buttonAddSchedule.Text = "+ 新增排程";
            this.buttonAddSchedule.UseVisualStyleBackColor = false;
            // 
            // labelScheduleSection
            // 
            this.labelScheduleSection.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelScheduleSection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.labelScheduleSection.Location = new System.Drawing.Point(15, 10);
            this.labelScheduleSection.Name = "labelScheduleSection";
            this.labelScheduleSection.Size = new System.Drawing.Size(100, 20);
            this.labelScheduleSection.TabIndex = 0;
            this.labelScheduleSection.Text = "排程設定";
            // 
            // panelScheduleList
            // 
            this.panelScheduleList.AutoScroll = true;
            this.panelScheduleList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(43)))));
            this.panelScheduleList.Location = new System.Drawing.Point(20, 225);
            this.panelScheduleList.Name = "panelScheduleList";
            this.panelScheduleList.Size = new System.Drawing.Size(420, 200);
            this.panelScheduleList.TabIndex = 3;
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.buttonCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonCancel.FlatAppearance.BorderSize = 0;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(20, 440);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(200, 45);
            this.buttonCancel.TabIndex = 4;
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
            this.buttonConfirm.Location = new System.Drawing.Point(240, 440);
            this.buttonConfirm.Name = "buttonConfirm";
            this.buttonConfirm.Size = new System.Drawing.Size(200, 45);
            this.buttonConfirm.TabIndex = 5;
            this.buttonConfirm.Text = "確認";
            this.buttonConfirm.UseVisualStyleBackColor = false;
            // 
            // ModeEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(460, 500);
            this.Controls.Add(this.buttonConfirm);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.panelScheduleList);
            this.Controls.Add(this.panelScheduleTitle);
            this.Controls.Add(this.panelBasic);
            this.Controls.Add(this.labelFormTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModeEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "編輯模式";
            this.panelBasic.ResumeLayout(false);
            this.panelBasic.PerformLayout();
            this.panelScheduleTitle.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelFormTitle;
        private System.Windows.Forms.Panel panelBasic;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelBasicSection;
        private System.Windows.Forms.Panel panelScheduleTitle;
        private System.Windows.Forms.Button buttonAddSchedule;
        private System.Windows.Forms.Label labelScheduleSection;
        private System.Windows.Forms.Panel panelScheduleList;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonConfirm;
    }
}
