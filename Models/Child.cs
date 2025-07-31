namespace SYLOGOS.Models
{
    public class Child
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Today;

        public int MemberId { get; set; }
        public Member Member { get; set; }
    }
}
