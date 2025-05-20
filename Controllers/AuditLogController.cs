using KDomBackend.Models.DTOs.AuditLog;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/audit-log")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _auditLogService.GetAllAsync();
            return Ok(logs);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetFiltered([FromQuery] AuditLogFilterDto filters)
        {
            var result = await _auditLogService.GetFilteredAsync(filters);
            return Ok(result);
        }


    }
}
