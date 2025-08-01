namespace IpManager.Domain.Models.Entity
{
    public class Countries
    {
        public int Id { get; set; }
        public required string Name {  get; set; }
        public required string TwoLetterCode { get; set; }
        public required string ThreeLetterCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
