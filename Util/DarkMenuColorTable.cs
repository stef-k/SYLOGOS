namespace SYLOGOS.Util
{
    public class DarkMenuColorTable : ProfessionalColorTable
    {
        public override Color MenuBorder => Color.Gray;
        public override Color MenuItemBorder => Color.DimGray;
        public override Color MenuItemSelected => Color.FromArgb(70, 70, 75);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(70, 70, 75);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(70, 70, 75);
        public override Color MenuStripGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color MenuStripGradientEnd => Color.FromArgb(45, 45, 48);
        public override Color ToolStripDropDownBackground => Color.FromArgb(50, 50, 55);

        public override Color ImageMarginGradientBegin => Color.FromArgb(50, 50, 55);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(50, 50, 55);
        public override Color ImageMarginGradientEnd => Color.FromArgb(50, 50, 55);

        public override Color SeparatorDark => Color.Gray;
        public override Color SeparatorLight => Color.Gray;

        public override Color ToolStripGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color ToolStripGradientMiddle => Color.FromArgb(45, 45, 48);
        public override Color ToolStripGradientEnd => Color.FromArgb(45, 45, 48);
    }

}
