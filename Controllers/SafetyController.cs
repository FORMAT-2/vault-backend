using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using vault_backend.Data;
using vault_backend.Models.DTOs.Safety;

namespace vault_backend.Controllers;

[ApiController]
[Route("api/safety")]
[Authorize]
public class SafetyController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly ILogger<SafetyController> _logger;

    public SafetyController(MongoDbContext db, ILogger<SafetyController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var contacts = await _db.EmergencyContacts.Find(c => c.UserId == userId).ToListAsync();
        return Ok(new EmergencySettingsResponse
        {
            Contacts = contacts,
            CustomMessage = "I need help! Please contact me."
        });
    }

    [HttpPost("trigger")]
    public IActionResult TriggerSos([FromBody] TriggerSosRequest request)
    {
        _logger.LogInformation("SOS triggered by user {UserId} at lat={Lat}, lng={Lng}",
            request.UserId, request.Location.Lat, request.Location.Lng);
        return Ok();
    }
}
