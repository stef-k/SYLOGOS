using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SYLOGOS.Models;
using SYLOGOS.Util;
using System.ComponentModel;

namespace SYLOGOS.Forms
{
    public partial class MembersView : UserControl
    {
        private AppDbContext db;
        private readonly BindingSource _memberSource = new BindingSource();
        private Panel filterPanel;
        private TextBox searchBox;
        private MemberFormPanel memberFormPanel;
        public ChildMembershipPanel childMembershipPanel;
        public DataGridView memberGrid;
        private ComboBox filterMode;
        private System.Windows.Forms.Timer searchDebounceTimer;
        private string lastSearchText = "";
        private Label resultsLabel;
        private Label gridTitle;

        private Member? currentMember;
        private BindingList<Child> children = new();
        private BindingList<Membership> memberships = new();
        private bool isInitializing = false;
        private bool hasLoaded = false;
        private int? pendingSelectionMemberNumber = null;

        private ContextMenuStrip memberContextMenu;
        private ContextMenuStrip childContextMenu;
        private ContextMenuStrip membershipContextMenu;

        public MembersView()
        {
            InitializeComponent();
            WireValidationEvents();
            memberGrid.DataSource = _memberSource;
            memberGrid.DataBindingComplete += MemberGrid_DataBindingComplete;
            memberGrid.SelectionChanged += MemberGrid_SelectionChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!hasLoaded)
            {
                hasLoaded = true;
            }
        }

        private void WireValidationEvents()
        {
            foreach (TextBox? tb in new[]
            {
                memberFormPanel.txtFullName,
                memberFormPanel.txtSpouseFullName,
                memberFormPanel.txtMemberPhone,
                memberFormPanel.txtSpousePhone,
                memberFormPanel.txtEmail,
                memberFormPanel.txtCity,
                memberFormPanel.txtAddress,
                memberFormPanel.txtCertificateNumber,
                memberFormPanel.txtCertificatePublisher,
                memberFormPanel.txtNotes
            })
            {
                tb.TextChanged += (_, _) => UpdateSaveButtonState();
            }
        }



        /// <summary>
        /// Enables Save only when the Full Name is non-empty.
        /// </summary>
        public void UpdateSaveButtonState()
        {
            bool canSave = !string.IsNullOrWhiteSpace(memberFormPanel.txtFullName.Text);
            memberFormPanel.btnSave.Enabled = canSave;
        }

        public void ApplyGridStyles(DataGridView grid)
        {
            using AppDbContext db = new();
            bool dark = db.Settings.FirstOrDefault()?.UseDarkMode ?? false;

            if (dark)
            {
                grid.BackgroundColor = Color.FromArgb(45, 45, 48);
                grid.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                grid.DefaultCellStyle.ForeColor = Color.White;

                grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
                grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;

                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(62, 62, 66);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            }
            else
            {
                grid.BackgroundColor = Color.White;
                grid.DefaultCellStyle.BackColor = Color.White;
                grid.DefaultCellStyle.ForeColor = Color.Black;

                grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.Black;

                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            }

            grid.DefaultCellStyle.SelectionBackColor = Color.DarkSlateBlue;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.EnableHeadersVisualStyles = false;
        }

        private void ApplyHighlightingToGrid(DataGridView grid, string highlight, Func<object?, bool> isMatch)
        {
            using AppDbContext db = new();
            bool dark = db.Settings.FirstOrDefault()?.UseDarkMode ?? false;

            foreach (DataGridViewRow row in grid.Rows)
            {
                bool match = isMatch(row.DataBoundItem);

                row.DefaultCellStyle.BackColor = match
                    ? (dark ? Color.FromArgb(90, 90, 0) : Color.LightGoldenrodYellow)
                    : (row.Index % 2 == 0
                        ? (dark ? Color.FromArgb(45, 45, 48) : Color.White)
                        : (dark ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 240, 240)));
            }
        }


        private void InitializeSearchPanel()
        {
            filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 5,
                RowCount = 2,
                Padding = new Padding(10, 8, 0, 0),
                AutoSize = true,
                Margin = new Padding(10, 0, 0, 0) // ← aligns exactly with MemberFormPanel
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250)); // label column
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300)); // filter mode
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300)); // search box
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // search button
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // clear button

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label filterLabel = new Label
            {
                Text = "Αναζήτηση / Φιλτράρισμα με:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = true
            };

            filterMode = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };
            filterMode.Items.AddRange(new object[]
            {
                "Αρ. Μέλους", "Ονοματεπώνυμο", "Τηλ. Μέλους", "Όνομα Συζύγου", "Τηλ. Συζύγου", "Email", "Πόλη",
                "Έτος Εγγραφής", "Μήνας Εγγραφής", "Όνομα Τέκνου", "Έτος Συνδρομής", "Αρ. Πιστοποιητικού"
            });
            filterMode.SelectedIndex = 0;

            searchBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Αναζήτηση..."
            };

            Button searchBtn = new Button
            {
                Text = "🔍 Αναζήτηση",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(6, 2, 6, 2),
                Margin = new Padding(10, 0, 0, 0)
            };
            searchBtn.Click += (_, _) => LoadMembers(searchBox.Text, filterMode.SelectedItem?.ToString());

            Button clearBtn = new Button
            {
                Text = "❌ Εκκαθάριση",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(6, 2, 6, 2),
                Margin = new Padding(10, 0, 0, 0)
            };
            clearBtn.Click += (_, _) =>
            {
                searchBox.Text = "";
                filterMode.SelectedIndex = 0;
                LoadMembers();
            };

            resultsLabel = new Label
            {
                Text = "",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.DarkSlateGray,
                Padding = new Padding(0, 8, 0, 2),
                Margin = new Padding(0, 0, 0, 2)
            };

            // Row 0: filter controls
            layout.Controls.Add(filterLabel, 0, 0);
            layout.Controls.Add(filterMode, 1, 0);
            layout.Controls.Add(searchBox, 2, 0);
            layout.Controls.Add(searchBtn, 3, 0);
            layout.Controls.Add(clearBtn, 4, 0);

            // Row 1: result label spanning all columns
            layout.Controls.Add(new Label(), 0, 1); // empty cell to preserve column structure
            layout.Controls.Add(resultsLabel, 1, 1);
            layout.SetColumnSpan(resultsLabel, 4); // span columns 1-4

            // Add the layout to the filter panel
            filterPanel.Controls.Clear();
            filterPanel.Controls.Add(layout);

            // Setup debounce
            searchDebounceTimer = new System.Windows.Forms.Timer { Interval = 300 };
            searchDebounceTimer.Tick += (_, _) =>
            {
                searchDebounceTimer.Stop();
                if (searchBox.Text != lastSearchText)
                {
                    lastSearchText = searchBox.Text;
                    LoadMembers(searchBox.Text, filterMode.SelectedItem?.ToString());
                }
            };

            searchBox.TextChanged += (_, _) =>
            {
                if (searchBox.Focused)
                {
                    searchDebounceTimer.Stop();
                    searchDebounceTimer.Start();
                }
            };

            searchBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    LoadMembers(searchBox.Text, filterMode.SelectedItem?.ToString());
                }
            };

            searchBox.TextChanged += (_, _) =>
            {
                if (searchBox.Focused)
                {
                    LoadMembers(searchBox.Text, filterMode.SelectedItem?.ToString());
                }
            };
        }

        private void InitializeContextMenus()
        {
            // Members
            memberContextMenu = new ContextMenuStrip();
            memberContextMenu.Items.Add("📥 Εξαγωγή Όλων των Μελών", null, (_, _) => ExportAllMembersWithChildren());
            memberContextMenu.Items.Add("🪪 Εκτύπωση Κάρτας Μέλους", null, (_, _) => ExportSelectedMemberCard());
            memberGrid.ContextMenuStrip = memberContextMenu;

            // Memberships
            membershipContextMenu = new ContextMenuStrip();
            membershipContextMenu.Items.Add("📄 Εξαγωγή Απόδειξης Είσπραξης", null, (_, _) => ExportSelectedMembership());
            childMembershipPanel.membershipGrid.ContextMenuStrip = membershipContextMenu;
        }


        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;

            db = new AppDbContext();

            // Filter/Search Panel
            InitializeSearchPanel();

            // Member grid
            memberGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            ApplyGridStyles(memberGrid);
            DataGridViewTextBoxColumn memberNumberCol = new DataGridViewTextBoxColumn
            {
                HeaderText = "Αριθμός Μέλους",
                DataPropertyName = "MemberNumber",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
            };
            memberGrid.Columns.Add(memberNumberCol);
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ονοματεπώνυμο", DataPropertyName = "FullName" });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Τηλέφωνο", DataPropertyName = "MemberPhone" });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Ονοματεπώνυμο Συζύγου",
                DataPropertyName = "SpouseFullName"
            });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Τηλέφωνο Συζύγου",
                DataPropertyName = "SpousePhone"
            });

            DataGridViewTextBoxColumn emailCol = new()
            {
                HeaderText = "Email",
                DataPropertyName = "Email",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                CellTemplate = new CustomLinkCell()
            };
            memberGrid.Columns.Add(emailCol);

            memberGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            memberGrid.ColumnHeadersDefaultCellStyle.Font = new Font(memberGrid.Font, FontStyle.Bold);
            memberGrid.SelectionChanged += MemberGrid_SelectionChanged;
            memberGrid.CellFormatting += MemberGrid_CellFormatting;
            memberGrid.CellContentClick += MemberGrid_CellContentClick;

            memberGrid.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                    memberGrid.Columns[e.ColumnIndex].HeaderText == "Email")
                {
                    object? val = memberGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    string? email = val?.ToString();
                    memberGrid.Cursor = string.IsNullOrWhiteSpace(email) ? Cursors.Default : Cursors.Hand;
                }
            };

            memberGrid.CellMouseLeave += (s, e) =>
            {
                memberGrid.Cursor = Cursors.Default;
            };

            memberGrid.CellToolTipTextNeeded += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                    memberGrid.Columns[e.ColumnIndex].HeaderText == "Email")
                {
                    string? val = memberGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                    e.ToolTipText = string.IsNullOrWhiteSpace(val) ? "" : $"Αποστολή Email στον λογαριασμό: {val}";
                }
            };


            gridTitle = new Label
            {
                Text = "📋 Λίστα Μελών ( 0 )",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 24
            };

            // Panels
            memberFormPanel = new MemberFormPanel();
            childMembershipPanel = new ChildMembershipPanel();

            // Button events
            memberFormPanel.btnNew.Click += BtnNew_Click;
            memberFormPanel.btnSave.Click += BtnSave_Click;
            memberFormPanel.btnDelete.Click += BtnDelete_Click;
            memberFormPanel.btnClear.Click += (_, _) => ClearSelectedMember();
            childMembershipPanel.btnAddChild.Click += (_, _) =>
            {
                if (currentMember == null) { MessageBox.Show("Πρέπει να αποθηκεύσετε το μέλος πρώτα."); return; }
                children.Add(new Child { FullName = "", DateOfBirth = DateTime.Today });
                childMembershipPanel.childGrid.DataSource = children;
                UpdateDetailCounts();
            };
            childMembershipPanel.btnDeleteChild.Click += (_, _) =>
            {
                if (childMembershipPanel.childGrid.CurrentRow?.DataBoundItem is Child c)
                {
                    children.Remove(c);
                    childMembershipPanel.childGrid.DataSource = children;
                    UpdateDetailCounts();
                }
            };
            childMembershipPanel.btnAddMembership.Click += (_, _) =>
            {
                if (currentMember == null)
                {
                    MessageBox.Show("Πρέπει να αποθηκεύσετε το μέλος πρώτα.");
                    return;
                }

                int defaultYear = DateTime.Now.Year;
                int newYear = defaultYear;

                // Step 1: Get list of existing years
                HashSet<int> existingYears = memberships.Select(m => m.Year).ToHashSet();

                // Step 2: Try defaultYear, then defaultYear+1, etc.
                while (existingYears.Contains(newYear))
                {
                    newYear++;
                    if (newYear > defaultYear + 10)
                    {
                        MessageBox.Show("Αδυναμία εισαγωγής πληρωμής — πάρα πολλές μελλοντικές εισαγωγές.", "Εξάντληση Ορίου");
                        return;
                    }
                }

                // 🧮 Step 3: Preview receipt number
                //int nextNumber;
                //using (AppDbContext db = new AppDbContext())
                //{
                //    ReceiptSequence? seq = db.ReceiptSequences.AsNoTracking().FirstOrDefault(r => r.Year == newYear);
                //    if (seq != null)
                //    {
                //        nextNumber = seq.LastIssuedNumber + 1;
                //    }
                //    else
                //    {
                //        // ✅ Count already-saved + in-session memberships
                //        int saved = db.Memberships.Count(m => m.MemberId == currentMember.Id && m.Year == newYear);
                //        int pending = memberships.Count(m => m.Year == newYear);
                //        int alreadyAdded = saved + pending;

                //        int start = db.Settings.FirstOrDefault()?.ReceiptStartNumber ?? 1;
                //        nextNumber = start + alreadyAdded;
                //    }
                //}

                // 🎯 Step 4: Add row
                //Membership ms = new Membership
                //{
                //    Year = newYear,
                //    Amount = 0,
                //    ReceiptYear = newYear,
                //    ReceiptNumber = nextNumber
                //};

                Membership ms = new Membership
                {
                    Year = newYear,
                    Amount = 0,
                    ReceiptYear = null,
                    ReceiptNumber = null
                };

                memberships.Add(ms); // <-- direct binding-safe add

                childMembershipPanel.membershipGrid.Refresh(); // optional visual repaint

                UpdateDetailCounts();
            };


            childMembershipPanel.btnDeleteMembership.Click += (_, _) =>
            {
                if (childMembershipPanel.membershipGrid.CurrentRow?.DataBoundItem is Membership m)
                {
                    memberships.Remove(m);
                    childMembershipPanel.membershipGrid.DataSource = memberships;
                    UpdateDetailCounts();
                }
            };

            // 🔒 Prevent editing Year of finalized receipt
            //childMembershipPanel.membershipGrid.CellBeginEdit += (s, e) =>
            //{
            //    DataGridView grid = childMembershipPanel.membershipGrid;
            //    string colName = grid.Columns[e.ColumnIndex].DataPropertyName;

            //    if (colName == "Year" && grid.Rows[e.RowIndex].DataBoundItem is Membership ms)
            //    {
            //        if (ms.ReceiptNumber != null && ms.ReceiptYear != null)
            //        {
            //            MessageBox.Show("Η χρονιά δεν μπορεί να αλλάξει — έχει εκδοθεί απόδειξη.", "🔒", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //            e.Cancel = true;
            //        }
            //    }
            //};

            //childMembershipPanel.membershipGrid.CellFormatting += (s, e) =>
            //{
            //    DataGridView grid = childMembershipPanel.membershipGrid;

            //    if (e.RowIndex >= 0 &&
            //        grid.Columns[e.ColumnIndex].DataPropertyName == "Year" &&
            //        grid.Rows[e.RowIndex].DataBoundItem is Membership ms &&
            //        ms.ReceiptNumber != null)
            //    {
            //        e.Value = $"🔒 {ms.Year}";
            //        e.FormattingApplied = true;
            //    }
            //};

            //childMembershipPanel.membershipGrid.CellToolTipTextNeeded += (s, e) =>
            //{
            //    DataGridView grid = childMembershipPanel.membershipGrid;

            //    try
            //    {
            //        if (e.RowIndex >= 0 &&
            //            grid.Columns[e.ColumnIndex].DataPropertyName == "Year" &&
            //            grid.Rows[e.RowIndex].DataBoundItem is Membership ms &&
            //            ms.ReceiptNumber != null)
            //        {
            //            e.ToolTipText = "Η χρονιά δεν μπορεί να αλλάξει — έχει εκδοθεί απόδειξη.";
            //        }
            //    }
            //    catch (Exception)
            //    {

            //        // Ignore any exceptions here, just in case
            //    }
            //};


            ApplyGridStyles(childMembershipPanel.childGrid);
            ApplyGridStyles(childMembershipPanel.membershipGrid);

            Panel scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            scrollContainer.Controls.Add(memberGrid);
            scrollContainer.Controls.Add(gridTitle);
            scrollContainer.Controls.Add(childMembershipPanel);
            scrollContainer.Controls.Add(memberFormPanel);
            scrollContainer.Controls.Add(filterPanel);

            // Add scrollable panel to main view
            Controls.Add(scrollContainer);


            // ensure Save is disabled at startup
            UpdateSaveButtonState();
            // Hook all TextBoxes to uppercase (skipping email)
            EnforceUppercase(this);

            InitializeContextMenus();
        }

        private void MemberGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (memberGrid.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
            {
                bool dark = db.Settings.FirstOrDefault()?.UseDarkMode ?? false;
                bool isSelected = memberGrid.Rows[e.RowIndex].Selected;

                if (isSelected)
                {
                    // Selection color: white (works on both themes)
                    e.CellStyle.ForeColor = Color.White;
                }
                else if (dark)
                {
                    // For dark theme: bright readable blue regardless of row parity
                    e.CellStyle.ForeColor = Color.DeepSkyBlue;
                }
                else
                {
                    // Light theme: default link blue
                    e.CellStyle.ForeColor = Color.Blue;
                }
                e.CellStyle.SelectionForeColor = Color.White;
            }
        }


        private void MemberGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && memberGrid.Columns[e.ColumnIndex].HeaderText == "Email")
            {
                string? email = memberGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = $"mailto:{email}",
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Αδυναμία εκτέλεσης προγράμματος Email:\n{ex.Message}", "Σφάλμα", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MemberGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!pendingSelectionMemberNumber.HasValue)
            {
                memberGrid.ClearSelection();

                // Safely defer the CurrentCell clear
                BeginInvoke((Action)(() =>
                {
                    memberGrid.CurrentCell = null;
                }));
            }
        }

        private void ClearSelectedMember()
        {
            memberGrid.ClearSelection();
            memberGrid.CurrentCell = null;
            currentMember = null;
            ClearDetailForm();
            UpdateSaveButtonState();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                ClearSelectedMember();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void LoadMembers(string filter = "", string? mode = null)
        {
            try
            {
                isInitializing = true;   // suppress SelectionChanged

                // 1) Fetch
                IQueryable<Member> query = db.Members
                    .Include(m => m.Children)
                    .Include(m => m.Memberships);

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string f = filter.ToUpperInvariant();
                    switch (mode)
                    {
                        case "Ονοματεπώνυμο":
                            query = query.Where(m => m.FullName.ToUpper().Contains(f));
                            break;
                        case "Όνομα Συζύγου":
                            query = query.Where(m => (m.SpouseFullName ?? "").ToUpper().Contains(f));
                            break;
                        case "Τηλ. Μέλους":
                            query = query.Where(m => (m.MemberPhone ?? "").Contains(filter));
                            break;
                        case "Τηλ. Συζύγου":
                            query = query.Where(m => (m.SpousePhone ?? "").Contains(filter));
                            break;
                        case "Email":
                            query = query.Where(m => (m.Email ?? "").ToUpper().Contains(f));
                            break;
                        case "Πόλη":
                            query = query.Where(m => (m.City ?? "").ToUpper().Contains(f));
                            break;
                        case "Έτος Εγγραφής":
                            if (int.TryParse(filter, out int year))
                            {
                                query = query.Where(m => m.RegistrationDate.HasValue && m.RegistrationDate.Value.Year == year);
                            }
                            break;
                        case "Μήνας Εγγραφής":
                            if (int.TryParse(filter, out int month) && month is >= 1 and <= 12)
                            {
                                query = query.Where(m => m.RegistrationDate.HasValue && m.RegistrationDate.Value.Month == month);
                            }
                            break;
                        case "Όνομα Τέκνου":
                            query = query.Where(m => m.Children.Any(c => c.FullName.ToUpper().Contains(f)));
                            break;
                        case "Έτος Συνδρομής":
                            if (int.TryParse(filter, out int y))
                            {
                                query = query.Where(m => m.Memberships.Any(ms => ms.Year == y));
                            }
                            break;
                        case "Αρ. Πιστοποιητικού":
                            query = query.Where(m => (m.CertificateNumber ?? "").ToUpper().Contains(f));
                            break;
                        default: // "MemberNumber" exact  match only
                            if (int.TryParse(filter, out int exactNumber))
                            {
                                query = query.Where(m => m.MemberNumber == exactNumber);
                            }
                            else
                            {
                                query = query.Where(m => false); // no match if not a number
                            }
                            break;

                    }
                }

                List<Member> list = query.OrderBy(m => m.FullName).ToList();

                string highlight = filter?.Trim().ToUpperInvariant() ?? "";
                string modeKey = mode?.Trim() ?? "All";

                // 2) Bind with isolation to prevent selection
                memberGrid.DataSource = null;  // <-- prevent automatic row 0 selection
                _memberSource.DataSource = new SortableBindingList<Member>(list);
                memberGrid.DataSource = _memberSource;
                gridTitle.Text = $"📋 Λίστα Μελών ( {list.Count} )";

                // 2.1) Re-select saved member after save, if any
                if (pendingSelectionMemberNumber.HasValue)
                {
                    int target = pendingSelectionMemberNumber.Value;
                    this.BeginInvoke((Action)(() =>
                    {
                        this.SuspendLayout();
                        foreach (DataGridViewRow row in memberGrid.Rows)
                        {
                            if (row.DataBoundItem is Member m && m.MemberNumber == target)
                            {
                                memberGrid.ClearSelection();
                                row.Selected = true;
                                memberGrid.CurrentCell = row.Cells[0];
                                memberGrid.FirstDisplayedScrollingRowIndex = row.Index;

                                // ✅ Ensure memberships and children are reloaded
                                MemberGrid_SelectionChanged(memberGrid, EventArgs.Empty);
                                break;
                            }
                        }
                        this.ResumeLayout();
                        pendingSelectionMemberNumber = null;
                    }));
                }

                resultsLabel.Text = list.Count > 0 ? $"Αποτελέσματα: {list.Count}" : "Δεν βρέθηκαν αποτελέσματα.";

                if (memberGrid.Rows.Count > 0)
                {
                    memberGrid.FirstDisplayedScrollingRowIndex = 0;
                }

                using (AppDbContext db = new AppDbContext())
                {
                    bool dark = db.Settings.FirstOrDefault()?.UseDarkMode ?? false;

                    foreach (DataGridViewRow row in memberGrid.Rows)
                    {
                        if (row.DataBoundItem is Member m && !string.IsNullOrWhiteSpace(highlight))
                        {
                            bool match = modeKey switch
                            {
                                "Αρ. Μέλους" => m.MemberNumber.ToString() == highlight,

                                "Ονοματεπώνυμο" => m.FullName?.ToUpper().Contains(highlight) == true,

                                "Όνομα Συζύγου" => (m.SpouseFullName ?? "")
                                                       .ToUpper()
                                                       .Contains(highlight),

                                "Τηλ. Μέλους" => (m.MemberPhone ?? "").Contains(filter),

                                "Τηλ. Συζύγου" => (m.SpousePhone ?? "").Contains(filter),

                                "Email" => (m.Email ?? "")
                                                       .ToUpper()
                                                       .Contains(highlight),

                                "Πόλη" => (m.City ?? "")
                                                       .ToUpper()
                                                       .Contains(highlight),

                                _ => false
                            };

                            row.DefaultCellStyle.BackColor = match
                                ? (dark ? Color.FromArgb(90, 90, 0) : Color.LightGoldenrodYellow)
                                : (row.Index % 2 == 0
                                    ? (dark ? Color.FromArgb(45, 45, 48) : Color.White)
                                    : (dark ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 240, 240)));
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = (row.Index % 2 == 0
                                ? (dark ? Color.FromArgb(45, 45, 48) : Color.White)
                                : (dark ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 240, 240)));
                        }
                    }
                }


                // 4) Defer selection clearing only if no pending selection
                if (!pendingSelectionMemberNumber.HasValue)
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        memberGrid.ClearSelection();
                        memberGrid.CurrentCell = null;
                    }));
                }

                // 5) Wipe out the detail‐panel only if no selection is pending
                if (!pendingSelectionMemberNumber.HasValue)
                {
                    ClearDetailForm();
                }

                // ✅ Always re-enable selection logic after all async layout completes
                this.BeginInvoke(() => isInitializing = false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Αδυναμία φόρτωσης μελών:\n{ex.Message}",
                    "Σφάλμα Φόρτωσης",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        private void ClearDetailForm()
        {
            currentMember = null;

            memberFormPanel.txtMemberNumber.Text = "";
            memberFormPanel.txtRegistrationDate.Text = "";
            memberFormPanel.txtFullName.Text = "";
            memberFormPanel.txtSpouseFullName.Text = "";
            memberFormPanel.txtSpousePhone.Text = "";
            memberFormPanel.txtMemberPhone.Text = "";
            memberFormPanel.txtEmail.Text = "";
            memberFormPanel.txtCity.Text = "";
            memberFormPanel.txtAddress.Text = "";
            memberFormPanel.txtCertificateNumber.Text = "";
            memberFormPanel.txtCertificatePublisher.Text = "";
            memberFormPanel.txtNotes.Text = "";

            children = new BindingList<Child>();
            memberships = new BindingList<Membership>();
            childMembershipPanel.childGrid.DataSource = children;
            childMembershipPanel.membershipGrid.DataSource = memberships;
            UpdateSaveButtonState();
        }



        private void MemberGrid_SelectionChanged(object? sender, EventArgs e)
        {
            // Ignore any selection changes not initiated by the user
            if ((isInitializing && !pendingSelectionMemberNumber.HasValue)
                || memberGrid.SelectedRows.Count == 0
                || memberGrid.CurrentRow == null
                || memberGrid.CurrentRow.Index < 0)
            {
                return;
            }

            if (memberGrid.CurrentRow.DataBoundItem is not Member selected)
            {
                return;
            }

            currentMember = selected;

            memberFormPanel.txtMemberNumber.Text = selected.MemberNumber.ToString();
            memberFormPanel.txtRegistrationDate.Text = selected.RegistrationDate?.ToShortDateString() ?? "";
            memberFormPanel.txtFullName.Text = selected.FullName;
            memberFormPanel.txtSpouseFullName.Text = selected.SpouseFullName ?? "";
            memberFormPanel.txtSpousePhone.Text = selected.SpousePhone ?? "";
            memberFormPanel.txtMemberPhone.Text = selected.MemberPhone ?? "";
            memberFormPanel.txtEmail.Text = selected.Email ?? "";
            memberFormPanel.txtCity.Text = selected.City ?? "";
            memberFormPanel.txtAddress.Text = selected.Address ?? "";
            memberFormPanel.txtCertificateNumber.Text = selected.CertificateNumber ?? "";
            memberFormPanel.txtCertificatePublisher.Text = selected.CertificatePublisher ?? "";
            memberFormPanel.txtNotes.Text = selected.Notes ?? "";

            children = new SortableBindingList<Child>(db.Children.Where(c => c.MemberId == selected.Id).ToList());
            memberships = new SortableBindingList<Membership>(db.Memberships.Where(m => m.MemberId == selected.Id).ToList());

            childMembershipPanel.childGrid.DataSource = children;
            childMembershipPanel.membershipGrid.DataSource = memberships;
            UpdateDetailCounts();

            string highlight = searchBox.Text.Trim().ToUpperInvariant();

            ApplyHighlightingToGrid(childMembershipPanel.childGrid, highlight, row =>
            {
                return row is Child c && !string.IsNullOrWhiteSpace(highlight) && c.FullName.ToUpper().Contains(highlight);
            });

            ApplyHighlightingToGrid(childMembershipPanel.membershipGrid, highlight, row =>
            {
                return row is Membership ms && int.TryParse(highlight, out int y) && ms.Year == y;
            });

        }



        private void BtnNew_Click(object? sender, EventArgs e)
        {
            currentMember = null;
            memberFormPanel.txtMemberNumber.Text = "";
            memberFormPanel.txtRegistrationDate.Text = "";
            memberFormPanel.txtFullName.Text = "";
            memberFormPanel.txtSpouseFullName.Text = "";
            memberFormPanel.txtSpousePhone.Text = "";
            memberFormPanel.txtMemberPhone.Text = "";
            memberFormPanel.txtEmail.Text = "";
            memberFormPanel.txtCity.Text = "";
            memberFormPanel.txtAddress.Text = "";
            memberFormPanel.txtCertificateNumber.Text = "";
            memberFormPanel.txtCertificatePublisher.Text = "";
            memberFormPanel.txtNotes.Text = "";


            children = new SortableBindingList<Child>();
            memberships = new SortableBindingList<Membership>();
            childMembershipPanel.childGrid.DataSource = children;
            childMembershipPanel.membershipGrid.DataSource = memberships;
            UpdateDetailCounts();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            try
            {
                if (currentMember == null)
                {
                    MessageBox.Show("Δεν έχει επιλεγεί μέλος.", "Σφάλμα Διαγραφής", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show("Να διαγραφεί αυτό το μέλος;", "Επιβεβαίωση Διαγραφής", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                db.Members.Remove(currentMember);
                db.SaveChanges();

                // Refresh grid, clear form & disable Save
                LoadMembers();
                UpdateSaveButtonState();
                UpdateDetailCounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Προέκυψε σφάλμα κατά τη διαγραφή:\n" + ex.Message,
                    "Σφάλμα Διαγραφής",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Wrap entire save in try/catch for robust error handling
            try
            {
                // --- VALIDATION BLOCK ---
                // Children validation
                List<string> childErrors = new List<string>();
                HashSet<string> seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < children.Count; i++)
                {
                    Child c = children[i];
                    string name = c.FullName?.Trim() ?? "";
                    bool rowError = false;

                    if (string.IsNullOrEmpty(name))
                    {
                        childErrors.Add($"Γραμμή {i + 1}: Το όνομα απαιτείται");
                        childMembershipPanel.childGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }
                    else if (!seenNames.Add(name))
                    {
                        childErrors.Add($"Γραμμή {i + 1}: Διπλότυπο όνομα '{name}'");
                        childMembershipPanel.childGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }

                    if (c.DateOfBirth.Year < 1900)
                    {
                        childErrors.Add($"Γραμμή {i + 1}: Λάθος ημερομηνίας γεννήσεως '{c.DateOfBirth:yyyy}'");
                        childMembershipPanel.childGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }

                    if (rowError && childMembershipPanel.childGrid.FirstDisplayedScrollingRowIndex == 0)
                    {
                        childMembershipPanel.childGrid.FirstDisplayedScrollingRowIndex = i;
                    }
                }

                // Membership validation
                List<string> membershipErrors = new List<string>();
                HashSet<int> seenYears = new HashSet<int>();
                for (int i = 0; i < memberships.Count; i++)
                {
                    Membership m = memberships[i];
                    bool rowError = false;

                    if (!seenYears.Add(m.Year))
                    {
                        membershipErrors.Add($"Γραμμή {i + 1}: Διπλότυπη χρονιά {m.Year}");
                        childMembershipPanel.membershipGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }
                    if (m.Year < 1900 || m.Year > DateTime.Now.Year + 1)
                    {
                        membershipErrors.Add($"Γραμμή {i + 1}: Το έτος {m.Year} είναι εκτός ορίων");
                        childMembershipPanel.membershipGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }
                    if (m.Amount < 0)
                    {
                        membershipErrors.Add($"Γραμμή {i + 1}: Το ποσό δεν μπορεί να είναι αρνητικός αριθμός");
                        childMembershipPanel.membershipGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }
                    if (rowError && childMembershipPanel.membershipGrid.FirstDisplayedScrollingRowIndex == 0)
                    {
                        childMembershipPanel.membershipGrid.FirstDisplayedScrollingRowIndex = i;
                    }
                    // 🛡 Prevent duplicate membership years
                    List<int> duplicateYears = memberships
                        .GroupBy(m => m.Year)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();

                    if (duplicateYears.Any())
                    {
                        MessageBox.Show(
                            $"Κάθε πληρωμή συνδρομής πρέπει να είναι μοναδική για το έτος. Βρέθηκαν διπλότυπα για: {string.Join(", ", duplicateYears)}",
                            "Διπλότυπα Έτη",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }

                }

                // Aggregate and display errors
                List<string> allErrors = childErrors.Concat(membershipErrors).ToList();
                if (allErrors.Any())
                {
                    MessageBox.Show(
                        "Παρακαλώ διορθώστε τα παρακάτω σφάλματα πριν την αποθήκευση:\n\n" +
                        string.Join("\n", allErrors),
                        "Σφάλματα Δεδομένων",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                // --- END VALIDATION ---

                // Basic Full Name guard
                if (string.IsNullOrWhiteSpace(memberFormPanel.txtFullName.Text))
                {
                    MessageBox.Show("Το Ονοματεπώνυμο είναι υποχρεωτικό.", "Σφάλμα Δεδομένων", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // --- MANUAL MEMBER NUMBER & REGISTRATION DATE ---
                if (!int.TryParse(memberFormPanel.txtMemberNumber.Text?.Trim(), out int manualNumber) || manualNumber <= 0)
                {
                    MessageBox.Show("Ο Αριθμός Μέλους πρέπει να είναι θετικός ακέραιος.", "Σφάλμα Δεδομένων",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // must be unique across all members (excluding the one we're editing)
                bool numberTaken = db.Members.Any(m => m.MemberNumber == manualNumber &&
                                                       (currentMember == null || m.Id != currentMember.Id));
                if (numberTaken)
                {
                    MessageBox.Show($"Ο Αριθμός Μέλους {manualNumber} υπάρχει ήδη.", "Διπλότυπος Αριθμός",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Member member = currentMember ?? new Member();
                member.MemberNumber = manualNumber;

                // registration date: user-edited; allow empty (null) or any valid date
                member.RegistrationDate = DateTime.TryParse(memberFormPanel.txtRegistrationDate.Text?.Trim(), out DateTime reg) ? reg.Date : null;

                // --- END MANUAL MEMBER NUMBER & REGISTRATION DATE ---

                // Copy fields
                member.FullName = memberFormPanel.txtFullName.Text.Trim();
                member.SpouseFullName = memberFormPanel.txtSpouseFullName.Text.Trim();
                member.SpousePhone = memberFormPanel.txtSpousePhone.Text.Trim();
                member.MemberPhone = memberFormPanel.txtMemberPhone.Text.Trim();
                member.Email = memberFormPanel.txtEmail.Text.Trim();
                member.City = memberFormPanel.txtCity.Text.Trim();
                member.Address = memberFormPanel.txtAddress.Text.Trim();
                member.CertificateNumber = memberFormPanel.txtCertificateNumber.Text.Trim();
                member.CertificatePublisher = memberFormPanel.txtCertificatePublisher.Text.Trim();
                member.Notes = memberFormPanel.txtNotes.Text.Trim();

                if (currentMember == null)
                {
                    db.Members.Add(member);
                    db.SaveChanges(); // ✅ Save now to get member.Id for FK relations
                }
                else
                {
                    db.Entry(member).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                // Remove existing children and re-add fresh copies to avoid primary-key conflicts
                db.Children.RemoveRange(db.Children.Where(c => c.MemberId == member.Id));
                foreach (Child child in children)
                {
                    Child newChild = new Child
                    {
                        FullName = child.FullName?.Trim() ?? "",
                        DateOfBirth = child.DateOfBirth,
                        MemberId = member.Id
                    };
                    db.Children.Add(newChild);
                }

                // Remove and re-add memberships as brand-new objects
                db.Memberships.RemoveRange(db.Memberships.Where(m => m.MemberId == member.Id));

                foreach (Membership ms in memberships)
                {

                    int? mNumber = ms.ReceiptNumber;
                    int? manualReceiptYear = mNumber.HasValue ? ms.Year : (int?)null;

                    Membership newMs = new()
                    {
                        Year = ms.Year,
                        Amount = ms.Amount,                 // 0 allowed
                        ReceiptNumber = mNumber,       // user-provided
                        ReceiptYear = manualReceiptYear,    // set iff number exists
                        MemberId = member.Id
                    };
                    db.Memberships.Add(newMs);
                }


                db.SaveChanges();
                // get the current member again to ensure we have the latest state
                pendingSelectionMemberNumber = member.MemberNumber;
                LoadMembers();
            }
            catch (Exception ex)
            {
                string message = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show(
                    "Παρουσιάστηκε σφάλμα κατά την αποθήκευση:\n" + message,
                    "Σφάλμα Αποθήκευσης",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }


        private int GenerateNextMemberNumber()
        {
            return db.Members.Select(m => m.MemberNumber).DefaultIfEmpty().Max() + 1;
        }

        /// <summary>
        /// Resets the view to “new member” mode with no selection.
        /// </summary>
        public void InitDefaultState()
        {
            // 🔒 Detach all bound data BEFORE disposing the context
            memberGrid.DataSource = null;
            _memberSource.DataSource = null;

            db?.Dispose(); // Clean up old context
            db = new AppDbContext(); // Reassign

            LoadMembers(); // Safe now

            currentMember = null;
            memberGrid.ClearSelection();
            BtnNew_Click(null, null);
            UpdateSaveButtonState();
        }




        /// <summary>
        /// Recursively walks all child controls and, for each TextBox,
        /// hooks TextChanged to force uppercase characters.
        /// Skips the email field if you wish to preserve lower-case there.
        /// </summary>
        private void EnforceUppercase(Control parent)
        {
            foreach (Control ctl in parent.Controls)
            {
                if (ctl is TextBox tb && tb != memberFormPanel.txtEmail)
                {
                    tb.CharacterCasing = CharacterCasing.Upper;
                }
                // Recurse into containers
                if (ctl.HasChildren)
                {
                    EnforceUppercase(ctl);
                }
            }
        }

        private void UpdateDetailCounts()
        {
            if (childMembershipPanel.Controls.Find("childrenTitleLabel", true).FirstOrDefault() is Label c)
            {
                c.Text = $"👶 Τέκνα ({children.Count})";
            }
            if (childMembershipPanel.Controls.Find("membershipTitleLabel", true).FirstOrDefault() is Label m)
            {
                m.Text = $"💳 Συνδρομές Μελών ({memberships.Count})";
            }
        }

        /// <summary>
        /// Exports a list of all members along with their associated children and membership details to an Excel file.
        /// </summary>
        /// <remarks>This method retrieves all members from the database, including their children and
        /// membership information,  and organizes the data into a structured format for export. Each member's details,
        /// including their children  (if any), are included in the output. The exported file is named "All Members and
        /// Children".</remarks>
        private void ExportAllMembersWithChildren()
        {
            try
            {
                using AppDbContext db = new();
                List<Member> members = db.Members
                    .Include(m => m.Children)
                    .Include(m => m.Memberships)
                    .OrderBy(m => m.FullName)
                    .ToList();

                int maxChildren = members.Max(m => m.Children.Count);

                Dictionary<string, Func<Member, object?>> columns = new()
                {
                    ["ΑΡΙΘΜΟΣ ΜΕΛΟΥΣ"] = m => m.MemberNumber,
                    ["ΟΝΟΜΑΤΕΠΩΝΥΜΟ"] = m => m.FullName,
                    ["ΤΗΛΕΦΩΝΟ"] = m => m.MemberPhone,
                    ["ΟΝΟΜΑΤΕΠΩΝΥΜΟ ΣΥΖΥΓΟΥ"] = m => m.SpouseFullName,
                    ["ΤΗΛΕΦΩΝΟ ΣΥΖΥΓΟΥ"] = m => m.SpousePhone,
                    ["ΠΟΛΗ"] = m => m.City,
                    ["ΔΙΕΥΘΥΝΣΗ"] = m => m.Address,
                    ["EMAIL"] = m => m.Email,
                    ["ΑΡΙΘ. ΠΙΣΤΟΠΟΙΗΤΙΚΟΥ"] = m => m.CertificateNumber,
                    ["ΕΚΔΟΤΗΣ"] = m => m.CertificatePublisher,
                    ["ΣΗΜΕΙΩΣΕΙΣ"] = m => m.Notes,
                    ["ΗΜΕΡΟΜΗΝΙΑ ΕΓΓΡΑΦΗΣ"] = m => m.RegistrationDate?.ToString("dd-MM-yyyy"),

                    ["ΤΕΛΕΥΤΑΙΑ ΠΛΗΡΩΜΗ"] = m => m.Memberships
                        .Where(ms => (ms.Amount > 0) || (ms.ReceiptNumber != null && ms.ReceiptNumber > 0))
                        .OrderByDescending(ms => ms.Year)
                        .FirstOrDefault()?.Year,

                    ["ΠΟΣΟ"] = m => m.Memberships
                        .Where(ms => (ms.Amount > 0) || (ms.ReceiptNumber != null && ms.ReceiptNumber > 0))
                        .OrderByDescending(ms => ms.Year)
                        .FirstOrDefault()?.Amount
                };

                for (int i = 0; i < maxChildren; i++)
                {
                    int index = i;
                    columns[$"ΤΕΚΝΟ {i + 1} ΟΝΟΜΑ"] = m => m.Children.Count > index ? m.Children[index].FullName : "";
                    columns[$"ΤΕΚΝΟ {i + 1} ΗΜ/ΝΙΑ ΓΕΝΝΗΣΕΩΣ"] = m => m.Children.Count > index ? m.Children[index].DateOfBirth.ToString("dd-MM-yyyy") : "";
                }

                ExportHelper.ExportExcelWithNotice(members, "Όλα τα Μέλη", columns);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Αποτυχία εξαγωγής:\n" + ex.Message, "Σφάλμα Εξαγωγής", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Exports the receipt for the selected membership payment as a PDF file.
        /// </summary>
        /// <remarks>This method retrieves the selected membership payment from the grid, along with the
        /// associated member and application settings, to generate a receipt. The receipt is saved as a PDF file on the
        /// user's desktop with a filename based on the member's name and the membership year. If no membership is
        /// selected, or if the required data cannot be loaded, an appropriate error message is displayed.</remarks>
        private void ExportSelectedMembership()
        {
            if (childMembershipPanel.membershipGrid.CurrentRow?.DataBoundItem is not Membership membership)
            {
                MessageBox.Show("Επιλέξτε μία συνδρομή πρώτα.", "Εξαγωγή", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using AppDbContext db = new();
            Member? member = db.Members.FirstOrDefault(m => m.Id == membership.MemberId);
            AppSetting? settings = db.Settings.FirstOrDefault();

            if (member == null || settings == null)
            {
                MessageBox.Show("Αδυναμία φόρτωσης μέλους ή ρυθμίσεων που απαιτούνται για την εξαγωγή", "Σφάλμα Εξαγωγής", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fileName = $"Απόδειξη_{ExportHelper.SanitizeFileName(member.FullName)}_{membership.Year}.pdf";
            string path = Path.Combine(ExportHelper.GetDesktopPath(), fileName);

            ReceiptDocument doc = new(member, membership, settings);
            doc.GeneratePdf(path);

            MessageBox.Show($"Η απόδειξη αποθηκεύτηκε στο:\n{path}", "Ολοκήρωση Εξαγωγής");
        }

        private void ExportSelectedMemberCard()
        {
            if (currentMember == null)
            {
                MessageBox.Show("Επιλέξτε μέλος.", "Εξαγωγή", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using AppDbContext db = new();
            AppSetting? settings = db.Settings.FirstOrDefault();
            if (settings == null)
            {
                MessageBox.Show("Δεν βρέθηκαν ρυθμίσεις συλλόγου.", "Σφάλμα", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fileName = $"Κάρτα_Μέλους_{ExportHelper.SanitizeFileName(currentMember.FullName)}.pdf";
            string path = Path.Combine(ExportHelper.GetDesktopPath(), fileName);

            MemberCardDocument doc = new(currentMember, settings);
            doc.GeneratePdf(path);

            MessageBox.Show($"Η κάρτα εξήχθη στο:\n{path}", "Ολοκλήρωση");
        }


        /// <summary>
        /// Helper method to load a member by their MemberNumber. From query results dialog.
        /// </summary>
        /// <param name="memberNumber"></param>
        public void LoadMemberByNumber(int memberNumber)
        {
            for (int i = 0; i < memberGrid.Rows.Count; i++)
            {
                if (memberGrid.Rows[i].DataBoundItem is Member m && m.MemberNumber == memberNumber)
                {
                    memberGrid.ClearSelection();
                    memberGrid.Rows[i].Selected = true;
                    memberGrid.CurrentCell = memberGrid.Rows[i].Cells[0];
                    memberGrid.FirstDisplayedScrollingRowIndex = i;

                    // ✅ force selection logic
                    MemberGrid_SelectionChanged(memberGrid, EventArgs.Empty);
                    return;
                }
            }

            LoadMembers();

            for (int i = 0; i < memberGrid.Rows.Count; i++)
            {
                if (memberGrid.Rows[i].DataBoundItem is Member m && m.MemberNumber == memberNumber)
                {
                    memberGrid.ClearSelection();
                    memberGrid.Rows[i].Selected = true;
                    memberGrid.CurrentCell = memberGrid.Rows[i].Cells[0];
                    memberGrid.FirstDisplayedScrollingRowIndex = i;

                    // ✅ force selection logic
                    MemberGrid_SelectionChanged(memberGrid, EventArgs.Empty);
                    return;
                }
            }
        }

    }
}
