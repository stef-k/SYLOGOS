namespace SYLOGOS.Models
{
    public class Membership
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }
    }
}
