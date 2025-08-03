using SYLOGOS.Util;

namespace SYLOGOS.Forms
{
    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkMenuColorTable()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Color bgColor;

            if (e.Item.Selected)
            {
                bgColor = Color.FromArgb(70, 70, 75); // hover
            }
            else
            {
                bgColor = Color.FromArgb(45, 45, 48); // default
            }

            using Brush b = new SolidBrush(bgColor);
            e.Graphics.FillRectangle(b, e.Item.ContentRectangle);
        }
    }
}
