using System.Diagnostics;

namespace SYLOGOS.Forms
{
    public class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text = "About SYLOGOS";
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
SYLOGOS — Family Club Membership Manager
Version 1.0.0

Developed by Stef Kariotidis

🌐 Website: https://example.com
📧 Email: mailto:stef@example.com (stef@example.com)
🐙 GitHub: https://github.com/stefkariotidis/sylogos

This application is open source under the MIT license.
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
                    MessageBox.Show("Failed to open link:\n" + ex.Message);
                }
            };

            Controls.Add(infoBox);
        }
    }
}
