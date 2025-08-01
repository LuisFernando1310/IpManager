using IpManager.Contract.Response;
using IpManager.Data.Models;
using IpManager.Domain.Models.Entity;

namespace IpManager.Domain.Repository
{
    public interface IIpRepository
    {
        Countries GetCountryByIpAddress(string ipAddress);
        int GetCountryId(string twoLettlerCountryCode);
        void SaveIpAndCountry(string ipAddres, Ip2cCountryResponse ipCountry);
        void SaveIp(string ipAddres, int countryId);
        int SaveCountry(Ip2cCountryResponse country);
        List<IPAddresses> GetListIpAddress();
        void UpdateCountry(Countries country);
        void UpdateIpAddress(IPAddresses IpAddress);
        Task<IEnumerable<CountryReport>> GetCountryReports(List<string> countryCodes);

    }
}
