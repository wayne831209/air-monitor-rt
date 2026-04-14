using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DeviceBox
{
    /// <summary>
    /// 手動模式下，多台壓縮機時的選擇視窗
    /// </summary>
    public class ManualCompressorSelectForm : Form
    {
        private readonly FactoryConfig _factory;
        private readonly List<DeviceConfig> _compressors;
        private readonly Dictionary<string, ushort> _manualDOStates;

        public DeviceConfig SelectedCompressor { get; private set; }

        public ManualCompressorSelectForm(FactoryConfig factory, List<DeviceConfig> compressors, Dictionary<string, ushort> manualDOStates)
        {
            _factory = factory;
            _compressors = compressors;
            _manualDOStates = manualDOStates;
            InitializeComponent();
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ClientSize = new Size(350, 60 + _compressors.Count * 70);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "選擇要控制的壓縮機";
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            Label titleLabel = new Label
            {
                Text = $"{_factory.Name} - 選擇壓縮機",
                Location = new Point(15, 10),
                Size = new Size(320, 30),
                Font = new Font("微軟正黑體", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            this.Controls.Add(titleLabel);

            int yOffset = 50;
            foreach (var compressor in _compressors.OrderBy(c => c.MachineNo))
            {
                string key = _factory.Id + "_" + compressor.MachineNo;
                ushort currentState;
                if (!_manualDOStates.TryGetValue(key, out currentState))
                    currentState = 0;

                string stateText = currentState == 1 ? "運轉中" : "已停止";
                Color stateColor = currentState == 1 ? Color.FromArgb(0, 200, 0) : Color.FromArgb(128, 128, 128);

                Button btn = new Button
                {
                    Text = $"{compressor.Name}  [{stateText}]",
                    Location = new Point(15, yOffset),
                    Size = new Size(320, 50),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = currentState == 1 ? Color.FromArgb(0, 100, 0) : Color.FromArgb(60, 60, 65),
                    ForeColor = Color.White,
                    Font = new Font("微軟正黑體", 13F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Tag = compressor
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = stateColor;
                btn.Click += (s, e) =>
                {
                    SelectedCompressor = (DeviceConfig)((Button)s).Tag;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                this.Controls.Add(btn);
                yOffset += 60;
            }
        }
    }
}
