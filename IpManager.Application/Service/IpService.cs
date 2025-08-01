using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IpManager.Application.Business;
using IpManager.Contract.Models;
using IpManager.Contract.Response;
using IpManager.Data.Models;
using IpManager.Domain.Repository;
using IpManager.Domain.Service;
using System.Diagnostics;

namespace IpManager.Service
{
    public class IpService : IIpService
    {
        private readonly ILogger<IpService> _log;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IIpRepository _repository;
        private readonly Uri ip2cAddres;
        private readonly HttpClient _client;

        public IpService(ILogger<IpService> log, IIpRepository repository, IConfiguration config, IMemoryCache cache)
        {
            _log = log;
            _config = config;
            _cache = cache;
            _repository = repository;
            ip2cAddres = new Uri(_config["IP2C_BasePath"]);
            _client = new HttpClient();
            _client.BaseAddress = ip2cAddres;
        }


        public Country GetIpCountryByIpAddress(string ip)
        {
            DateTimeOffset cacheExpiration = DateTimeOffset.Now.AddHours(1);

            var response = CacheManager.GetOrAdd(ip, () =>
            {
                var x = _repository.GetCountryByIpAddress(ip);
                if (x != null)
                {
                    var country = new Country 
                    { 
                        CountryName = x.Name, 
                        TwoLetterCode = x.TwoLetterCode, 
                        ThreeLetterCode = x.ThreeLetterCode
                    };

                    return country;
                }
                else
                {
                    HttpResponseMessage ip2c = _client.GetAsync(_client.BaseAddress + ip).Result;

                    var ip2cResponse = MapIp2cResponse(ip2c.Content.ReadAsStringAsync().Result);

                    var countryId = _repository.GetCountryId(ip2cResponse.TwoLetterCode);

                    if (countryId == 0)
                        _repository.SaveIpAndCountry(ip, ip2cResponse);
                    else
                        _repository.SaveIp(ip, countryId);

                    var country = new Country
                    {
                        CountryName = ip2cResponse.CountryName,
                        TwoLetterCode = ip2cResponse.TwoLetterCode,
                        ThreeLetterCode = ip2cResponse.ThreeLetterCode
                    };

                    return country;
                }
            }, cacheExpiration);

            return response;
        }

        public void UpdateIps()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var ips = _repository.GetListIpAddress();

            DateTimeOffset cacheExpiration = DateTimeOffset.Now.AddHours(3);

            foreach (var item in ips)
            {
                try
                {
                    _log.LogWarning("Atualizando informações do IP '{ip}' Atualizado", item.Ip);

                    HttpResponseMessage ip2cResponse = _client.GetAsync(_client.BaseAddress + item.Ip).Result;

                    var ip2cCountry = MapIp2cResponse(ip2cResponse.Content.ReadAsStringAsync().Result);

                    if (!ip2cResponse.IsSuccessStatusCode || ip2cCountry.Status == 2)
                    {
                        _log.LogError("Falha ao chamar o serviço IP2C para o IP '{Ip}'. Status: '{StatusCode}'",item.Ip, ip2cResponse.StatusCode);
                        continue;
                    }

                    var ipCacheKey = item.Ip + ip2cCountry.TwoLetterCode;
                    var x = CacheManager.GetOrAdd(ipCacheKey, () =>
                    {
                        var countryId = _repository.GetCountryId(ip2cCountry.TwoLetterCode);

                        if (countryId == 0)
                        {
                            var newcountryId = _repository.SaveCountry(ip2cCountry);
                            item.UpdatedAt = DateTime.Now;
                            item.CountryId = newcountryId;

                            _repository.UpdateIpAddress(item);
                        }
                        else
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.CountryId = countryId;

                            _repository.UpdateIpAddress(item);
                        }

                        _log.LogWarning("IP '{Ip}' Atualizado", item.Ip);

                        return ipCacheKey;
                    }, cacheExpiration);
                }
                catch
                {
                    _log.LogError("Falha ao atualizar o IP '{Ip}'", item.Ip);
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
