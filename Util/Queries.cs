using Microsoft.EntityFrameworkCore;
using SYLOGOS.Models;

namespace SYLOGOS.Util
{
    public class MemberChildSummary
    {
        public string MemberName { get; set; }
        public int ChildCount { get; set; }
    }

    public class MembershipDisplay
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public int MemberNumber { get; set; }
        public string MemberName { get; set; }
    }

    public class ChildBirthdayDisplay
    {
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int MemberNumber { get; set; }
        public string MemberName { get; set; }
    }

    public class ChildAgedDisplay
    {
        public string FullName { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public int MemberNumber { get; set; }
        public string MemberName { get; set; } = "";
    }

    public class ChildrenSummaryDisplay
    {
        public int MemberNumber { get; set; }
        public string MemberName { get; set; } = "";
        public int Children { get; set; }
    }

    public static class Queries
    {
        // ---------------------- MEMBERSHIP QUERIES ----------------------

        /// <summary>
        /// Returns members who have not paid membership for the specified year.
        /// A year is considered paid if there is a membership with ReceiptNumber and ReceiptYear == Year.
        /// </summary>
        public static List<Member> GetUnpaidMembers(AppDbContext db, int year)
        {
            return db.Members
                .Include(m => m.Memberships)
                .Where(m => !m.Memberships.Any(ms =>
                    ms.Year == year &&
                    ms.ReceiptNumber != null &&
                    ms.ReceiptYear == ms.Year))
                .OrderBy(m => m.FullName)
                .ToList();
        }

        /// <summary>
        /// Returns members who have paid for every year from registration up to current year.
        /// Paid = has receipt and receiptYear matches Year.
        /// </summary>
        public static List<Member> GetFullyPaidMembers(AppDbContext db)
        {
            int currentYear = DateTime.Now.Year;

            return db.Members
                .Include(m => m.Memberships)
                .Where(m => m.RegistrationDate.HasValue && m.Memberships.Count > 0)
                .AsEnumerable()
                .Where(m =>
                {
                    int startYear = m.RegistrationDate!.Value.Year;
                    IEnumerable<int> expectedYears = Enumerable.Range(startYear, currentYear - startYear + 1);

                    HashSet<int> paidYears = m.Memberships
                        .Where(ms => ms.ReceiptNumber != null && ms.ReceiptYear == ms.Year)
                        .Select(ms => ms.Year)
                        .ToHashSet();

                    return expectedYears.All(y => paidYears.Contains(y));
                })
                .OrderBy(m => m.FullName)
                .ToList();
        }

        /// <summary>
        /// Returns members who have at least one valid payment but missed at least one year.
        /// Valid = receipt present and receiptYear matches.
        /// </summary>
        public static List<Member> GetPartiallyPaidMembers(AppDbContext db)
        {
            int currentYear = DateTime.Now.Year;

            return db.Members
                .Include(m => m.Memberships)
                .Where(m => m.RegistrationDate.HasValue && m.Memberships.Count > 0)
                .AsEnumerable()
                .Where(m =>
                {
                    int startYear = m.RegistrationDate!.Value.Year;
                    IEnumerable<int> expectedYears = Enumerable.Range(startYear, currentYear - startYear + 1);

                    HashSet<int> paidYears = m.Memberships
                        .Where(ms => ms.ReceiptNumber != null && ms.ReceiptYear == ms.Year)
                        .Select(ms => ms.Year)
                        .ToHashSet();

                    return paidYears.Count > 0 && expectedYears.Any(y => !paidYears.Contains(y));
                })
                .OrderBy(m => m.FullName)
                .ToList();
        }

        /// <summary>
        /// Returns all membership payments for a specific year.
        /// </summary>
        public static List<MembershipDisplay> GetMembershipsByYear(AppDbContext db, int year)
        {
            return db.Memberships
                .Include(ms => ms.Member)
                .Where(ms => ms.Year == year)
                .OrderBy(ms => ms.Member.FullName)
                .Select(ms => new MembershipDisplay
                {
                    Id = ms.Id,
                    Year = ms.Year,
                    Amount = ms.Amount,
                    MemberNumber = ms.Member.MemberNumber,
                    MemberName = ms.Member.FullName
                })
                .ToList();
        }


        /// <summary>
        /// Calculates the total amount received from memberships for a given year.
        /// </summary>
        public static decimal GetTotalPaymentsByYear(AppDbContext db, int year)
        {
            return db.Memberships
                .Where(ms => ms.Year == year)
                .Sum(ms => ms.Amount);
        }

        // ---------------------- CHILDREN QUERIES ----------------------

        /// <summary>
        /// Returns children whose birthday falls on the specified date (day/month only).
        /// </summary>
        public static List<ChildBirthdayDisplay> GetChildrenWithBirthday(AppDbContext db, DateTime date)
        {
            return db.Children
                .Include(c => c.Member)
                .Where(c => c.DateOfBirth.Month == date.Month && c.DateOfBirth.Day == date.Day)
                .OrderBy(c => c.FullName)
                .Select(c => new ChildBirthdayDisplay
                {
                    FullName = c.FullName,
                    DateOfBirth = c.DateOfBirth,
                    MemberNumber = c.Member.MemberNumber,
                    MemberName = c.Member.FullName
                })
                .ToList();
        }

        /// <summary>
        /// Returns children whose ages fall within the specified min and max range.
        /// </summary>
        public static List<ChildAgedDisplay> GetChildrenAgedBetween(AppDbContext db, int minAge, int maxAge)
        {
            DateTime today = DateTime.Today;
            DateTime maxBirthDate = today.AddYears(-minAge);
            DateTime minBirthDate = today.AddYears(-maxAge - 1).AddDays(1);

            return db.Children
                .Include(c => c.Member)
                .Where(c => c.DateOfBirth >= minBirthDate && c.DateOfBirth <= maxBirthDate)
                .OrderBy(c => c.FullName)
                .Select(c => new ChildAgedDisplay
                {
                    FullName = c.FullName,
                    DateOfBirth = c.DateOfBirth,
                    MemberNumber = c.Member.MemberNumber,
                    MemberName = c.Member.FullName
                })
                .ToList();
        }

        /// <summary>
        /// Returns a summary of number of children per member.
        /// </summary>
        public static List<ChildrenSummaryDisplay> GetChildrenPerMemberSummary(AppDbContext db)
        {
            return db.Members
                .Include(m => m.Children)
                .Where(m => m.Children.Any())
                .OrderBy(m => m.FullName)
                .Select(m => new ChildrenSummaryDisplay
                {
                    MemberNumber = m.MemberNumber,
                    MemberName = m.FullName,
                    Children = m.Children.Count
                })
                .ToList();
        }

        // ---------------------- MEMBER METADATA QUERIES ----------------------

        /// <summary>
        /// Returns members registered in the last given number of months.
        /// </summary>
        public static List<Member> GetRecentlyRegisteredMembers(AppDbContext db, int months)
        {
            DateTime threshold = DateTime.Today.AddMonths(-months);
            return db.Members
                .Where(m => m.RegistrationDate.HasValue && m.RegistrationDate >= threshold)
                .OrderByDescending(m => m.RegistrationDate)
                .ToList();
        }

        /// <summary>
        /// Returns members filtered by city (case-insensitive substring match).
        /// </summary>
        public static List<Member> GetMembersByCity(AppDbContext db, string city)
        {
            return db.Members
                .AsNoTracking()
                .Where(m => !string.IsNullOrWhiteSpace(m.City))
                .ToList() // fetch all with non-null cities
                .Where(m => m.City!.Contains(city, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.FullName)
                .ToList();
        }


        /// <summary>
        /// Returns members who are missing both email or phone.
        /// </summary>
        public static List<Member> GetMembersMissingEmailOrPhone(AppDbContext db)
        {
            return db.Members
                .Where(m => string.IsNullOrWhiteSpace(m.Email) || string.IsNullOrWhiteSpace(m.MemberPhone))
                .OrderBy(m => m.FullName)
                .ToList();
        }

        /// <summary>
        /// Returns members that have a certificate number assigned.
        /// </summary>
        public static List<Member> GetMembersWithCertificate(AppDbContext db)
        {
            return db.Members
                .Where(m => !string.IsNullOrWhiteSpace(m.CertificateNumber))
                .OrderBy(m => m.FullName)
                .ToList();
        }

        /// <summary>
        /// Returns members that do not have a certificate number assigned.
        /// </summary>
        public static List<Member> GetMembersWithoutCertificate(AppDbContext db)
        {
            return db.Members
                .Where(m => string.IsNullOrWhiteSpace(m.CertificateNumber))
                .OrderBy(m => m.FullName)
                .ToList();
        }

        /// <summary>
        /// Returns members registered in the specified year.
        /// </summary>
        public static List<Member> GetMembersRegisteredInYear(AppDbContext db, int year)
        {
            return db.Members
                .Where(m => m.RegistrationDate.HasValue && m.RegistrationDate.Value.Year == year)
                .OrderBy(m => m.FullName)
                .ToList();
        }

        // ---------------------- RANDOM / LOTTERY ----------------------

        /// <summary>
        /// Randomly selects N members from the full member list.
        /// </summary>
        public static List<Member> PickRandomMembers(AppDbContext db, int count, bool excludeUnpaid = false)
        {
            int currentYear = DateTime.Now.Year;

            IQueryable<Member> query = db.Members
                .Include(m => m.Memberships);

            if (excludeUnpaid)
            {
                query = query.Where(m =>
                    m.Memberships.Any(ms =>
                        ms.Year == currentYear &&
                        ms.ReceiptNumber != null &&
                        ms.ReceiptNumber > 0 &&
                        ms.ReceiptYear == ms.Year));
            }

            List<int> ids = query.Select(m => m.Id).ToList();

            if (count >= ids.Count)
            {
                return query
                    .AsNoTracking()
                    .OrderBy(m => m.FullName)
                    .ToList();
            }

            List<int> selectedIds = ids
                .OrderBy(_ => Random.Shared.Next())
                .Take(count)
                .ToList();

            return query
                .AsNoTracking()
                .Where(m => selectedIds.Contains(m.Id))
                .ToList();
        }

        /// <summary>
        /// Randomly selects N members from those whose city matches the input (case-insensitive).
        /// </summary>
        public static List<Member> PickRandomMembersByCity(AppDbContext db, string city, int count, bool excludeUnpaid = false)
        {
            int currentYear = DateTime.Now.Year;
            string cityUpper = city.ToUpper();

            IQueryable<Member> query = db.Members
                .Include(m => m.Memberships)
                .Where(m => !string.IsNullOrWhiteSpace(m.City) &&
                            m.City.ToUpper().Contains(cityUpper));

            if (excludeUnpaid)
            {
                query = query.Where(m =>
                    m.Memberships.Any(ms =>
                        ms.Year == currentYear &&
                        ms.ReceiptNumber != null &&
                        ms.ReceiptNumber > 0 &&
                        ms.ReceiptYear == ms.Year));
            }

            List<int> ids = query.Select(m => m.Id).ToList();

            if (count >= ids.Count)
            {
                return query
                    .AsNoTracking()
                    .OrderBy(m => m.FullName)
                    .ToList();
            }

            List<int> selectedIds = ids
                .OrderBy(_ => Random.Shared.Next())
                .Take(count)
                .ToList();

            return query
                .AsNoTracking()
                .Where(m => selectedIds.Contains(m.Id))
                .ToList();
        }
    }
}
