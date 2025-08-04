using Microsoft.EntityFrameworkCore;
using SYLOGOS.Forms;
using SYLOGOS.Models;
using System.Globalization;

namespace SYLOGOS
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("el-GR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("el-GR");
            Application.EnableVisualStyles();
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(exeDir); // force working dir

            // Ensure database and migrations
            using (AppDbContext context = new AppDbContext())
            {
                context.Database.Migrate(); // Creates DB and applies migrations

                // Ensure Settings row exists
                if (!context.Settings.Any())
                {
                    context.Settings.Add(new AppSetting
                    {
                        ClubName = "Σύλλογος Τριτέκνων Έβρου",
                        Phone = "",
                        Email = "",
                        Website = "",
                        Address = "",
                        ReceiptStartNumber = 1,
                        UseDarkMode = false,
                        ScaleMode = UiScaleMode.Normal
                    });

                    context.SaveChanges();
                }
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            using (SplashForm splash = new SplashForm())
            {
                splash.ShowDialog();
            }
            Application.Run(new MainForm());
        }
    }
}