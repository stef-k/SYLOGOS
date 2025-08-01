// no designer
using SYLOGOS.Models;

namespace SYLOGOS.Forms
{
    public partial class SettingsView : UserControl
    {
        private TextBox txtClubName;
        private TextBox txtPhone;
        private PictureBox logoBox;
        private TextBox txtEmail;
        private TextBox txtWebsite;
        private TextBox txtAddress;

        private CheckBox chkDarkMode;
        private ComboBox cmbScale;
        private Button btnUpload;
        private Button btnClearLogo;
        private Button btnSave;

        public event Action<bool>? DarkModeChanged;
        public event Action<UiScaleMode>? UiScaleChanged;

        private AppSetting currentSetting;

        public SettingsView()
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
                RowCount = 0,
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // --- CLUB DETAILS HEADER ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            Label clubHeader = new Label
            {
                Text = "🏛️ Στοιχεία Συλλόγου",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            layout.Controls.Add(clubHeader, 0, layout.RowCount - 1);
            layout.SetColumnSpan(clubHeader, 2);

            // --- Club Name ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.Controls.Add(new Label { Text = "Club Name:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, layout.RowCount - 1);
            txtClubName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtClubName, 1, layout.RowCount - 1);

            // --- Phone ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.Controls.Add(new Label { Text = "Phone:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, layout.RowCount - 1);
            txtPhone = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtPhone, 1, layout.RowCount - 1);

            // --- Email ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.Controls.Add(new Label { Text = "Email:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, layout.RowCount - 1);
            txtEmail = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtEmail, 1, layout.RowCount - 1);

            // --- Website ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.Controls.Add(new Label { Text = "Website:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, layout.RowCount - 1);
            txtWebsite = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtWebsite, 1, layout.RowCount - 1);

            // --- Address ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.Controls.Add(new Label { Text = "Address:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, layout.RowCount - 1);
            txtAddress = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtAddress, 1, layout.RowCount - 1);

            // --- Logo ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
            layout.Controls.Add(new Label { Text = "Logo:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, layout.RowCount - 1);
            logoBox = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 200,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            layout.Controls.Add(logoBox, 1, layout.RowCount - 1);

            // --- Upload / Clear Buttons ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
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
            layout.Controls.Add(new Label(), 0, layout.RowCount - 1);
            layout.Controls.Add(logoButtonPanel, 1, layout.RowCount - 1);

            // --- APPLICATION SETTINGS HEADER ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            Label appHeader = new Label
            {
                Text = "⚙️ Ρυθμίσεις Εφαρμογής ",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            layout.Controls.Add(appHeader, 0, layout.RowCount - 1);
            layout.SetColumnSpan(appHeader, 2);

            // --- Dark Mode ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            chkDarkMode = new CheckBox { Text = "Enable Dark Mode", AutoSize = true };
            layout.Controls.Add(new Label(), 0, layout.RowCount - 1);
            layout.Controls.Add(chkDarkMode, 1, layout.RowCount - 1);

            // --- UI Scale ---
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.Controls.Add(new Label { Text = "UI Scale:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, layout.RowCount - 1);
            cmbScale = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Left };
            cmbScale.Items.AddRange(Enum.GetNames(typeof(UiScaleMode)));
            layout.Controls.Add(cmbScale, 1, layout.RowCount - 1);

            // --- Save Button ---
            btnSave = new Button { Text = "Save Settings", AutoSize = true };
            btnSave.Click += BtnSave_Click;

            FlowLayoutPanel savePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 0),
                WrapContents = false
            };
            savePanel.Controls.Add(btnSave);

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
            txtEmail.Text = currentSetting.Email ?? "";
            txtWebsite.Text = currentSetting.Website ?? "";
            txtAddress.Text = currentSetting.Address ?? "";

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
            setting.Email = txtEmail.Text.Trim();
            setting.Website = txtWebsite.Text.Trim();
            setting.Address = txtAddress.Text.Trim();
            setting.ClubLogo = logoBox.Image != null ? ConvertImageToBytes(logoBox.Image) : null;

            bool darkChanged = setting.UseDarkMode != chkDarkMode.Checked;
            bool scaleChanged = Enum.TryParse<UiScaleMode>(cmbScale.SelectedItem?.ToString(), out UiScaleMode selectedScale)
                                && setting.ScaleMode != selectedScale;

            setting.UseDarkMode = chkDarkMode.Checked;
            if (scaleChanged)
            {
                setting.ScaleMode = selectedScale;
            }

            db.SaveChanges();

            if (darkChanged || scaleChanged)
            {
                MessageBox.Show(
                    "The selected setting(s) require an app restart.\nThe application will now restart.",
                    "Restart Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Application.Restart();
                Environment.Exit(0);
                return;
            }

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
