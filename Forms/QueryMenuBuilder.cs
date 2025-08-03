using SYLOGOS.Forms;
using SYLOGOS.Models;
using SYLOGOS.Util;

namespace SYLOGOS
{
    public static class QueryMenuBuilder
    {
        public static ToolStripMenuItem Build(MainForm mainForm)
        {
            ToolStripMenuItem queriesMenu = new("ΕΡΩΤΗΜΑΤΑ");
            AddQueryMenuItems(mainForm, queriesMenu);
            return queriesMenu;
        }


        public static void AddQueryMenuItems(MainForm mainForm, ToolStripMenuItem queriesMenu)
        {
            queriesMenu.DropDownItems.Add(BuildQueryItem("Απλήρωτες Συνδρομές", () =>
            {
                using AppDbContext db = new();
                int year = DateTime.Now.Year;
                List<Member> results = Queries.GetUnpaidMembers(db, year);
                QueryResultDialog.Show(mainForm, results, "UnpaidMembers", null, year);
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Πληρωμένες Συνδρομές", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetFullyPaidMembers(db);
                QueryResultDialog.Show(mainForm, results, "FullyPaidMembers");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Μερικώς Πληρωμένες Συνδρομές", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetPartiallyPaidMembers(db);
                QueryResultDialog.Show(mainForm, results, "PartiallyPaidMembers");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Συνδρομές ανα Έτος...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Εισάγετε Έτος", new[] { "Έτος" }, new[] { InputType.Numeric });
                if (input != null && input["Έτος"] is decimal y)
                {
                    int year = (int)y;
                    using AppDbContext db = new();
                    List<MembershipDisplay> results = Queries.GetMembershipsByYear(db, year);
                    decimal total = results.Sum(r => r.Amount);
                    QueryResultDialog.Show(mainForm, results, "MembershipsByYear", $"Σύνολο: {total:C2}", year);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Πρόσφατα Εγγεγραμμένα Μέλη (μήνες)...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Μήνες", new[] { "Μήνες" }, new[] { InputType.Numeric });
                if (input != null && input["Μήνες"] is decimal m)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.GetRecentlyRegisteredMembers(db, (int)m);
                    QueryResultDialog.Show(mainForm, results, "RecentlyRegisteredMembers", null, (int)m);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Μέλη ανα Πόλη...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Εισαγωγή Πόλης", new[] { "Πόλη" }, new[] { InputType.Text });
                if (input != null && input["Πόλη"] is string c)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.GetMembersByCity(db, c);
                    QueryResultDialog.Show(mainForm, results, "MembersByCity", null, c);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Μέλη Χωρίς Τηλέφωνο ή Email", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetMembersMissingEmailOrPhone(db);
                QueryResultDialog.Show(mainForm, results, "MembersMissingEmailOrPhone");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Μέλη με Πιστοποιητικό Οικ. Κατάστασης", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetMembersWithCertificate(db);
                QueryResultDialog.Show(mainForm, results, "MembersWithCertificate");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Μέλη Χωρίς Πιστοποιητικό Οικ. Κατάστασης", () =>
            {
                using AppDbContext db = new();
                List<Member> results = Queries.GetMembersWithoutCertificate(db);
                QueryResultDialog.Show(mainForm, results, "MembersWithoutCertificate");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Μέλη Εγγεγραμμένα το Έτος...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Εισαγωγή έτους", new[] { "Έτος" }, new[] { InputType.Numeric });
                if (input != null && input["Έτος"] is decimal y)
                {
                    using AppDbContext db = new();
                    List<Member> results = Queries.GetMembersRegisteredInYear(db, (int)y);
                    QueryResultDialog.Show(mainForm, results, "MembersRegisteredInYear", null, (int)y);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Τέκνα με Γεννέθλια στις...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Είσάγετε Ημερομηνία", new[] { "Ημερομηνία" }, new[] { InputType.Date });
                if (input != null && input["Ημερομηνία"] is DateTime d)
                {
                    using AppDbContext db = new();
                    List<ChildBirthdayDisplay> results = Queries.GetChildrenWithBirthday(db, d);
                    QueryResultDialog.Show(mainForm, results, "ChildrenWithBirthday", null, d);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Τέκνα Γεννημένα Μεταξύ...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show("Εισάγετε Εύρος Ηλικίας", new[] { "Κατώτατη Ηλικία", "Ανώτατη Ηλικία" }, new[] { InputType.Numeric, InputType.Numeric });
                if (input != null && input["Κατώτατη Ηλικία"] is decimal min && input["Ανώτατη Ηλικία"] is decimal max)
                {
                    using AppDbContext db = new();
                    List<ChildAgedDisplay> results = Queries.GetChildrenAgedBetween(db, (int)min, (int)max);
                    QueryResultDialog.Show(mainForm, results, "ChildrenAgedBetween", null, (int)min, (int)max);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Τέκνα ανα Μέλος", () =>
            {
                using AppDbContext db = new();
                List<ChildrenSummaryDisplay> results = Queries.GetChildrenPerMemberSummary(db);
                QueryResultDialog.Show(mainForm, results, "ChildrenPerMemberSummary");
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Τυχαία Επιλογή Αριθμού Μελών...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show(
                    "Επιλέξτε Αριθμό Μελών",
                    new[] { "Αριθμός", "Τίτλος Λοταρίας", "Εξαίρεση Μελών με Απλήρωτη Τρέχουσα Συνδρομή;" },
                    new[] { InputType.Numeric, InputType.Text, InputType.Checkbox });

                if (input != null &&
                    input["Αριθμός"] is decimal n &&
                    input["Εξαίρεση Μελών με Απλήρωτη Τρέχουσα Συνδρομή;"] is bool excludeUnpaid)
                {
                    string rawTitle = input.TryGetValue("Τίτλος Λοταρίας", out object? t) ? t?.ToString() ?? "" : "";
                    string title = string.IsNullOrWhiteSpace(rawTitle) ? "ΤΥΧΑΙΑ ΕΠΙΛΟΓΗ" : rawTitle.Trim();

                    using AppDbContext db = new();
                    List<Member> results = Queries.PickRandomMembers(db, (int)n, excludeUnpaid);
                    QueryResultDialog.Show(mainForm, results, "PickRandomMembers", null, title, (int)n);
                }
            }));

            queriesMenu.DropDownItems.Add(BuildQueryItem("Τυχαία Επιλογή Αριθμού Μελών ανα Πόλη...", () =>
            {
                Dictionary<string, object?>? input = InputPromptDialog.Show(
                    "Επιλογή Πόλης",
                    new[] { "Πόλη", "Αριθμός", "Τίτλος Λοταρίας", "Εξαίρεση Μελών με Απλήρωτη Τρέχουσα Συνδρομή;" },
                    new[] { InputType.Text, InputType.Numeric, InputType.Text, InputType.Checkbox });

                if (input != null &&
                    input["Πόλη"] is string city &&
                    input["Αριθμός"] is decimal n &&
                    input["Εξαίρεση Μελών με Απλήρωτη Τρέχουσα Συνδρομή;"] is bool excludeUnpaid)
                {
                    string rawTitle = input.TryGetValue("Τίτλος Λοταρίας", out object? t) ? t?.ToString() ?? "" : "";
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
