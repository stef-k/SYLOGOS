using SYLOGOS.Models;
using System.Diagnostics;
namespace SYLOGOS.Forms
{
    public partial class MainForm : Form
    {
        private MenuStrip menuStrip;
        private TableLayoutPanel layout;
        private Panel mainPanel;

        public MembersView membersView;
        public SettingsView settingsForm;

        private enum CurrentView { Members, Settings }
        private CurrentView lastViewShown = CurrentView.Members;


        public MainForm()
        {
            MinimumSize = new Size(1120, 800);
            this.Icon = IconHelper.AppIcon;

            InitializeLayout();
        }


        private void SaveScaleModeToDb(UiScaleMode scale)
        {
            using AppDbContext db = new AppDbContext();
            AppSetting? s = db.Settings.FirstOrDefault();
            if (s != null)
            {
                s.ScaleMode = scale;
                db.SaveChanges();
            }
        }

        private void ReloadMainFormWithCurrentView()
        {
            // Store currently shown view type
            Control? previousView = mainPanel.Controls.OfType<Control>().FirstOrDefault();
            bool wasSettings = previousView is SettingsView;

            Controls.Clear();       // remove all layout
            InitializeLayout();     // rebuild layout (creates new views)

            if (wasSettings)
            {
                ShowView(settingsForm);
            }
            else
            {
                ShowView(membersView);
                membersView.InitDefaultState();
            }
        }

        public void ShowMembersView()
        {
            ShowView(membersView);
        }


        private void InitializeLayout()
        {
            // Load settings once
            bool useDarkMode = false;
            UiScaleMode scale = UiScaleMode.Normal;

            using (AppDbContext db = new AppDbContext())
            {
                AppSetting? s = db.Settings.FirstOrDefault();
                if (s != null)
                {
                    useDarkMode = s.UseDarkMode;
                    scale = s.ScaleMode;
                }
            }

            // Apply scale first (before creating controls)
            ApplyScale(this, scale);

            // Create views once
            membersView = new MembersView { Dock = DockStyle.Fill };
            settingsForm = new SettingsView { Dock = DockStyle.Fill };

            // Hook events once
            settingsForm.UiScaleChanged += newScale =>
            {
                SaveScaleModeToDb(newScale);
                ReloadMainFormWithCurrentView();
            };

            settingsForm.DarkModeChanged += useDark =>
            {
                if (useDark)
                {
                    ApplyDarkTheme(this);
                }
                else
                {
                    ApplyLightTheme(this);
                }

                Refresh();
                RefreshGridThemes(); // 🔥 ensure all grids restyle after toggle
            };

            // Set form scaling and state
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = false;
            WindowState = FormWindowState.Maximized;

            // Create main menu
            menuStrip = new MenuStrip();
            menuStrip.Font = new Font("Segoe UI", 11, FontStyle.Regular);

            if (useDarkMode)
            {
                menuStrip.Renderer = new DarkMenuRenderer();  // ← custom renderer that fixes white-on-white
                menuStrip.BackColor = Color.FromArgb(45, 45, 48);
                menuStrip.ForeColor = Color.White;
            }

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("ΑΡΧΕΙΟ");
            fileMenu.DropDownItems.Add("ΕΞΟΔΟΣ", null, (_, _) => Application.Exit());

            ToolStripMenuItem viewMenu = new ToolStripMenuItem("ΠΡΟΒΟΛΗ");
            viewMenu.DropDownItems.Add("ΜΕΛΗ", null, (_, _) => ShowView(membersView));
            viewMenu.DropDownItems.Add("ΡΥΘΜΙΣΕΙΣ", null, (_, _) => ShowView(settingsForm));

            ToolStripMenuItem helpMenu = new ToolStripMenuItem("ΒΟΗΘΕΙΑ");
            helpMenu.DropDownItems.Add("📘 ΒΟΗΘΕΙΑ", null, (_, _) =>
            {
                string pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "help.pdf");
                if (File.Exists(pdfPath))
                {
                    Process.Start(new ProcessStartInfo { FileName = pdfPath, UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Δεν βρέθηκε το αρχείο βοήθειας.", "ΣΦΑΛΜΑ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });

            helpMenu.DropDownItems.Add("ℹ️ ΠΛΗΡΟΦΟΡΙΕΣ", null, (_, _) =>
            {
                using AboutDialog about = new();
                about.ShowDialog(this);
            });

            helpMenu.DropDownItems.Add(new ToolStripSeparator());

            helpMenu.DropDownItems.Add("📄 ΑΔΕΙΑ ΧΡΗΣΗΣ", null, (_, _) =>
            {
                using LicenseDialog license = new();
                license.ShowDialog(this);
            });

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(viewMenu);
            menuStrip.Items.Add(QueryMenuBuilder.Build(this)); // QUERIES
            menuStrip.Items.Add(helpMenu);

            // Create layout grid
            layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // for menu
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // for mainPanel

            // Main panel to host active views
            mainPanel = new Panel { Dock = DockStyle.Fill };

            layout.Controls.Add(menuStrip, 0, 0);
            layout.Controls.Add(mainPanel, 0, 1);
            Controls.Add(layout);

            MainMenuStrip = menuStrip;

            // Apply theme AFTER views are created
            if (useDarkMode)
            {
                ApplyDarkTheme(this);
            }
            else
            {
                ApplyLightTheme(this);
            }

            // Show default view
            ShowView(membersView);
            this.Shown += (_, _) => membersView.InitDefaultState();
        }


        private void ShowView(Control control)
        {
            lastViewShown = control is SettingsView ? CurrentView.Settings : CurrentView.Members;
            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(control);
        }

        private void RefreshGridThemes()
        {
            membersView?.ApplyGridStyles(membersView.memberGrid);
            membersView?.ApplyGridStyles(membersView.childMembershipPanel.childGrid);
            membersView?.ApplyGridStyles(membersView.childMembershipPanel.membershipGrid);
        }


        private void ApplyDarkTheme(Control root)
        {
            root.BackColor = Color.FromArgb(32, 32, 32);
            root.ForeColor = Color.White;

            foreach (Control control in root.Controls)
            {
                if (control is DataGridView dgv)
                {
                    dgv.BackgroundColor = Color.FromArgb(45, 45, 48);
                    dgv.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                    dgv.DefaultCellStyle.ForeColor = Color.White;
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
                    dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(62, 62, 66);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.Invalidate();
                }
                else if (control is MenuStrip menu)
                {
                    menu.Renderer = new DarkMenuRenderer();
                    menu.BackColor = Color.FromArgb(45, 45, 48);  // dark background
                    menu.ForeColor = Color.White;                 // white text

                    foreach (ToolStripMenuItem item in menu.Items)
                    {
                        item.BackColor = Color.FromArgb(45, 45, 48);
                        item.ForeColor = Color.White;
                        foreach (ToolStripItem subItem in item.DropDownItems)
                        {
                            subItem.BackColor = Color.FromArgb(50, 50, 55);
                            subItem.ForeColor = Color.White;
                        }
                    }
                }


                ApplyDarkTheme(control);
            }

        }

        private void ApplyLightTheme(Control root)
        {
            root.BackColor = SystemColors.Control;
            root.ForeColor = SystemColors.ControlText;

            foreach (Control c in root.Controls)
            {
                if (c is DataGridView dgv)
                {
                    dgv.BackgroundColor = Color.White;
                    dgv.DefaultCellStyle.BackColor = Color.White;
                    dgv.DefaultCellStyle.ForeColor = Color.Black;

                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                    dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.Black;

                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

                    dgv.DefaultCellStyle.SelectionBackColor = Color.DarkSlateBlue;
                    dgv.DefaultCellStyle.SelectionForeColor = Color.White;

                    dgv.EnableHeadersVisualStyles = false;
                    dgv.Invalidate();
                }

                ApplyLightTheme(c);
            }
        }

        private void ApplyScale(Control control, UiScaleMode scale)
        {
            float fontSize = scale switch
            {
                UiScaleMode.Small => 8.5f,
                UiScaleMode.Normal => 10f,
                UiScaleMode.Large => 12f,
                _ => 10f
            };

            try
            {
                control.Font = new Font("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Point);
            }
            catch (ArgumentException)
            {
                // Fallback in case Segoe UI is not found
                control.Font = SystemFonts.DefaultFont;
            }

            foreach (Control c in control.Controls)
            {
                ApplyScale(c, scale);
            }
        }



    }
}
