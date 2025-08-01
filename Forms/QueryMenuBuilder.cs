using SYLOGOS.Models;
using SYLOGOS.Util;

namespace SYLOGOS.Forms
{
    public static class QueryMenuBuilder
    {
        public static ToolStripMenuItem Build(MainForm mainForm)
        {
            ToolStripMenuItem queriesMenu = new ToolStripMenuItem("Queries");

            // --- Membership Queries ---
            queriesMenu.DropDownItems.Add(BuildQueryItem("Unpaid Members", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetUnpaidMembers(db, DateTime.Now.Year);
                QueryResultDialog.Show(mainForm, results, $"Unpaid Members ({DateTime.Now.Year})");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Fully Paid Members", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetFullyPaidMembers(db);
                QueryResultDialog.Show(mainForm, results, "Fully Paid Members");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Partially Paid Members", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetPartiallyPaidMembers(db);
                QueryResultDialog.Show(mainForm, results, "Partially Paid Members");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Memberships by Year...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Enter Year", new[] { "Year" }, new[] { InputType.Numeric });
                if (input != null && input["Year"] is decimal y)
                {
                    int year = (int)y;
                    using AppDbContext db = new();
                    List<MembershipDisplay> results = Queries.GetMembershipsByYear(db, year);
                    decimal total = results.Sum(r => r.Amount);
                    QueryResultDialog.Show(mainForm, results, $"Memberships ({year})", $"Total: {total:C2}");
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Total Payments by Year...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Enter Year", new[] { "Year" }, new[] { InputType.Numeric });
                if (input != null && input["Year"] is decimal y)
                {
                    using AppDbContext db = new();
                    decimal total = Queries.GetTotalPaymentsByYear(db, (int)y);
                    MessageBox.Show($"Total payments in {(int)y}: {total:C2}");
                }
            }));

            queriesMenu.DropDownItems.Add(new ToolStripSeparator());

            // --- Member Queries ---
            queriesMenu.DropDownItems.Add(BuildQueryItem("Recently Registered (months)...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Months", new[] { "Months" }, new[] { InputType.Numeric });
                if (input != null && input["Months"] is decimal m)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.GetRecentlyRegisteredMembers(db, (int)m);
                    QueryResultDialog.Show(mainForm, results, $"Registered Last {(int)m} Months");
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members by City...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Enter City", new[] { "City" }, new[] { InputType.Text });
                if (input != null && input["City"] is string c)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.GetMembersByCity(db, c);
                    QueryResultDialog.Show(mainForm, results, $"City: {c}");
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members Missing Email or Phone", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetMembersMissingEmailOrPhone(db);
                QueryResultDialog.Show(mainForm, results, "Missing Contact Info");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members With Certificate", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetMembersWithCertificate(db);
                QueryResultDialog.Show(mainForm, results, "With Certificate");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members Without Certificate", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetMembersWithoutCertificate(db);
                QueryResultDialog.Show(mainForm, results, "Without Certificate");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members Registered in Year...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Enter Year", new[] { "Year" }, new[] { InputType.Numeric });
                if (input != null && input["Year"] is decimal y)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.GetMembersRegisteredInYear(db, (int)y);
                    QueryResultDialog.Show(mainForm, results, $"Registered in {(int)y}");
                }
            }));

            queriesMenu.DropDownItems.Add(new ToolStripSeparator());

            // --- Children Queries ---
            queriesMenu.DropDownItems.Add(BuildQueryItem("Children with Birthday on...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Birthday Date", new[] { "Date" }, new[] { InputType.Date });
                if (input != null && input["Date"] is DateTime d)
                {
                    using AppDbContext db = new();
                    List<ChildBirthdayDisplay> results = Queries.GetChildrenWithBirthday(db, d);
                    QueryResultDialog.Show(mainForm, results, $"Children Born on {d:MMMM d}");
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Children Aged Between...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Enter Age Range", new[] { "Min Age", "Max Age" }, new[] { InputType.Numeric, InputType.Numeric });
                if (input != null && input["Min Age"] is decimal min && input["Max Age"] is decimal max)
                {
                    using AppDbContext db = new();
                    List<ChildAgedDisplay> results = Queries.GetChildrenAgedBetween(db, (int)min, (int)max);
                    QueryResultDialog.Show(mainForm, results, $"Children Aged {(int)min}–{(int)max}");
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Children per Member Summary", () =>
            {
                using AppDbContext db = new();
                List<ChildrenSummaryDisplay> results = Queries.GetChildrenPerMemberSummary(db);
                QueryResultDialog.Show(mainForm, results, "Children per Member");
            }));

            queriesMenu.DropDownItems.Add(new ToolStripSeparator());

            // --- Random Pick Queries ---
            queriesMenu.DropDownItems.Add(BuildQueryItem("Randomly Pick N Members...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Pick Random", new[] { "Count", "Title" }, new[] { InputType.Numeric, InputType.Text });
                if (input != null && input["Count"] is decimal n && input["Title"] is string title)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.PickRandomMembers(db, (int)n);
                    QueryResultDialog.Show(mainForm, results, title);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Randomly Pick N Members by City...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Pick by City", new[] { "City", "Count", "Title" }, new[] { InputType.Text, InputType.Numeric, InputType.Text });
                if (input != null && input["City"] is string city && input["Count"] is decimal n && input["Title"] is string title)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.PickRandomMembersByCity(db, city, (int)n);
                    QueryResultDialog.Show(mainForm, results, title);
                }
            }));

            return queriesMenu;
        }

        private static ToolStripMenuItem BuildQueryItem(string label, Action onClick)
        {
            ToolStripMenuItem item = new(label);
            item.Click += (_, _) => onClick();
            return item;
        }
    }
}
