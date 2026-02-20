using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using vault_backend.Data;
using vault_backend.Models.DTOs.Safety;
using vault_backend.Models.Entities;

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
        var settings = await _db.EmergencySettings.Find(s => s.UserId == userId).FirstOrDefaultAsync();
        return Ok(new EmergencySettingsResponse
        {
            Contacts = contacts,
            CustomMessage = settings?.CustomMessage ?? "I need help! Please contact me."
        });
    }

    [HttpPost("settings")]
    public async Task<IActionResult> SaveSettings([FromBody] SaveEmergencySettingsRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        // Replace all emergency contacts for this user
        await _db.EmergencyContacts.DeleteManyAsync(c => c.UserId == userId);
        if (request.Contacts.Count > 0)
        {
            var contacts = request.Contacts.Select(c => new EmergencyContact
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Name = c.Name,
                Phone = c.Phone
            }).ToList();
            await _db.EmergencyContacts.InsertManyAsync(contacts);
        }

        // Upsert custom message
        var existingSettings = await _db.EmergencySettings.Find(s => s.UserId == userId).FirstOrDefaultAsync();
        if (existingSettings != null)
        {
            var update = Builders<EmergencySettings>.Update.Set(s => s.CustomMessage, request.CustomMessage);
            await _db.EmergencySettings.UpdateOneAsync(s => s.UserId == userId, update);
        }
        else
        {
            await _db.EmergencySettings.InsertOneAsync(new EmergencySettings
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                CustomMessage = request.CustomMessage
            });
        }

        return Ok();
    }

    [HttpPost("trigger")]
    public IActionResult TriggerSos([FromBody] TriggerSosRequest request)
    {
        _logger.LogInformation("SOS triggered by user {UserId} at lat={Lat}, lng={Lng}, notifyPartner={NotifyPartner}",
            request.UserId, request.Location.Lat, request.Location.Lng, request.NotifyPartner);
        return Ok();
    }
}
