using System.Diagnostics;

namespace SYLOGOS.Forms
{
    public class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text = "ΠΛΗΡΟΦΟΡΙΕΣ ΕΦΑΡΜΟΓΗΣ";
            StartPosition = FormStartPosition.CenterParent;
            Width = 500;
            Height = 300;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            RichTextBox infoBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                DetectUrls = true,
                BackColor = SystemColors.Control,
                TabStop = false,
                Cursor = Cursors.Arrow,
                Font = new Font("Segoe UI", 10),
                Text = """
ΣΥΛΛΟΓΟΣ — Διαχείριση Μελλών Συλλόγου
Έκδοση 1.0.0

Ανάπτυξη από Στέφανος Καρυωτίδης

🌐 Website: https://stefk.me
📧 Email: mailto:stef.kariotidis@mgail.com (stef.kariotidis@mgail.com)
🐙 GitHub: https://github.com/stef-k/SYLOGOS

Η εφαρμογή αυτή είναι ανοιχτού κώδικα υπό την άδεια MIT.
"""
            };

            infoBox.LinkClicked += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = e.LinkText.StartsWith("mailto:") ? e.LinkText : e.LinkText,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Αποτυχία ανοίγματος συνδέσμου:\n" + ex.Message);
                }
            };

            Controls.Add(infoBox);
        }
    }
}
