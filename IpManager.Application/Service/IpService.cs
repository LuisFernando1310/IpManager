using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IpManager.Contract.Models;
using IpManager.Contract.Response;
using IpManager.Data.Models;
using IpManager.Domain.Repository;
using IpManager.Domain.Service;

namespace IpManager.Service
{
    public class IpService : IIpService
    {
        private readonly ILogger<IpService> _log;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IIpRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;

        public IpService(
            ILogger<IpService> log, 
            IIpRepository repository, 
            IConfiguration config, 
            IMemoryCache cache, 
            IHttpClientFactory httpClientFactory)
        {
            _log = log;
            _config = config;
            _cache = cache;
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Country> GetIpCountryByIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("IP address is required.", nameof(ip));

            DateTimeOffset cacheExpiration = DateTimeOffset.Now.AddHours(1);

            // Example: Use IMemoryCache directly for async
            if (!_cache.TryGetValue(ip, out Country country))
            {
                var x = _repository.GetCountryByIpAddress(ip);
                if (x != null)
                {
                    country = new Country
                    {
                        CountryName = x.Name,
                        TwoLetterCode = x.TwoLetterCode,
                        ThreeLetterCode = x.ThreeLetterCode
                    };
                }
                else
                {
                    var client = _httpClientFactory.CreateClient();
                    client.BaseAddress = new Uri(_config["IP2C_BasePath"]);
                    var ip2c = await client.GetAsync(ip);
                    var ip2cResponse = MapIp2cResponse(await ip2c.Content.ReadAsStringAsync());

                    var countryId = _repository.GetCountryId(ip2cResponse.TwoLetterCode);

                    if (countryId == 0)
                        _repository.SaveIpAndCountry(ip, ip2cResponse);
                    else
                        _repository.SaveIp(ip, countryId);

                    country = new Country
                    {
                        CountryName = ip2cResponse.CountryName,
                        TwoLetterCode = ip2cResponse.TwoLetterCode,
                        ThreeLetterCode = ip2cResponse.ThreeLetterCode
                    };
                }
                _cache.Set(ip, country, cacheExpiration);
            }

            return country;
        }

        public async Task UpdateIps()
        {
            var ips = _repository.GetListIpAddress();
            DateTimeOffset cacheExpiration = DateTimeOffset.Now.AddHours(3);

            foreach (var item in ips)
            {
                try
                {
                    _log.LogWarning("Updating IP information '{ip}'", item.Ip);

                    var client = _httpClientFactory.CreateClient();
                    client.BaseAddress = new Uri(_config["IP2C_BasePath"]);
                    var ip2cResponse = await client.GetAsync(item.Ip);

                    var ip2cCountry = MapIp2cResponse(await ip2cResponse.Content.ReadAsStringAsync());

                    if (!ip2cResponse.IsSuccessStatusCode || ip2cCountry.Status == 2)
                    {
                        _log.LogError("Failed to call IP2C service for IP '{Ip}'. Status: '{StatusCode}'", item.Ip, ip2cResponse.StatusCode);
                        continue;
                    }

                    var ipCacheKey = item.Ip + ip2cCountry.TwoLetterCode;
                    // ... (rest of your cache logic)
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Failed to update IP '{Ip}'", item.Ip);
                    continue;
                }
            }
        }

        public async Task<List<CountryReport>> GetReport(List<string> countryCodes)
        {
            var response = await _repository.GetCountryReports(countryCodes);
            return response.ToList();
        }

        private Ip2cCountryResponse MapIp2cResponse(string result)
        {
            var partResults = result.Split(';');
            Ip2cCountryResponse response = new Ip2cCountryResponse
            {
                TwoLetterCode = partResults[1],
                ThreeLetterCode = partResults[2],
                CountryName = partResults[3],
            };

            return response;
        }
    }
}
