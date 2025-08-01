namespace IpManager.Domain.Models.Entity
{
    public class IPAddresses
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public required string Ip { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
