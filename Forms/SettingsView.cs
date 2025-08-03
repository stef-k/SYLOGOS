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
        private NumericUpDown nudReceiptStart;
        private Label lblReceiptNote;

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

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 0,
                AutoSize = true
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // ---- CLUB INFO ----
            TableLayoutPanel clubTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 0,
                AutoSize = true
            };
            clubTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
            clubTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Label clubHeader = new Label
            {
                Text = "🏛️ Στοιχεία Συλλόγου",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            clubTable.RowCount++;
            clubTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            clubTable.Controls.Add(clubHeader, 0, 0);
            clubTable.SetColumnSpan(clubHeader, 2);

            void AddClubRow(string label, out TextBox box)
            {
                clubTable.RowCount++;
                clubTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                clubTable.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, clubTable.RowCount - 1);
                box = new TextBox { Dock = DockStyle.Fill };
                clubTable.Controls.Add(box, 1, clubTable.RowCount - 1);
            }

            AddClubRow("Ονομασία Συλλόγου:", out txtClubName);
            AddClubRow("Τηλέφωνο:", out txtPhone);
            AddClubRow("Email:", out txtEmail);
            AddClubRow("Ιστοσελίδα:", out txtWebsite);
            AddClubRow("Διεύθυνση:", out txtAddress);

            // Logo row
            clubTable.RowCount++;
            clubTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
            clubTable.Controls.Add(new Label { Text = "Logo:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, clubTable.RowCount - 1);
            logoBox = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 200,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            clubTable.Controls.Add(logoBox, 1, clubTable.RowCount - 1);

            // Upload / Clear buttons
            FlowLayoutPanel logoButtons = new FlowLayoutPanel { Dock = DockStyle.Left, AutoSize = true };
            btnUpload = new Button
            {
                Text = "Εισαγωγή Λογότυπου",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(5, 0, 5, 0),
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUpload.FlatAppearance.BorderSize = 0;
            btnUpload.Click += BtnUpload_Click;
            btnClearLogo = new Button
            {
                Text = "Διαγραφή Λογότυπου",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(5, 0, 5, 0)
            };
            btnClearLogo.BackColor = Color.Firebrick;
            btnClearLogo.ForeColor = Color.White;
            btnClearLogo.FlatStyle = FlatStyle.Flat;
            btnClearLogo.FlatAppearance.BorderSize = 0;
            btnClearLogo.Click += (_, _) => logoBox.Image = null;
            logoButtons.Controls.Add(btnUpload);
            logoButtons.Controls.Add(btnClearLogo);
            clubTable.RowCount++;
            clubTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            logoButtons.Margin = new Padding(0, 8, 0, 8);
            clubTable.Controls.Add(new Label(), 0, clubTable.RowCount - 1);
            clubTable.Controls.Add(logoButtons, 1, clubTable.RowCount - 1);

            // Receipt start number
            Label lblReceiptStart = new Label
            {
                Text = "Αρχικός Αριθμός Αποδείξεων:",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill
            };

            nudReceiptStart = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 99999,
                Increment = 1,
                Value = 1,
                Dock = DockStyle.Fill
            };

            clubTable.RowCount++;
            clubTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            clubTable.Controls.Add(lblReceiptStart, 0, clubTable.RowCount - 1);
            clubTable.Controls.Add(nudReceiptStart, 1, clubTable.RowCount - 1);

            lblReceiptNote = new Label
            {
                Text = "Αυτός ο αριθμός χρησιμοποιείται μόνο κατά την έκδοση της πρώτης απόδειξης του έτους. Μόλις εκδοθούν αποδείξεις για το έτος, η τιμή αυτή θα έχει πλέον επίδραση στο επόμενο έτος.",
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0),
                Margin = new Padding(0, 2, 0, 8),
                Width = 500 // ⬅ important
            };

            clubTable.RowCount++;
            clubTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // for testing
            clubTable.Controls.Add(lblReceiptNote, 0, clubTable.RowCount - 1);
            clubTable.SetColumnSpan(lblReceiptNote, 2);


            // ---- APP SETTINGS ----
            TableLayoutPanel appTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 0,
                AutoSize = true
            };
            appTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            appTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Label appHeader = new Label
            {
                Text = "⚙️ Ρυθμίσεις Εφαρμογής ",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            appTable.RowCount++;
            appTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            appTable.Controls.Add(appHeader, 0, 0);
            appTable.SetColumnSpan(appHeader, 2);

            // Dark mode
            appTable.RowCount++;
            appTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            chkDarkMode = new CheckBox { Text = "Ενεργοποίηση Σκούρου Θέματος", AutoSize = true };
            appTable.Controls.Add(new Label(), 0, appTable.RowCount - 1);
            appTable.Controls.Add(chkDarkMode, 1, appTable.RowCount - 1);

            // UI scale
            appTable.RowCount++;
            appTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            appTable.Controls.Add(new Label { Text = "Μέγεθος Γραμματοσειράς:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, appTable.RowCount - 1);
            cmbScale = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Left };
            cmbScale.Items.Clear();
            cmbScale.Items.AddRange(new object[] { "Μικρό", "Κανονικό", "Μεγάλο" });
            appTable.Controls.Add(cmbScale, 1, appTable.RowCount - 1);

            // Add both tables
            root.Controls.Add(clubTable, 0, 0);
            root.Controls.Add(appTable, 1, 0);

            // Save button row
            FlowLayoutPanel savePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 0),
                WrapContents = false
            };
            btnSave = new Button
            {
                Text = "Αποθήκευση Ρυθμίσεων",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(5, 0, 5, 0)
            };
            btnSave.BackColor = Color.ForestGreen;
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            btnSave.FlatAppearance.BorderSize = 0;
            savePanel.Controls.Add(btnSave);

            Font labelFont = new Font("Segoe UI", 11); // adjust to 12 if needed

            foreach (Control ctl in clubTable.Controls)
            {
                if (ctl is Label lbl && !lbl.Font.Bold)  // skip header
                {
                    lbl.Font = labelFont;
                }
            }

            foreach (Control ctl in appTable.Controls)
            {
                if (ctl is Label lbl && !lbl.Font.Bold)  // skip header
                {
                    lbl.Font = labelFont;
                }
            }

            foreach (Control ctl in appTable.Controls)
            {
                if (ctl is Label lbl && !lbl.Font.Bold)
                {
                    lbl.Font = labelFont;
                }
                else if (ctl is CheckBox cb)
                {
                    cb.Font = labelFont;
                }
            }


            Controls.Add(root);
            Controls.Add(savePanel);
        }


        private void LoadSetting()
        {
            using AppDbContext db = new AppDbContext();

            currentSetting = db.Settings.FirstOrDefault();
            bool isNew = false;

            if (currentSetting == null)
            {
                currentSetting = new AppSetting();
                db.Settings.Add(currentSetting);
                db.SaveChanges();
                isNew = true;
            }

            // Populate UI fields safely
            txtClubName.Text = currentSetting.ClubName ?? "";
            txtPhone.Text = currentSetting.Phone ?? "";
            txtEmail.Text = currentSetting.Email ?? "";
            txtWebsite.Text = currentSetting.Website ?? "";
            txtAddress.Text = currentSetting.Address ?? "";
            nudReceiptStart.Value = Math.Clamp(currentSetting.ReceiptStartNumber, (int)nudReceiptStart.Minimum, (int)nudReceiptStart.Maximum);

            logoBox.Image = currentSetting.ClubLogo != null
                ? ConvertBytesToImage(currentSetting.ClubLogo)
                : null;

            chkDarkMode.Checked = currentSetting.UseDarkMode;

            // Ensure valid enum
            cmbScale.SelectedItem = currentSetting.ScaleMode switch
            {
                UiScaleMode.Small => "Μικρό",
                UiScaleMode.Normal => "Κανονικό",
                UiScaleMode.Large => "Μεγάλο",
                _ => "Κανονικό"
            };

            if (isNew)
            {
                MessageBox.Show("A new default settings profile was created.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.png;*.bmp",
                Title = "Επιλέξτε Λογότυπου Συλλόγου"
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
                MessageBox.Show("Δεν βρέθηκαν αποθηκευμένες ρυθμίσεις.", "Σφαλμα", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            setting.ClubName = txtClubName.Text;
            setting.Phone = txtPhone.Text;
            setting.Email = txtEmail.Text.Trim();
            setting.Website = txtWebsite.Text.Trim();
            setting.Address = txtAddress.Text.Trim();
            setting.ClubLogo = logoBox.Image != null ? ConvertImageToBytes(logoBox.Image) : null;

            bool darkChanged = setting.UseDarkMode != chkDarkMode.Checked;
            UiScaleMode selectedScale = cmbScale.SelectedItem?.ToString() switch
            {
                "Μικρό" => UiScaleMode.Small,
                "Κανονικό" => UiScaleMode.Normal,
                "Μεγάλο" => UiScaleMode.Large,
                _ => UiScaleMode.Normal
            };

            bool scaleChanged = setting.ScaleMode != selectedScale;


            setting.UseDarkMode = chkDarkMode.Checked;
            if (scaleChanged)
            {
                setting.ScaleMode = selectedScale;
            }

            db.SaveChanges();

            if (darkChanged || scaleChanged)
            {
                MessageBox.Show(
                    "Οι επιλεγμένες ρυθμίσεις απαιτούν επανεκκίνηση της εφαρμογής.\nΗ εφαρμογή θα επανεκκίνησει τώρα.",
                    "Απαιτείται Επανεκκίνηση",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Application.Restart();
                Environment.Exit(0);
                return;
            }

            MessageBox.Show("Οι ρυθμίσεις αποθηκεύτηκαν με επιτυχία.", "Αποθήκευση", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
