using SYLOGOS.Models;

namespace SYLOGOS.Util;
public static class ExportHelper
{
    private static readonly string[] Units = { "", "ένα", "δύο", "τρία", "τέσσερα", "πέντε", "έξι", "επτά", "οκτώ", "εννέα" };
    private static readonly string[] Teens = { "δέκα", "έντεκα", "δώδεκα", "δεκατρία", "δεκατέσσερα", "δεκαπέντε", "δεκαέξι", "δεκαεπτά", "δεκαοκτώ", "δεκαεννέα" };
    private static readonly string[] Tens = { "", "", "είκοσι", "τριάντα", "σαράντα", "πενήντα", "εξήντα", "εβδομήντα", "ογδόντα", "ενενήντα" };

    public static string ToGreekAmountText(decimal amount)
    {
        int euros = (int)Math.Floor(amount);
        int cents = (int)((amount - euros) * 100);

        string result = "";

        if (euros > 0)
        {
            result += ConvertUnder1000(euros) + " ευρώ";
        }

        if (cents > 0)
        {
            if (euros > 0)
            {
                result += " και ";
            }

            result += ConvertUnder1000(cents) + " λεπτά";
        }

        if (euros == 0 && cents == 0)
        {
            result = "μηδέν ευρώ";
        }

        return result.Trim();
    }

    private static string ConvertUnder1000(int n)
    {
        if (n < 10)
        {
            return Units[n];
        }

        if (n < 20)
        {
            return Teens[n - 10];
        }

        if (n < 100)
        {
            int t = n / 10;
            int u = n % 10;
            return Tens[t] + (u > 0 ? $" {Units[u]}" : "");
        }
        if (n < 1000)
        {
            int h = n / 100;
            int rem = n % 100;
            string hundreds = h switch
            {
                1 => "εκατό",
                2 => "διακόσια",
                3 => "τριακόσια",
                4 => "τετρακόσια",
                5 => "πεντακόσια",
                6 => "εξακόσια",
                7 => "επτακόσια",
                8 => "οκτακόσια",
                9 => "εννιακόσια",
                _ => ""
            };
            return hundreds + (rem > 0 ? $" {ConvertUnder1000(rem)}" : "");
        }

        return n.ToString(); // fallback
    }

    public static string GetDesktopPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    }

    public static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name.Trim();
    }

    public static void ExportExcelWithNotice<T>(IEnumerable<T> data, string title, Dictionary<string, Func<T, object?>> columns)
    {
        string path = Path.Combine(GetDesktopPath(), SanitizeFileName(title) + ".xlsx");
        ExportService.ExportToExcel(data, title, path, columns);
        System.Windows.Forms.MessageBox.Show($"Excel exported to:\n{path}", "Export Complete");
    }

    public static void ExportPdfWithNotice<T>(IEnumerable<T> data, string title, Dictionary<string, Func<T, object?>> columns)
    {
        // PDF table logic removed — we use dedicated documents now
        System.Windows.Forms.MessageBox.Show("Use a dedicated document class for PDF export.", "Not Implemented");
    }

    public static void ExportMembershipReceiptWithNotice(Member member, Membership membership)
    {
        string fileName = $"Receipt_{SanitizeFileName(member.FullName)}_{membership.Year}.pdf";
        string path = Path.Combine(GetDesktopPath(), fileName);
        ExportService.ExportMembershipReceipt(member, membership, path);
        System.Windows.Forms.MessageBox.Show($"Receipt exported to:\n{path}", "Export Complete");
    }
}
