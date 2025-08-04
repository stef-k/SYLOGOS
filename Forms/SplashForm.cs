using SYLOGOS.Forms;
using SYLOGOS.Models;

public class SplashForm : Form
{
    private System.Windows.Forms.Timer _timer;

    public SplashForm()
    {
        this.Icon = IconHelper.AppIcon;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;

        Image? bg = LoadLogoImage();
        if (bg != null)
        {
            BackgroundImage = bg;
            Width = bg.Width;
            Height = bg.Height;
        }
        else
        {
            Width = 600;
            Height = 300;
        }

        BackgroundImageLayout = ImageLayout.Stretch;

        string labelText = GetClubNameOrFallback();

        Label title = new Label
        {
            Text = labelText,
            AutoSize = false,
            Dock = DockStyle.Bottom,
            Height = 50,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(120, 0, 0, 0), // semi-transparent black overlay
            Padding = new Padding(0, 10, 0, 10)
        };

        Controls.Add(title);

        _timer = new System.Windows.Forms.Timer { Interval = 2200 };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            Close();
        };
    }

    private string GetClubNameOrFallback()
    {
        try
        {
            using AppDbContext db = new();
            string? name = db.Settings.FirstOrDefault()?.ClubName;
            return string.IsNullOrWhiteSpace(name)
                ? "Φόρτωση εφαρμογής..."
                : $"{name}";
        }
        catch
        {
            return "Φόρτωση εφαρμογής...";
        }
    }

    private Image? LoadLogoImage()
    {
        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.jpg");
        return File.Exists(logoPath) ? Image.FromFile(logoPath) : null;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _timer.Start();
    }
}
