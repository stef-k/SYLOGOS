using SYLOGOS.Models;
using SYLOGOS.Util;

namespace SYLOGOS.Forms
{
    public partial class QueryResultDialog : Form
    {
        private readonly MainForm _mainForm;
        private readonly Type _dataType;
        private readonly DataGridView _grid;
        private readonly Button _btnExportExcel;
        private readonly Button _btnLoadSelected;
        private readonly string _queryKey;
        private readonly object[] _queryArgs;

        private QueryResultDialog(MainForm mainForm, object dataSource, Type type, string queryKey, string? summaryText, params object[] args)
        {
            _queryKey = queryKey;
            _queryArgs = args;
            _mainForm = mainForm;
            _dataType = type;

            string localizedTitle = FieldHeaderMapper.GetQueryTitle(_queryKey, args);
            Text = localizedTitle;
            Width = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.9);
            Height = (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.85);
            StartPosition = FormStartPosition.CenterParent;

            Label header = new Label
            {
                Text = localizedTitle,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 5, 0, 0)
            };

            Label? summaryLabel = null;
            if (!string.IsNullOrWhiteSpace(summaryText))
            {
                summaryLabel = new Label
                {
                    Text = summaryText,
                    Dock = DockStyle.Top,
                    Font = new Font("Segoe UI", 11, FontStyle.Italic),
                    ForeColor = Color.DimGray,
                    Height = 24,
                    Padding = new Padding(10, 0, 0, 0)
                };
            }

            object bindingSource = dataSource;
            Type listType = dataSource.GetType();
            if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type itemType = listType.GetGenericArguments()[0];
                Type sortableListType = typeof(SortableBindingList<>).MakeGenericType(itemType);
                bindingSource = Activator.CreateInstance(sortableListType, dataSource)!;
            }

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                DataSource = bindingSource,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True },
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            _grid.DataBindingComplete += (_, _) =>
            {
                foreach (DataGridViewColumn col in _grid.Columns)
                {
                    string propName = col.DataPropertyName;
                    if (!string.IsNullOrWhiteSpace(propName))
                    {
                        col.HeaderText = FieldHeaderMapper.GetHeader(propName, _dataType);
                    }
                }
            };

            _grid.RowPostPaint += (s, e) =>
            {
                DataGridView grid = (DataGridView)s!;
                string rowIdx = (e.RowIndex + 1).ToString();
                StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                Rectangle bounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
                e.Graphics.DrawString(rowIdx, grid.Font, SystemBrushes.ControlText, bounds, center);
            };

            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.CellDoubleClick += (_, _) => TryLoadSelected();

            Label rowCountLabel = new()
            {
                Text = $"Εμφανίζονται {(bindingSource as System.Collections.IList)?.Count ?? 0} αποτελέσματα",
                Dock = DockStyle.Bottom,
                Height = 24,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 10, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.DimGray
            };

            _btnExportExcel = new Button { Text = "Εξαγωγή σε Excel", Dock = DockStyle.Left, Width = 120 };
            _btnLoadSelected = new Button { Text = "Φόρτωση Επιλογής", Dock = DockStyle.Right, Width = 140 };

            _btnExportExcel.Click += (_, _) => ExportToExcel();
            _btnLoadSelected.Click += (_, _) => TryLoadSelected();

            _btnLoadSelected.Enabled = _dataType == typeof(Member) || HasMemberNumberProperty(dataSource);

            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            buttonPanel.Controls.Add(_btnExportExcel);
            buttonPanel.Controls.Add(_btnLoadSelected);

            Button btnCopy = new() { Text = "Αντιγραφή", Dock = DockStyle.Left, Width = 100 };
            btnCopy.Click += (_, _) => CopySelectedRowToClipboard();
            buttonPanel.Controls.Add(btnCopy);

            Controls.Add(_grid);
            Controls.Add(buttonPanel);
            Controls.Add(rowCountLabel);
            if (summaryLabel != null)
            {
                Controls.Add(summaryLabel);
            }

            Controls.Add(header);
        }

        private void ExportToExcel()
        {
            try
            {
                if (_grid.DataSource is System.Collections.IEnumerable rawList)
                {
                    List<object> items = rawList.Cast<object>().ToList();
                    if (items.Count == 0)
                    {
                        MessageBox.Show("Δεν υπάρχουν δεδομένα για εξαγωγή.", "Εξαγωγή", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    Dictionary<string, Func<object, object?>> columns = new();
                    foreach (DataGridViewColumn col in _grid.Columns)
                    {
                        string propName = col.DataPropertyName;
                        string header = FieldHeaderMapper.GetHeader(propName, _dataType);
                        columns[header] = item =>
                        {
                            System.Reflection.PropertyInfo? prop = item.GetType().GetProperty(propName);
                            return prop?.GetValue(item);
                        };
                    }

                    string fileTitle = FieldHeaderMapper.GetQueryTitle(_queryKey, _queryArgs);
                    ExportHelper.ExportExcelWithNotice(items, fileTitle, columns);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Αποτυχία εξαγωγής:\n" + ex.Message, "Σφάλμα Εξαγωγής", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CopySelectedRowToClipboard()
        {
            if (_grid.CurrentRow == null)
            {
                return;
            }

            string[] values = _grid.Columns
                .Cast<DataGridViewColumn>()
                .Select(c => _grid.CurrentRow.Cells[c.Index].Value?.ToString() ?? "")
                .ToArray();

            Clipboard.SetText(string.Join("\t", values));
        }

        private void TryLoadSelected()
        {
            if (_dataType == typeof(Member) && _grid.CurrentRow?.DataBoundItem is Member member)
            {
                _mainForm.ShowMembersView();
                _mainForm.membersView.LoadMemberByNumber(member.MemberNumber);
                Close();
                return;
            }

            object? selectedItem = _grid.CurrentRow?.DataBoundItem;
            if (selectedItem != null)
            {
                System.Reflection.PropertyInfo? prop = selectedItem.GetType().GetProperty("MemberNumber");
                if (prop?.GetValue(selectedItem) is int number)
                {
                    _mainForm.ShowMembersView();
                    _mainForm.membersView.LoadMemberByNumber(number);
                    Close();
                    return;
                }
            }

            MessageBox.Show("Το αποτέλεσμα δεν γίνεται να φορτωθεί στη φόρμα μελών.", "Δεν υποστηρίζεται", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static bool HasMemberNumberProperty(object dataSource)
        {
            Type? listType = dataSource.GetType();
            Type? itemType = listType.IsGenericType
                ? listType.GetGenericArguments()[0]
                : listType.GetElementType();

            return itemType?.GetProperty("MemberNumber") != null;
        }

        public static void Show<T>(MainForm mainForm, List<T> results, string queryKey, string? summary = null, params object[] args)
        {
            if (results.Count == 0)
            {
                MessageBox.Show("Δεν υπάρχουν αποτελέσματα.", "Αποτελέσματα", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            QueryResultDialog dlg = new(mainForm, results, typeof(T), queryKey, summary, args);
            dlg.ShowDialog(mainForm);
        }
    }
}
