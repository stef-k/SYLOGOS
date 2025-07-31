using SYLOGOS.Models;

namespace SYLOGOS.Forms
{
    public partial class MainForm : Form
    {
        private MenuStrip menuStrip;
        private TableLayoutPanel layout;
        private Panel mainPanel;

        private MembersView membersView;
        private SettingsForm settingsForm;

        public MainForm()
        {
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            bool useDarkMode;
            using (AppDbContext db = new AppDbContext())
            {
                useDarkMode = db.Settings.FirstOrDefault()?.UseDarkMode ?? false;
            }
            if (useDarkMode)
            {
                ApplyDarkTheme(this);
            }

            UiScaleMode scale = UiScaleMode.Normal;
            using (AppDbContext db = new AppDbContext())
            {
                AppSetting? s = db.Settings.FirstOrDefault();
                if (s != null)
                {
                    if (s.UseDarkMode)
                    {
                        ApplyDarkTheme(this);
                    }

                    scale = s.ScaleMode;
                }
            }
            ApplyScale(this, scale);

            // Set base form properties
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = false;
            WindowState = FormWindowState.Maximized;

            // Create views
            membersView = new MembersView { Dock = DockStyle.Fill };
            settingsForm = new SettingsForm { Dock = DockStyle.Fill };
            settingsForm.UiScaleChanged += newScale =>
            {
                ApplyScale(this, newScale);
                PerformLayout();
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
                Refresh(); // Optional: force redraw
            };
            // Menu
            menuStrip = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Exit", null, (_, _) => Application.Exit());

            ToolStripMenuItem viewMenu = new ToolStripMenuItem("View");
            viewMenu.DropDownItems.Add("Members", null, (_, _) => ShowView(membersView));
            viewMenu.DropDownItems.Add("Settings", null, (_, _) => ShowView(settingsForm));

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(viewMenu);

            // TableLayoutPanel with 2 rows
            layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // for menu
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // for mainPanel

            // Main panel to host views
            mainPanel = new Panel { Dock = DockStyle.Fill };

            // Add controls to layout
            layout.Controls.Add(menuStrip, 0, 0);
            layout.Controls.Add(mainPanel, 0, 1);
            Controls.Add(layout);

            // Set main menu for keyboard shortcuts etc.
            MainMenuStrip = menuStrip;

            // Load default view
            ShowView(membersView);
            this.Shown += (_, _) => membersView.InitDefaultState();
        }

        private void ShowView(Control control)
        {
            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(control);
        }

        private void ApplyDarkTheme(Control root)
        {
            root.BackColor = Color.FromArgb(32, 32, 32);
            root.ForeColor = Color.White;

            foreach (Control control in root.Controls)
            {
                ApplyDarkTheme(control);
            }
        }

        private void ApplyLightTheme(Control root)
        {
            root.BackColor = SystemColors.Control;
            root.ForeColor = SystemColors.ControlText;

            foreach (Control c in root.Controls)
            {
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
