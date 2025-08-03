using SYLOGOS.Forms;
using SYLOGOS.Models;
using SYLOGOS.Util;

namespace SYLOGOS
{
    public static class QueryMenuBuilder
    {
        public static ToolStripMenuItem Build(MainForm mainForm)
        {
            ToolStripMenuItem queriesMenu = new("Queries");
            AddQueryMenuItems(mainForm, queriesMenu);
            return queriesMenu;
        }


        public static void AddQueryMenuItems(MainForm mainForm, ToolStripMenuItem queriesMenu)
        {
            queriesMenu.DropDownItems.Add(BuildQueryItem("Unpaid Members", () =>
            {
                using AppDbContext db = new();
                int year = DateTime.Now.Year;
                List<Member> results = Queries.GetUnpaidMembers(db, year);
                QueryResultDialog.Show(mainForm, results, "UnpaidMembers", null, year);
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Fully Paid Members", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetFullyPaidMembers(db);
                QueryResultDialog.Show(mainForm, results, "FullyPaidMembers");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Partially Paid Members", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetPartiallyPaidMembers(db);
                QueryResultDialog.Show(mainForm, results, "PartiallyPaidMembers");
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
                    QueryResultDialog.Show(mainForm, results, "MembershipsByYear", $"Total: {total:C2}", year);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Recently Registered (months)...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Months", new[] { "Months" }, new[] { InputType.Numeric });
                if (input != null && input["Months"] is decimal m)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.GetRecentlyRegisteredMembers(db, (int)m);
                    QueryResultDialog.Show(mainForm, results, "RecentlyRegisteredMembers", null, (int)m);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members by City...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Enter City", new[] { "City" }, new[] { InputType.Text });
                if (input != null && input["City"] is string c)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.GetMembersByCity(db, c);
                    QueryResultDialog.Show(mainForm, results, "MembersByCity", null, c);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members Missing Email or Phone", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetMembersMissingEmailOrPhone(db);
                QueryResultDialog.Show(mainForm, results, "MembersMissingEmailOrPhone");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members With Certificate", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetMembersWithCertificate(db);
                QueryResultDialog.Show(mainForm, results, "MembersWithCertificate");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members Without Certificate", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetMembersWithoutCertificate(db);
                QueryResultDialog.Show(mainForm, results, "MembersWithoutCertificate");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Members Registered in Year...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Enter Year", new[] { "Year" }, new[] { InputType.Numeric });
                if (input != null && input["Year"] is decimal y)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.GetMembersRegisteredInYear(db, (int)y);
                    QueryResultDialog.Show(mainForm, results, "MembersRegisteredInYear", null, (int)y);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Children with Birthday on...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Birthday Date", new[] { "Date" }, new[] { InputType.Date });
                if (input != null && input["Date"] is DateTime d)
                {
                    using AppDbContext db = new();
                    List<ChildBirthdayDisplay> results = Queries.GetChildrenWithBirthday(db, d);
                    QueryResultDialog.Show(mainForm, results, "ChildrenWithBirthday", null, d);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Children Aged Between...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Enter Age Range", new[] { "Min Age", "Max Age" }, new[] { InputType.Numeric, InputType.Numeric });
                if (input != null && input["Min Age"] is decimal min && input["Max Age"] is decimal max)
                {
                    using AppDbContext db = new();
                    List<ChildAgedDisplay> results = Queries.GetChildrenAgedBetween(db, (int)min, (int)max);
                    QueryResultDialog.Show(mainForm, results, "ChildrenAgedBetween", null, (int)min, (int)max);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Children per Member Summary", () =>
            {
                using AppDbContext db = new();
                List<ChildrenSummaryDisplay> results = Queries.GetChildrenPerMemberSummary(db);
                QueryResultDialog.Show(mainForm, results, "ChildrenPerMemberSummary");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Randomly Pick N Members...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show(
                    "Pick Random",
                    new[] { "Count", "Title", "Exclude Unpaid?" },
                    new[] { InputType.Numeric, InputType.Text, InputType.Checkbox });

                if (input != null &&
                    input["Count"] is decimal n &&
                    input["Exclude Unpaid?"] is bool excludeUnpaid)
                {
                    string rawTitle = input.TryGetValue("Title", out object? t) ? t?.ToString() ?? "" : "";
                    string title = string.IsNullOrWhiteSpace(rawTitle) ? "ΤΥΧΑΙΑ ΕΠΙΛΟΓΗ" : rawTitle.Trim();

                    using AppDbContext db = new();
                    List<Member> results = Queries.PickRandomMembers(db, (int)n, excludeUnpaid);
                    QueryResultDialog.Show(mainForm, results, "PickRandomMembers", null, title, (int)n);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Randomly Pick N Members by City...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show(
                    "Pick by City",
                    new[] { "City", "Count", "Title", "Exclude Unpaid?" },
                    new[] { InputType.Text, InputType.Numeric, InputType.Text, InputType.Checkbox });

                if (input != null &&
                    input["City"] is string city &&
                    input["Count"] is decimal n &&
                    input["Exclude Unpaid?"] is bool excludeUnpaid)
                {
                    string rawTitle = input.TryGetValue("Title", out object? t) ? t?.ToString() ?? "" : "";
                    string title = string.IsNullOrWhiteSpace(rawTitle) ? "ΤΥΧΑΙΑ ΕΠΙΛΟΓΗ" : rawTitle.Trim();

                    using AppDbContext db = new();
                    List<Member> results = Queries.PickRandomMembersByCity(db, city, (int)n, excludeUnpaid);
                    QueryResultDialog.Show(mainForm, results, "PickRandomMembersByCity", null, title, (int)n, city);
                }
            }));
        }

        private static ToolStripMenuItem BuildQueryItem(string label, Action action)
        {
            ToolStripMenuItem item = new(label);
            item.Click += (_, _) => action();
            return item;
        }
    }
}
