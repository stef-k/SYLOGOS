using System.Globalization;

namespace SYLOGOS.Forms
{
    public class ChildMembershipPanel : Panel
    {
        public DataGridView childGrid;
        public DataGridView membershipGrid;
        public Button btnAddChild, btnDeleteChild, btnAddMembership, btnDeleteMembership;

        public ChildMembershipPanel()
        {
            Dock = DockStyle.Top;
            Height = 240;
            Padding = new Padding(5);
            InitializeChildrenAndMembership();
        }

        private void InitializeChildrenAndMembership()
        {
            // --- Children Panel ---
            Label titleLabel = new Label
            {
                Text = "👶 Τέκνα",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Name = "childrenTitleLabel"
            };

            Label helpLabel = new Label
            {
                Text = "Tip: Press F2 or double-click on Date of Birth to use a date picker.",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.DimGray,
                AutoSize = true
            };

            FlowLayoutPanel childButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 0)
            };
            btnAddChild = new Button { Text = "➕ Add Child", BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat, AutoSize = true };
            btnDeleteChild = new Button { Text = "❌ Delete Selected", BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat, AutoSize = true };
            childButtons.Controls.Add(btnAddChild);
            childButtons.Controls.Add(btnDeleteChild);

            childGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };
            childGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Full Name", DataPropertyName = "FullName" });
            CalendarColumn dobColumn = new CalendarColumn
            {
                HeaderText = "Date of Birth",
                DataPropertyName = "DateOfBirth",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            };
            childGrid.Columns.Add(dobColumn);

            childGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            childGrid.ColumnHeadersDefaultCellStyle.Font = new Font(childGrid.Font, FontStyle.Bold);

            TableLayoutPanel childHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                RowCount = 3
            };
            childHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            childHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            childHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            childHeader.Controls.Add(titleLabel);
            childHeader.Controls.Add(helpLabel);
            childHeader.Controls.Add(childButtons);

            // Force uppercase in the “Full Name” cell when editing
            childGrid.EditingControlShowing += (s, e) =>
            {
                if (childGrid.CurrentCell?.OwningColumn.DataPropertyName == "FullName"
                    && e.Control is TextBox tb)
                {
                    tb.CharacterCasing = CharacterCasing.Upper;
                }
            };

            TableLayoutPanel childWrapper = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            childWrapper.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            childWrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            childWrapper.Controls.Add(childHeader);
            childWrapper.Controls.Add(childGrid);

            // --- Membership Panel ---
            Label membershipTitle = new Label
            {
                Text = "💳 Συνδρομές",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Name = "membershipTitleLabel"
            };

            FlowLayoutPanel membershipButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 0)
            };
            btnAddMembership = new Button { Text = "➕ Add Payment", BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat, AutoSize = true };
            btnDeleteMembership = new Button { Text = "❌ Delete Selected", BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat, AutoSize = true };
            membershipButtons.Controls.Add(btnAddMembership);
            membershipButtons.Controls.Add(btnDeleteMembership);

            membershipGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };
            membershipGrid.Columns.Clear();

            membershipGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Year",
                DataPropertyName = "Year",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            membershipGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Amount",
                DataPropertyName = "Amount",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    FormatProvider = new CultureInfo("el-GR"),
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            membershipGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Receipt #",
                DataPropertyName = "ReceiptNumber",
                ValueType = typeof(int?),
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    NullValue = "",
                    Format = "N0"
                }
            });

            membershipGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            membershipGrid.ColumnHeadersDefaultCellStyle.Font = new Font(membershipGrid.Font, FontStyle.Bold);

            TableLayoutPanel membershipHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                RowCount = 3
            };
            // match childHeader’s three AutoSize rows
            membershipHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            membershipHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            membershipHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            membershipHeader.Controls.Add(membershipTitle);
            // spacer
            int spacerHeight = helpLabel.Height;
            Label spacer = new Label
            {
                Text = "",
                AutoSize = false,
                Height = spacerHeight,
                Dock = DockStyle.Top
            };
            membershipHeader.Controls.Add(spacer);
            membershipHeader.Controls.Add(membershipButtons);

            TableLayoutPanel membershipWrapper = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            membershipWrapper.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            membershipWrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            membershipWrapper.Controls.Add(membershipHeader);
            membershipWrapper.Controls.Add(membershipGrid);

            // --- Combined layout ---
            TableLayoutPanel row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            row.Controls.Add(childWrapper, 0, 0);
            row.Controls.Add(membershipWrapper, 1, 0);

            Controls.Add(row);

            childGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            membershipGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        }
    }
}
