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
            // Ensure database and migrations
            using (AppDbContext context = new AppDbContext())
            {
                context.Database.Migrate(); // Creates DB and applies migrations
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}