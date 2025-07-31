using SYLOGOS.Models;

namespace SYLOGOS.Forms
{
    public partial class SettingsForm : UserControl
    {
        private TextBox txtClubName;
        private TextBox txtPhone;
        private PictureBox logoBox;
        private CheckBox chkDarkMode;
        private ComboBox cmbScale;
        private Button btnUpload;
        private Button btnClearLogo;
        private Button btnSave;

        public event Action<bool>? DarkModeChanged;
        public event Action<UiScaleMode>? UiScaleChanged;

        private AppSetting currentSetting;

        public SettingsForm()
        {
            InitializeLayout();
            LoadSetting();
        }

        private void InitializeLayout()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(20);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 6,
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // labels
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // inputs

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220)); // PictureBox
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Upload/Clear buttons
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Save button
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Spacer

            // Club Name
            layout.Controls.Add(new Label { Text = "Club Name:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            txtClubName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtClubName, 1, 0);

            // Phone
            layout.Controls.Add(new Label { Text = "Phone:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            txtPhone = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtPhone, 1, 1);

            // Logo preview
            layout.Controls.Add(new Label { Text = "Logo:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
            logoBox = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 200,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            layout.Controls.Add(logoBox, 1, 2);

            // Dark Mode checkbox
            chkDarkMode = new CheckBox { Text = "Enable Dark Mode", AutoSize = true };
            layout.Controls.Add(new Label(), 0, 4);
            layout.Controls.Add(chkDarkMode, 1, 4);

            // Scale Mode dropdown
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // row 5

            layout.Controls.Add(new Label { Text = "UI Scale:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 5);
            cmbScale = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Left };
            cmbScale.Items.AddRange(Enum.GetNames(typeof(UiScaleMode)));
            layout.Controls.Add(cmbScale, 1, 5);


            // Upload + Clear buttons
            FlowLayoutPanel logoButtonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true
            };
            btnUpload = new Button { Text = "Upload Logo" };
            btnUpload.Click += BtnUpload_Click;

            btnClearLogo = new Button { Text = "Clear Logo" };
            btnClearLogo.Click += (_, _) => logoBox.Image = null;

            logoButtonPanel.Controls.Add(btnUpload);
            logoButtonPanel.Controls.Add(btnClearLogo);
            layout.Controls.Add(new Label(), 0, 3); // empty label for spacing
            layout.Controls.Add(logoButtonPanel, 1, 3);

            // Save button
            btnSave = new Button { Text = "Save Settings", AutoSize = true };
            btnSave.Click += BtnSave_Click;

            Panel savePanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            savePanel.Controls.Add(btnSave);
            btnSave.Left = 0;
            btnSave.Top = 5;

            Controls.Add(layout);
            Controls.Add(savePanel);
        }

        private void LoadSetting()
        {
            using AppDbContext db = new AppDbContext();

            currentSetting = db.Settings.FirstOrDefault();
            if (currentSetting == null)
            {
                currentSetting = new AppSetting
                {
                    ClubName = "",
                    Phone = "",
                    ClubLogo = null
                };
                db.Settings.Add(currentSetting);
                db.SaveChanges();
            }

            txtClubName.Text = currentSetting.ClubName;
            txtPhone.Text = currentSetting.Phone;
            logoBox.Image = currentSetting.ClubLogo != null
                ? ConvertBytesToImage(currentSetting.ClubLogo)
                : null;
            chkDarkMode.Checked = currentSetting.UseDarkMode;
            cmbScale.SelectedItem = currentSetting.ScaleMode.ToString();
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.png;*.bmp",
                Title = "Select Club Logo"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                logoBox.Image = Image.FromFile(dialog.FileName);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using AppDbContext db = new AppDbContext();

            AppSetting? setting = db.Settings.FirstOrDefault(s => s.Id == currentSetting.Id);
            if (setting == null)
            {
                MessageBox.Show("Settings record not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            setting.ClubName = txtClubName.Text;
            setting.Phone = txtPhone.Text;
            setting.ClubLogo = logoBox.Image != null ? ConvertImageToBytes(logoBox.Image) : null;

            bool wasDarkMode = currentSetting.UseDarkMode;
            setting.UseDarkMode = chkDarkMode.Checked;

            if (chkDarkMode.Checked != wasDarkMode)
            {
                DarkModeChanged?.Invoke(chkDarkMode.Checked);
            }

            if (Enum.TryParse<UiScaleMode>(cmbScale.SelectedItem?.ToString(), out UiScaleMode selectedScale))
            {
                setting.ScaleMode = selectedScale;
                UiScaleChanged?.Invoke(selectedScale);
            }

            db.SaveChanges();
            MessageBox.Show("Settings saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private byte[] ConvertImageToBytes(Image image)
        {
            using MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        private Image ConvertBytesToImage(byte[] bytes)
        {
            using MemoryStream ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }
    }
}
