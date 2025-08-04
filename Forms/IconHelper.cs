using System.Windows.Forms;
namespace SYLOGOS.Forms
{
    public static class IconHelper
    {
        public static System.Drawing.Icon AppIcon =>
            System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
    }
}
