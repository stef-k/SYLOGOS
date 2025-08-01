using SYLOGOS.Models;

public class CustomLinkCell : DataGridViewTextBoxCell
{
    protected override void Paint(
        Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
        DataGridViewElementStates cellState, object? value, object? formattedValue,
        string? errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
        DataGridViewPaintParts paintParts)
    {
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState,
            value, formattedValue, errorText, cellStyle, advancedBorderStyle,
            paintParts & ~DataGridViewPaintParts.ContentForeground);

        string text = formattedValue?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        bool selected = (cellState & DataGridViewElementStates.Selected) != 0;
        bool dark;
        using (AppDbContext db = new()) { dark = db.Settings.FirstOrDefault()?.UseDarkMode ?? false; }

        Color color = selected ? Color.White : (dark ? Color.DeepSkyBlue : Color.Blue);

        // Measure without padding for pixel accuracy
        Size textSize = TextRenderer.MeasureText(graphics, text, cellStyle.Font, new Size(int.MaxValue, int.MaxValue),
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

        // Text position
        int textX = cellBounds.X + 6;
        int textY = cellBounds.Y + (cellBounds.Height - textSize.Height) / 2;

        Rectangle textRect = new Rectangle(textX, textY, textSize.Width, textSize.Height);

        // Draw text
        TextRenderer.DrawText(
            graphics,
            text,
            cellStyle.Font,
            textRect,
            color,
            TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine
        );

        // Underline (aligned to text bounds)
        int underlineY = textRect.Bottom - 2;
        int underlineX1 = textX;
        int underlineX2 = textX + textSize.Width - 1;

        using Pen underlinePen = new(color, 1f);
        graphics.DrawLine(underlinePen, underlineX1, underlineY, underlineX2, underlineY);
    }

    public override Type EditType => null; // make non-editable
}
