using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StackExchange.Redis;
using vault_backend.Data;
using vault_backend.Models.DTOs.Location;

namespace vault_backend.Controllers;

[ApiController]
[Route("api/location")]
[Authorize]
public class LocationController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly IConnectionMultiplexer _redis;

    public LocationController(MongoDbContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis;
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var redisDb = _redis.GetDatabase();
        var locationData = JsonSerializer.Serialize(new LocationDataResponse
        {
            Lat = request.Lat,
            Lng = request.Lng,
            Accuracy = request.Accuracy,
            Timestamp = request.Timestamp
        });
        await redisDb.StringSetAsync($"location:{userId}", locationData, TimeSpan.FromMinutes(30));
        return Ok();
    }

    [HttpGet("partner")]
    public async Task<IActionResult> GetPartnerLocation()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user?.PartnerId == null)
            return NotFound(new { message = "No partner set" });

        var redisDb = _redis.GetDatabase();
        var locationJson = await redisDb.StringGetAsync($"location:{user.PartnerId}");
        if (locationJson.IsNullOrEmpty)
            return NotFound(new { message = "No location available for partner" });

        var location = JsonSerializer.Deserialize<LocationDataResponse>(locationJson!);
        return Ok(location);
    }
}
