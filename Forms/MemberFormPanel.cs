namespace SYLOGOS.Forms
{
    public class MemberFormPanel : Panel
    {
        public TextBox txtMemberNumber;
        public TextBox txtFullName;
        public TextBox txtSpouseFullName;
        public TextBox txtSpousePhone;
        public TextBox txtMemberPhone;
        public TextBox txtEmail;
        public TextBox txtCertificateNumber;
        public TextBox txtCertificatePublisher;
        public TextBox txtNotes;
        public TextBox txtCity;
        public TextBox txtAddress;
        public TextBox txtRegistrationDate;
        public Button btnSave, btnNew, btnDelete, btnClear;

        public MemberFormPanel()
        {
            Dock = DockStyle.Top;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);
            InitializeForm();
        }

        private void InitializeForm()
        {
            TableLayoutPanel formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 8,
                AutoSize = true
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            static Label MakeLabel(string text)
            {
                return new()
                {
                    Text = text,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight
                };
            }

            formLayout.Controls.Add(MakeLabel("Αριθμός Μέλους:"), 0, 0);
            txtMemberNumber = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            formLayout.Controls.Add(txtMemberNumber, 1, 0);

            formLayout.Controls.Add(MakeLabel("Ημερομηνία Εγγραφής:"), 2, 0);
            txtRegistrationDate = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            formLayout.Controls.Add(txtRegistrationDate, 3, 0);

            formLayout.Controls.Add(MakeLabel("Ονοματεπώνυμο:"), 0, 1);
            txtFullName = new TextBox { Dock = DockStyle.Fill };
            txtFullName.TextChanged += (_, _) =>
                // tell parent view to re-check Save-button state
                (this.Parent as MembersView)?.UpdateSaveButtonState();
            formLayout.Controls.Add(txtFullName, 1, 1);

            formLayout.Controls.Add(MakeLabel("Ονοματεπώνυμοe Συζύγου:"), 2, 1);
            txtSpouseFullName = new TextBox { Dock = DockStyle.Fill };
            formLayout.Controls.Add(txtSpouseFullName, 3, 1);

            formLayout.Controls.Add(MakeLabel("Τηλέφωνο:"), 0, 2);
            txtMemberPhone = new TextBox { Dock = DockStyle.Fill };
            formLayout.Controls.Add(txtMemberPhone, 1, 2);

            formLayout.Controls.Add(MakeLabel("Τηλέφωνο Συζύγου:"), 2, 2);
            txtSpousePhone = new TextBox { Dock = DockStyle.Fill };
            formLayout.Controls.Add(txtSpousePhone, 3, 2);

            formLayout.Controls.Add(MakeLabel("Email:"), 0, 3);
            txtEmail = new TextBox { Dock = DockStyle.Fill };
            formLayout.Controls.Add(txtEmail, 1, 3);

            formLayout.Controls.Add(MakeLabel("Πόλη:"), 2, 3);
            txtCity = new TextBox { Dock = DockStyle.Fill };
            formLayout.Controls.Add(txtCity, 3, 3);

            formLayout.Controls.Add(MakeLabel("Διεύθυνση:"), 0, 4);
            txtAddress = new TextBox { Dock = DockStyle.Fill };
            formLayout.Controls.Add(txtAddress, 1, 4);

            formLayout.Controls.Add(MakeLabel("Αριθ. Πιστοποιητικού Οικ. Κατ.:"), 2, 4);
            txtCertificateNumber = new TextBox { Dock = DockStyle.Fill };
            formLayout.Controls.Add(txtCertificateNumber, 3, 4);

            formLayout.Controls.Add(new Label
            {
                Text = "Εκδότης Πιστοποιητικού Οικ. Κατ.:",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopRight
            }, 0, 5);
            txtCertificatePublisher = new TextBox { Dock = DockStyle.Fill };
            formLayout.Controls.Add(txtCertificatePublisher, 1, 5);

            formLayout.Controls.Add(new Label
            {
                Text = "Σημειώσεις:",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopRight
            }, 2, 5);

            txtNotes = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Height = txtFullName.Font.Height * 4 + 12 // ≈ 4 lines
            };

            formLayout.Controls.Add(txtNotes, 3, 5);

            //formLayout.Controls.Add(new Label(), 2, 4);
            //formLayout.Controls.Add(new Label(), 3, 4);

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 5, 0, 0),
                Margin = new Padding(0, 10, 0, 0)
            };

            btnNew = new Button
            {
                Text = "➕ Νέα Εγγραφή",
                AutoSize = true,
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnSave = new Button
            {
                Text = "💾 Αποθήκευση",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                MinimumSize = new Size(80, 30),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };

            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.ForestGreen;
            btnSave.FlatAppearance.MouseDownBackColor = Color.DarkGreen;

            // force color reapply
            btnSave.ForeColor = Color.White;
            btnSave.Invalidate();


            btnClear = new Button
            {
                Text = "🧹 Εκκαθάριση",
                AutoSize = true,
                BackColor = Color.Gainsboro,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };

            btnDelete = new Button
            {
                Text = "❌ Διαγραφή",
                AutoSize = true,
                BackColor = Color.Firebrick,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            static Control BoxSpacer(int width)
            {
                return new Label { Width = width };
            }


            for (int i = 0; i < formLayout.RowCount; i++)
            {
                formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            formLayout.RowStyles.Clear();
            formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 0
            formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 1
            formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 2
            formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 3
            formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 4
            formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 5 — Certificate + Notes
            formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 6 — buttons


            buttonPanel.Controls.Add(btnNew);
            buttonPanel.Controls.Add(BoxSpacer(10));
            buttonPanel.Controls.Add(btnSave);
            btnSave.ForeColor = Color.White;
            buttonPanel.Controls.Add(BoxSpacer(10));
            buttonPanel.Controls.Add(btnClear);
            buttonPanel.Controls.Add(BoxSpacer(10));
            buttonPanel.Controls.Add(btnDelete);

            formLayout.Controls.Add(buttonPanel, 1, 6);
            formLayout.SetColumnSpan(buttonPanel, 3);

            Controls.Add(formLayout);
        }
    }
}
