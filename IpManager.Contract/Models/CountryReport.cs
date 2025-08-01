namespace IpManager.Data.Models
{
    public class CountryReport
    {
        public required string CountryName { get; set; }
        public int AddressesCount { get; set; }
        public DateTime LastAddressUpdated { get; set; }
    }
}
