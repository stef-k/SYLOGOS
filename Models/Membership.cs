namespace SYLOGOS.Models
{
    public class Membership
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public int? ReceiptNumber { get; set; }         // null = not issued yet
        public int? ReceiptYear { get; set; }           // for display and reset logic

        public int MemberId { get; set; }
        public Member Member { get; set; }
    }
}
