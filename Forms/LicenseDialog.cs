namespace SYLOGOS.Forms
{
    public class LicenseDialog : Form
    {
        public LicenseDialog()
        {
            Text = "License - MIT";
            StartPosition = FormStartPosition.CenterParent;
            Width = 600;
            Height = 400;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            string licenseText = File.Exists("LICENSE.txt")
                ? File.ReadAllText("LICENSE.txt")
                : """
MIT License

Copyright (c) 2025 Stef Karyotidis https://stekf.me - https://github.com/stef-k/SYLOGOS

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
""";

            RichTextBox licenseBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                BackColor = SystemColors.Control,
                TabStop = false,
                Cursor = Cursors.Arrow,
                Font = new Font("Consolas", 9),
                Text = licenseText
            };

            Controls.Add(licenseBox);
        }
    }
}
