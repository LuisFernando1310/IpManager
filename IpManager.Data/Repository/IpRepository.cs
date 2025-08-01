using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using IpManager.Contract.Response;
using IpManager.Data.Models;
using IpManager.Domain.Models.Data;
using IpManager.Domain.Models.Entity;
using IpManager.Domain.Repository;

namespace IpManager.Data.Repository
{
    public class IpRepository : IIpRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public IpRepository(ApplicationDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;

            _connectionString = _config["ConnectionStrings:Conn"];
        }
        public int GetCountryId(string twoLettlerCountryCode)
        {
            try
            {
                var response = _dbContext.Countries.Where(x => x.TwoLetterCode == twoLettlerCountryCode);

                return response.Any() ? response.FirstOrDefault().Id : 0;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<IPAddresses> GetListIpAddress()
        {
            try
            {
                return _dbContext.IpAddresses.Take(100).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Countries GetCountryByIpAddress(string ipAddress)
        {
            try
            {
                var response = (from ip in _dbContext.IpAddresses
                                join country in _dbContext.Countries on ip.CountryId equals country.Id
                                where ip.Ip == ipAddress
                                select country);

                return response.FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateCountry(Countries country)
        {
            try
            {
                _dbContext.Countries.Update(country);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateIpAddress(IPAddresses IpAddress)
        {
            try
            {
                _dbContext.IpAddresses.Update(IpAddress);
                _dbContext.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveIpAndCountry(string ipAddres, Ip2cCountryResponse ipCountry)
        {
            try
            {
                var newCountry = new Countries
                {
                    Name = ipCountry.CountryName,
                    TwoLetterCode = ipCountry.TwoLetterCode,
                    ThreeLetterCode = ipCountry.ThreeLetterCode,
                    CreatedAt = DateTime.Now,
                };

                _dbContext.Countries.Add(newCountry);
                _dbContext.SaveChanges();

                var newIpAddres = new IPAddresses
                {
                    CountryId = newCountry.Id,
                    Ip = ipAddres,
                    UpdatedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                };

                _dbContext.IpAddresses.Add(newIpAddres);

                _dbContext.SaveChanges();


            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveIp(string ipAddres, int countryId)
        {
            try
            {
                var newIp = new IPAddresses
                {
                    Ip = ipAddres,
                    CountryId = countryId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };

                _dbContext.IpAddresses.Add(newIp);
                _dbContext.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int SaveCountry(Ip2cCountryResponse country)
        {
            try
            {
                var newCountry = new Countries
                {
                    Name = country.CountryName.Count() > 50 ? country.CountryName.Substring(0, 50) : country.CountryName,
                    TwoLetterCode = country.TwoLetterCode,
                    ThreeLetterCode = country.ThreeLetterCode,
                    CreatedAt = DateTime.Now,
                };

                _dbContext.Countries.Add(newCountry);
                _dbContext.SaveChanges();

                return newCountry.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<CountryReport>> GetCountryReports(List<string> countryCodes)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                string sql = @"
    SELECT 
        c.Name AS CountryName,
        COUNT(ip.Id) AS AddressesCount,
        MAX(ip.UpdatedAt) AS LastAddressUpdated
    FROM 
        IPAddresses ip
    JOIN 
        Countries c ON ip.CountryId = c.Id";

                if (countryCodes.Any())
                {
                    var countryCodesList = string.Join(",", countryCodes.Select(code => $"'{code}'"));
                    sql += $@"
    WHERE 
        c.TwoLetterCode IN ({countryCodesList})";
                }

                sql += @"
    GROUP BY 
        c.Name;";

                var parameters = new
                {
                    CountryCodes = countryCodes.Any() ? countryCodes : null
                };

                var result = await connection.QueryAsync<CountryReport>(sql, parameters);
                return result;
            }
        }
    }
}
