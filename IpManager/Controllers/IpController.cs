using Microsoft.AspNetCore.Mvc;
using IpManager.Domain.Service;
using Microsoft.AspNetCore.Authorization;

namespace IpManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class IpController : ControllerBase
    {
        private readonly ILogger<IpController> _logger;
        private readonly IIpService _service;

        public IpController(ILogger<IpController> logger, IIpService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("country")]
        public async Task<IActionResult> GetIp([FromQuery] string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                _logger.LogWarning("GetIp called with empty IP address.");
                return BadRequest("IP address is required.");
            }

            var response = await _service.GetIpCountryByIpAddress(ip);
            if (response == null)
            {
                return NotFound();
            }
            return Ok(response);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateIp()
        {
            await _service.UpdateIps();
            return Ok("IP addresses updated.");
        }

        [HttpPost("report")]
        public async Task<IActionResult> GetReport([FromBody] List<string> countryCodes)
        {
            if (countryCodes == null || !countryCodes.Any())
            {
                _logger.LogWarning("GetReport called with empty countryCodes.");
                return BadRequest("Country codes are required.");
            }

            var response = await _service.GetReport(countryCodes);
            return Ok(response);
        }
    }
}