namespace SYLOGOS.Models
{
    public enum UiScaleMode
    {
        Small,
        Normal,
        Large
    }

    public class AppSetting
    {
        public int Id { get; set; }
        public string? ClubName { get; set; }
        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Website { get; set; }

        public string? Address { get; set; }

        public int ReceiptStartNumber { get; set; } = 1; // Start number for receipt numbering for each year

        // Binary column to store image
        public byte[]? ClubLogo { get; set; }

        public bool UseDarkMode { get; set; } = false;

        public UiScaleMode ScaleMode { get; set; } = UiScaleMode.Normal;

    }
}
