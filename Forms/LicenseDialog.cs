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

Copyright (c) 2025 Stef Kariotidis

Permission is hereby granted, free of charge, to any person obtaining a copy...
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
