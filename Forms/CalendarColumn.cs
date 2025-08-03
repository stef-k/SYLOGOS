using System.ComponentModel;
using System.Globalization;

namespace SYLOGOS.Forms
{

    public class CalendarColumn : DataGridViewColumn
    {
        public CalendarColumn() : base(new CalendarCell()) { }

        public override DataGridViewCell CellTemplate
        {
            get => base.CellTemplate;
            set
            {
                if (value is not CalendarCell)
                {
                    throw new InvalidCastException("Πρέπει να είναι κελί ημερολογίου");
                }

                base.CellTemplate = value;
            }
        }
    }

    public class CalendarCell : DataGridViewTextBoxCell
    {
        public CalendarCell() : base()
        {
            this.Style.Format = "dd/MM/yyyy";
        }

        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue,
            DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            CalendarEditingControl ctl = DataGridView.EditingControl as CalendarEditingControl;
            DateTime value = this.Value is DateTime dt && dt.Year >= 1900 ? dt : DateTime.Today;
            ctl.Value = value;
        }

        public override Type EditType => typeof(CalendarEditingControl);
        public override Type ValueType => typeof(DateTime);
        public override object DefaultNewRowValue => DateTime.Today;
    }

    class CalendarEditingControl : DateTimePicker, IDataGridViewEditingControl
    {
        public CalendarEditingControl()
        {
            this.Format = DateTimePickerFormat.Custom;
            this.CustomFormat = "dd/MM/yyyy";
            this.RightToLeft = RightToLeft.No;

            // Force Greek month/day names using thread culture
            Thread.CurrentThread.CurrentCulture = new CultureInfo("el-GR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("el-GR");
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object EditingControlFormattedValue
        {
            get => this.Value.ToShortDateString();
            set
            {
                if (DateTime.TryParse(value?.ToString(), out DateTime result))
                {
                    this.Value = result;
                }
            }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return EditingControlFormattedValue;
        }

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.Font = dataGridViewCellStyle.Font;
            this.CalendarForeColor = dataGridViewCellStyle.ForeColor;
            this.CalendarMonthBackground = dataGridViewCellStyle.BackColor;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int EditingControlRowIndex { get; set; }

        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            return keyData == Keys.Left || keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Right
                    || keyData == Keys.Home || keyData == Keys.End || keyData == Keys.PageDown || keyData == Keys.PageUp;
        }

        public void PrepareEditingControlForEdit(bool selectAll) { }

        public bool RepositionEditingControlOnValueChange => false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataGridView EditingControlDataGridView { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EditingControlValueChanged { get; set; } = false;

        public Cursor EditingPanelCursor => base.Cursor;

        protected override void OnValueChanged(EventArgs eventargs)
        {
            EditingControlValueChanged = true;
            EditingControlDataGridView.NotifyCurrentCellDirty(true);
            base.OnValueChanged(eventargs);
        }
    }

}
