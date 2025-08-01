using Microsoft.AspNetCore.Mvc;
using IpManager.Domain.Service;

namespace IpManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IpController: ControllerBase
    {
        private readonly ILogger<IpController> _logger;
        private readonly IIpService _service;

        public IpController(ILogger<IpController> logger, IIpService service) 
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        [Route("get-ip")]
        public IActionResult GetIp (string ip)
        {
            var response = _service.GetIpCountryByIpAddress(ip);
            return Ok(response);
        }

        [HttpPut]
        [Route("update-ip")]
        public async Task<IActionResult> UpdateIp()
        {
            _service.UpdateIps();
            return Ok();
        }

        [HttpPost]
        [Route("get-report")]
        public async Task<IActionResult> GetReport(List<string> countryCodes)
        {
            var response = await _service.GetReport(countryCodes);
            return Ok(response);
        }
    }
}
