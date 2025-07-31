
using Microsoft.EntityFrameworkCore;
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
        private ChildMembershipPanel childMembershipPanel;
        private DataGridView memberGrid;
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

        /// <summary>
        /// Enables Save only when the Full Name is non-empty.
        /// </summary>
        public void UpdateSaveButtonState()
        {
            bool canSave = !string.IsNullOrWhiteSpace(memberFormPanel.txtFullName.Text);
            memberFormPanel.btnSave.Enabled = canSave;
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
                Text = "Search / Filter by:",
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
                "All", "Full Name", "Member Phone", "Spouse Name", "Spouse Phone", "Email", "City",
                "Registration Year", "Registration Month", "Child Name", "Membership Year"
            });
            filterMode.SelectedIndex = 0;

            searchBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Search..."
            };

            Button searchBtn = new Button
            {
                Text = "🔍 Search",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(6, 2, 6, 2),
                Margin = new Padding(10, 0, 0, 0)
            };
            searchBtn.Click += (_, _) => LoadMembers(searchBox.Text, filterMode.SelectedItem?.ToString());

            Button clearBtn = new Button
            {
                Text = "❌ Clear",
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
            memberContextMenu.Items.Add("📄 Export Member Only", null, (_, _) => ExportSelectedMember("member"));
            memberContextMenu.Items.Add("👪 Export Family (Member + Children)", null, (_, _) => ExportSelectedMember("family"));
            memberContextMenu.Items.Add("📦 Export All (Member + Family + Payments)", null, (_, _) => ExportSelectedMember("all"));
            memberGrid.ContextMenuStrip = memberContextMenu;

            // Children
            childContextMenu = new ContextMenuStrip();
            childContextMenu.Items.Add("📄 Export All Children", null, (_, _) => ExportChildrenToPdf());
            childMembershipPanel.childGrid.ContextMenuStrip = childContextMenu;

            // Memberships
            membershipContextMenu = new ContextMenuStrip();
            membershipContextMenu.Items.Add("📄 Export Payment Receipt", null, (_, _) => ExportSelectedMembership());
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
            DataGridViewTextBoxColumn memberNumberCol = new DataGridViewTextBoxColumn
            {
                HeaderText = "Member #",
                DataPropertyName = "MemberNumber",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
            };
            memberGrid.Columns.Add(memberNumberCol);
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Full Name", DataPropertyName = "FullName" });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phone", DataPropertyName = "MemberPhone" });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Spouse Name",
                DataPropertyName = "SpouseFullName"
            });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Spouse Phone",
                DataPropertyName = "SpousePhone"
            });

            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email" });
            memberGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            memberGrid.ColumnHeadersDefaultCellStyle.Font = new Font(memberGrid.Font, FontStyle.Bold);
            memberGrid.SelectionChanged += MemberGrid_SelectionChanged;

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
                if (currentMember == null) { MessageBox.Show("Save the member first."); return; }
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
                if (currentMember == null) { MessageBox.Show("Save the member first."); return; }
                memberships.Add(new Membership { Year = DateTime.Now.Year, Amount = 0 });
                childMembershipPanel.membershipGrid.DataSource = memberships;
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

            Controls.Add(memberGrid);
            Controls.Add(gridTitle);
            Controls.Add(childMembershipPanel);
            Controls.Add(memberFormPanel);
            Controls.Add(filterPanel);
            // ensure Save is disabled at startup
            UpdateSaveButtonState();
            // Hook all TextBoxes to uppercase (skipping email)
            EnforceUppercase(this);

            InitializeContextMenus();
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
                        case "Full Name":
                            query = query.Where(m => m.FullName.ToUpper().Contains(f));
                            break;
                        case "Spouse Name":
                            query = query.Where(m => (m.SpouseFullName ?? "").ToUpper().Contains(f));
                            break;
                        case "Member Phone":
                            query = query.Where(m => (m.MemberPhone ?? "").Contains(filter));
                            break;
                        case "Spouse Phone":
                            query = query.Where(m => (m.SpousePhone ?? "").Contains(filter));
                            break;
                        case "Email":
                            query = query.Where(m => (m.Email ?? "").ToUpper().Contains(f));
                            break;
                        case "City":
                            query = query.Where(m => (m.City ?? "").ToUpper().Contains(f));
                            break;
                        case "Registration Year":
                            if (int.TryParse(filter, out int year))
                            {
                                query = query.Where(m => m.RegistrationDate.HasValue && m.RegistrationDate.Value.Year == year);
                            }
                            break;
                        case "Registration Month":
                            if (int.TryParse(filter, out int month) && month is >= 1 and <= 12)
                            {
                                query = query.Where(m => m.RegistrationDate.HasValue && m.RegistrationDate.Value.Month == month);
                            }
                            break;
                        case "Child Name":
                            query = query.Where(m => m.Children.Any(c => c.FullName.ToUpper().Contains(f)));
                            break;
                        case "Membership Year":
                            if (int.TryParse(filter, out int y))
                            {
                                query = query.Where(m => m.Memberships.Any(ms => ms.Year == y));
                            }
                            break;
                        default: // "All"
                            query = query.Where(m =>
                                m.FullName.ToUpper().Contains(f) ||
                                (m.SpouseFullName ?? "").ToUpper().Contains(f) ||
                                (m.MemberPhone ?? "").Contains(filter) ||
                                (m.SpousePhone ?? "").Contains(filter) ||
                                (m.Email ?? "").ToUpper().Contains(f) ||
                                (m.City ?? "").ToUpper().Contains(f) ||
                                m.Children.Any(c => c.FullName.ToUpper().Contains(f)) ||
                                m.Memberships.Any(ms => ms.Year.ToString() == filter));
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
                                break;
                            }
                        }
                        this.ResumeLayout();
                        pendingSelectionMemberNumber = null;
                    }));
                }

                resultsLabel.Text = list.Count > 0 ? $"Results: {list.Count}" : "No matching results found.";

                if (memberGrid.Rows.Count > 0)
                {
                    memberGrid.FirstDisplayedScrollingRowIndex = 0;
                }

                // Highlight rows based on search criteria
                foreach (DataGridViewRow row in memberGrid.Rows)
                {
                    if (row.DataBoundItem is Member m && !string.IsNullOrWhiteSpace(highlight))
                    {
                        bool match = modeKey switch
                        {
                            "Full Name" => m.FullName.ToUpper().Contains(highlight),
                            "Spouse Name" => (m.SpouseFullName ?? "").ToUpper().Contains(highlight),
                            "Phone" => (m.MemberPhone ?? "").Contains(filter) || (m.SpousePhone ?? "").Contains(filter),
                            "Email" => (m.Email ?? "").ToUpper().Contains(highlight),
                            "City" => (m.City ?? "").ToUpper().Contains(highlight),
                            _ => false
                        };

                        row.DefaultCellStyle.BackColor = match ? Color.LightGoldenrodYellow : Color.White;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
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
                    $"Failed to load members:\n{ex.Message}",
                    "Load Error",
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

            children = new SortableBindingList<Child>(db.Children.Where(c => c.MemberId == selected.Id).ToList());
            memberships = new SortableBindingList<Membership>(db.Memberships.Where(m => m.MemberId == selected.Id).ToList());

            childMembershipPanel.childGrid.DataSource = children;
            childMembershipPanel.membershipGrid.DataSource = memberships;
            UpdateDetailCounts();
        }



        private void BtnNew_Click(object? sender, EventArgs e)
        {
            currentMember = null;
            memberFormPanel.txtMemberNumber.Text = GenerateNextMemberNumber().ToString();
            memberFormPanel.txtRegistrationDate.Text = "";
            memberFormPanel.txtFullName.Text = "";
            memberFormPanel.txtSpouseFullName.Text = "";
            memberFormPanel.txtSpousePhone.Text = "";
            memberFormPanel.txtMemberPhone.Text = "";
            memberFormPanel.txtEmail.Text = "";
            memberFormPanel.txtCity.Text = "";
            memberFormPanel.txtAddress.Text = "";

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
                    MessageBox.Show("No member selected.", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show("Delete this member and all related data?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
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
                    "An error occurred while deleting:\n" + ex.Message,
                    "Error",
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
                        childErrors.Add($"Child row {i + 1}: Name is required");
                        childMembershipPanel.childGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }
                    else if (!seenNames.Add(name))
                    {
                        childErrors.Add($"Child row {i + 1}: Duplicate name '{name}'");
                        childMembershipPanel.childGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }

                    if (c.DateOfBirth.Year < 1900)
                    {
                        childErrors.Add($"Child row {i + 1}: Invalid birth year '{c.DateOfBirth:yyyy}'");
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
                        membershipErrors.Add($"Payment row {i + 1}: Duplicate year {m.Year}");
                        childMembershipPanel.membershipGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }
                    if (m.Year < 1900 || m.Year > DateTime.Now.Year + 1)
                    {
                        membershipErrors.Add($"Payment row {i + 1}: Year {m.Year} out of range");
                        childMembershipPanel.membershipGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }
                    if (m.Amount <= 0)
                    {
                        membershipErrors.Add($"Payment row {i + 1}: Amount must be > 0");
                        childMembershipPanel.membershipGrid.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                        rowError = true;
                    }

                    if (rowError && childMembershipPanel.membershipGrid.FirstDisplayedScrollingRowIndex == 0)
                    {
                        childMembershipPanel.membershipGrid.FirstDisplayedScrollingRowIndex = i;
                    }
                }

                // Aggregate and display errors
                List<string> allErrors = childErrors.Concat(membershipErrors).ToList();
                if (allErrors.Any())
                {
                    MessageBox.Show(
                        "Please correct the following errors before saving:\n\n" +
                        string.Join("\n", allErrors),
                        "Validation Errors",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                // --- END VALIDATION ---

                // Basic Full Name guard
                if (string.IsNullOrWhiteSpace(memberFormPanel.txtFullName.Text))
                {
                    MessageBox.Show("Full Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // --- UNIQUE MEMBER NUMBER GENERATION ---
                Member member;
                if (currentMember == null)
                {
                    int number, attempts = 0;
                    do
                    {
                        number = GenerateNextMemberNumber();
                        attempts++;
                    }
                    while (db.Members.Any(m => m.MemberNumber == number) && attempts < 10);

                    if (attempts == 10)
                    {
                        MessageBox.Show(
                            "Could not generate a unique Member Number. Please try again.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    member = new Member
                    {
                        MemberNumber = number,
                        RegistrationDate = DateTime.Now
                    };
                }
                else
                {
                    member = currentMember;
                    db.Entry(member).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                // Copy fields
                member.FullName = memberFormPanel.txtFullName.Text.Trim();
                member.SpouseFullName = memberFormPanel.txtSpouseFullName.Text.Trim();
                member.SpousePhone = memberFormPanel.txtSpousePhone.Text.Trim();
                member.MemberPhone = memberFormPanel.txtMemberPhone.Text.Trim();
                member.Email = memberFormPanel.txtEmail.Text.Trim();
                member.City = memberFormPanel.txtCity.Text.Trim();
                member.Address = memberFormPanel.txtAddress.Text.Trim();

                if (currentMember == null)
                {
                    db.Members.Add(member);
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
                    Membership newMs = new Membership
                    {
                        Year = ms.Year,
                        Amount = ms.Amount,
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
                MessageBox.Show(
                    "An error occurred while saving:\n" + ex.Message,
                    "Error",
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
                c.Text = $"👶 Children ({children.Count})";
            }
            if (childMembershipPanel.Controls.Find("membershipTitleLabel", true).FirstOrDefault() is Label m)
            {
                m.Text = $"💳 Membership Payments ({memberships.Count})";
            }
        }

        private void ExportSelectedMember(string mode)
        {
            if (memberGrid.CurrentRow?.DataBoundItem is not Member m)
            {
                MessageBox.Show("Select a member first.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // You can switch here based on mode
            switch (mode)
            {
                case "member":
                    MessageBox.Show($"Exporting member only: {m.FullName}");
                    break;
                case "family":
                    MessageBox.Show($"Exporting member and children: {m.FullName}");
                    break;
                case "all":
                    MessageBox.Show($"Exporting all data: {m.FullName}");
                    break;
            }

            // TODO: Add real export logic here
        }

        private void ExportChildrenToPdf()
        {
            if (children.Count == 0)
            {
                MessageBox.Show("No children to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show($"Exporting {children.Count} children to PDF...");
            // TODO: Export children to PDF
        }

        private void ExportSelectedMembership()
        {
            if (childMembershipPanel.membershipGrid.CurrentRow?.DataBoundItem is not Membership m)
            {
                MessageBox.Show("Select a membership payment first.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show($"Exporting receipt for year {m.Year} amount {m.Amount:C2}.");
            // TODO: Export payment receipt PDF
        }


    }
}
