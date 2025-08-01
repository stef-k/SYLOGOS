namespace SYLOGOS.Models
{
    public class Member
    {
        public int Id { get; set; }
        public int MemberNumber { get; set; }
        public string FullName { get; set; }
        public string? SpouseFullName { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? MemberPhone { get; set; }
        public string? SpousePhone { get; set; }
        public string? Email { get; set; }

        public string? CertificateNumber { get; set; }

        public string? CertificatePublisher { get; set; }

        public string? Notes { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public List<Child> Children { get; set; } = new();
        public List<Membership> Memberships { get; set; } = new();
    }
}
