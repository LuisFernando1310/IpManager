using IpManager.Contract.Models;
using IpManager.Data.Models;

namespace IpManager.Domain.Service
{
    public interface IIpService
    {
        Country GetIpCountryByIpAddress(string ip);
        Task<List<CountryReport>> GetReport(List<string> countryCodes);
        void UpdateIps();
    }
}
