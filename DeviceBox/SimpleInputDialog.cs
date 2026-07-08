using System;
using System.Windows.Forms;

namespace DeviceBox
{
    /// <summary>
    /// 簡單的輸入對話框
    /// </summary>
    public static class SimpleInputDialog
    {
        public static string ShowDialog(string prompt, string title, string defaultValue = "")
        {
            Form inputForm = new Form
            {
                Width = 450,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = System.Drawing.Color.FromArgb(35, 35, 38)
            };

            Label promptLabel = new Label
            {
                Left = 20,
                Top = 20,
                Width = 400,
                Text = prompt,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("微軟正黑體", 10F)
            };

            TextBox inputTextBox = new TextBox
            {
                Left = 20,
                Top = 50,
                Width = 400,
                Text = defaultValue,
                Font = new System.Drawing.Font("微軟正黑體", 11F),
                BackColor = System.Drawing.Color.FromArgb(50, 50, 55),
                ForeColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button confirmButton = new Button
            {
                Text = "確定",
                Left = 240,
                Width = 80,
                Height = 30,
                Top = 85,
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold)
            };

            Button cancelButton = new Button
            {
                Text = "取消",
                Left = 340,
                Width = 80,
                Height = 30,
                Top = 85,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(80, 80, 85),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold)
            };

            confirmButton.Click += (sender, e) => { inputForm.Close(); };
            cancelButton.Click += (sender, e) => { inputForm.Close(); };

            inputForm.Controls.Add(promptLabel);
            inputForm.Controls.Add(inputTextBox);
            inputForm.Controls.Add(confirmButton);
            inputForm.Controls.Add(cancelButton);
            inputForm.AcceptButton = confirmButton;

            return inputForm.ShowDialog() == DialogResult.OK ? inputTextBox.Text : string.Empty;
        }
    }
}
