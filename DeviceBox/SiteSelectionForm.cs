using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DeviceBox
{
    /// <summary>
    /// 場域選擇對話框
    /// </summary>
    public class SiteSelectionForm : Form
    {
        private ComboBox cmbSites;
        private Button btnConfirm;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblDescription;
        private Label lblCurrent;

        public string SelectedSiteId { get; private set; }
        public string SelectedSiteName { get; private set; }

        private Dictionary<string, string> availableSites;

        public SiteSelectionForm(Dictionary<string, string> sites)
        {
            availableSites = sites;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "場域選擇 - 請仔細確認";
            this.Size = new Size(450, 280);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.WhiteSmoke;

            // 標題
            lblTitle = new Label
            {
                Text = "⚠ 請選擇當前場域",
                Font = new Font("Microsoft JhengHei", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(410, 35),
                ForeColor = Color.DarkRed
            };

            // 說明
            lblDescription = new Label
            {
                Text = "⚙ 此設定將儲存在本機,下次啟動時自動載入。\n" +
                       "📌 每個場域有獨立的排程模式和設定。\n" +
                       "🔄 同場域的多個軟體實例會自動同步模式。",
                Location = new Point(20, 60),
                Size = new Size(410, 65),
                Font = new Font("Microsoft JhengHei", 9)
            };

            // 當前選擇標籤
            var lblCurrent = new Label
            {
                Text = "當前選擇：",
                Location = new Point(20, 135),
                Size = new Size(80, 25),
                Font = new Font("Microsoft JhengHei", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            // 下拉選單
            cmbSites = new ComboBox
            {
                Location = new Point(105, 135),
                Size = new Size(320, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft JhengHei", 12, FontStyle.Bold),
                BackColor = Color.LightYellow
            };

            foreach (var site in availableSites)
            {
                cmbSites.Items.Add(new SiteItem { SiteId = site.Key, SiteName = site.Value });
            }

            if (cmbSites.Items.Count > 0)
            {
                cmbSites.SelectedIndex = 0;
            }

            cmbSites.DisplayMember = "SiteName";

            // 確認按鈕
            btnConfirm = new Button
            {
                Text = "✓ 確認選擇",
                Location = new Point(130, 185),
                Size = new Size(140, 40),
                Font = new Font("Microsoft JhengHei", 11, FontStyle.Bold),
                BackColor = Color.LightGreen,
                ForeColor = Color.DarkGreen
            };
            btnConfirm.Click += BtnConfirm_Click;

            // 取消按鈕
            btnCancel = new Button
            {
                Text = "✗ 取消",
                Location = new Point(280, 185),
                Size = new Size(100, 40),
                Font = new Font("Microsoft JhengHei", 10),
                BackColor = Color.LightGray
            };
            btnCancel.Click += BtnCancel_Click;

            // 添加控制項
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblDescription);
            this.Controls.Add(lblCurrent);
            this.Controls.Add(cmbSites);
            this.Controls.Add(btnConfirm);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnConfirm;
            this.CancelButton = btnCancel;
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (cmbSites.SelectedItem is SiteItem selected)
            {
                SelectedSiteId = selected.SiteId;
                SelectedSiteName = selected.SiteName;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("請選擇一個場域", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private class SiteItem
        {
            public string SiteId { get; set; }
            public string SiteName { get; set; }

            public override string ToString()
            {
                return SiteName;
            }
        }
    }
}
