namespace SYLOGOS.Forms
{
    public enum InputType
    {
        Text,
        Numeric,
        Date,
        Checkbox
    }

    public class InputPromptDialog : Form
    {
        private readonly Dictionary<string, Control> _inputs = new();
        public Dictionary<string, object?> Values { get; private set; } = new();

        public InputPromptDialog(string title, string[] labels, InputType[] types)
        {
            if (labels.Length != types.Length)
            {
                throw new ArgumentException("Οι ετικέτες και οι τύποι πρέπει να ταιριάζουν σε μήκος");
            }

            Text = title;
            Width = 500;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(650, 250);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                Padding = new Padding(10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < labels.Length; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // allow height to grow

                Control input;

                if (types[i] == InputType.Checkbox)
                {
                    CheckBox cb = new CheckBox
                    {
                        Text = labels[i],
                        AutoSize = true,
                        MaximumSize = new Size(500, 0),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Anchor = AnchorStyles.Left
                    };
                    input = cb;
                    _inputs.Add(labels[i], cb);

                    layout.Controls.Add(cb, 0, i);
                    layout.SetColumnSpan(cb, 2);
                }
                else
                {
                    Label label = new Label
                    {
                        Text = labels[i] + ":",
                        TextAlign = ContentAlignment.MiddleRight,
                        Dock = DockStyle.Fill
                    };
                    layout.Controls.Add(label, 0, i);

                    input = types[i] switch
                    {
                        InputType.Numeric => CreateNullableNumericUpDown(),
                        InputType.Date => new DateTimePicker { Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill },
                        _ => new TextBox { Dock = DockStyle.Fill }
                    };

                    _inputs.Add(labels[i], input);
                    layout.Controls.Add(input, 1, i);
                }
            }

            Button okButton = new Button { Text = "▶ Εκτέλεση Ερωτήματος", DialogResult = DialogResult.OK, Width = 120 };
            Button cancelButton = new Button { Text = "Ακύρωση", DialogResult = DialogResult.Cancel, Width = 80 };

            FlowLayoutPanel footer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 10, 10),
                AutoSize = true
            };
            footer.Controls.Add(okButton);
            footer.Controls.Add(cancelButton);

            TableLayoutPanel outerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            outerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // layout
            outerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // footer
            outerLayout.Controls.Add(layout, 0, 0);
            outerLayout.Controls.Add(footer, 0, 1);

            Controls.Add(outerLayout);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        private static Control CreateNullableNumericUpDown()
        {
            NumericUpDown num = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 9999,
                Dock = DockStyle.Fill,
                Tag = true
            };

            num.Value = 0;
            num.ForeColor = Color.Gray;
            num.Text = "";

            num.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
                {
                    num.Text = "";
                    e.SuppressKeyPress = true;
                }
            };

            num.Enter += (_, _) =>
            {
                if (num.ForeColor == Color.Gray)
                {
                    num.ForeColor = SystemColors.WindowText;
                }
            };

            return num;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                foreach (KeyValuePair<string, Control> kvp in _inputs)
                {
                    object? val = kvp.Value switch
                    {
                        TextBox tb => string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text,
                        NumericUpDown num => string.IsNullOrWhiteSpace(num.Text) ? null : num.Value,
                        DateTimePicker dt => dt.Value.Date,
                        CheckBox cb => cb.Checked,
                        _ => null
                    };

                    Values[kvp.Key] = val;
                }
            }
            base.OnFormClosing(e);
        }

        public static Dictionary<string, object?>? Show(string title, string[] labels, InputType[] types)
        {
            using InputPromptDialog dlg = new InputPromptDialog(title, labels, types);
            return dlg.ShowDialog() == DialogResult.OK ? dlg.Values : null;
        }
    }
}
