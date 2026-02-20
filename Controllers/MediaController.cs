using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using vault_backend.Data;
using vault_backend.Models.DTOs.Media;
using vault_backend.Models.Entities;
using vault_backend.Services;

namespace vault_backend.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly StorageService _storage;

    public MediaController(MongoDbContext db, StorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var mediaItems = await _db.MediaItems.Find(_ => true).ToListAsync();
        var mediaIds = mediaItems.Select(m => m.Id).ToList();
        var comments = await _db.Comments.Find(c => mediaIds.Contains(c.MediaId)).ToListAsync();
        var commentsByMedia = comments.GroupBy(c => c.MediaId).ToDictionary(g => g.Key, g => g.ToList());

        var result = mediaItems.Select(m => new MediaResponse
        {
            Id = m.Id,
            UserId = m.UserId,
            Username = m.Username,
            Url = m.Url,
            Type = m.Type,
            Caption = m.Caption,
            CreatedAt = m.CreatedAt,
            Likes = m.Likes,
            Comments = commentsByMedia.TryGetValue(m.Id, out var commentList)
                ? commentList.Select(c => new CommentResponse
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Username = c.Username,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }).ToList()
                : new List<CommentResponse>()
        }).ToList();

        return Ok(result);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromBody] UploadMediaRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        string url;
        // Parse Base64 data URL: "data:<mime>;base64,<data>"
        var mediaData = request.MediaData;
        var commaIndex = mediaData.IndexOf(',');
        if (commaIndex < 0)
            return BadRequest(new { message = "Invalid mediaData format" });

        var header = mediaData[..commaIndex]; // e.g. "data:image/jpeg;base64"
        var base64Data = mediaData[(commaIndex + 1)..];
        var bytes = Convert.FromBase64String(base64Data);

        // Extract content type and extension
        var contentType = header.Replace("data:", "").Replace(";base64", "");
        var extension = contentType.Split('/').LastOrDefault() ?? "bin";
        var fileName = $"{Guid.NewGuid()}.{extension}";

        using var stream = new MemoryStream(bytes);
        url = await _storage.UploadAsync(fileName, stream, contentType);

        var media = new Media
        {
            Id = Guid.NewGuid().ToString(),
            UserId = request.UserId ?? userId,
            Username = request.Username ?? userName,
            Url = url,
            Type = request.Type,
            Caption = request.Caption,
            CreatedAt = DateTime.UtcNow
        };

        await _db.MediaItems.InsertOneAsync(media);

        return Ok(new MediaResponse
        {
            Id = media.Id,
            UserId = media.UserId,
            Username = media.Username,
            Url = media.Url,
            Type = media.Type,
            Caption = media.Caption,
            CreatedAt = media.CreatedAt,
            Likes = media.Likes
        });
    }

    [HttpPost("comment")]
    public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
            return BadRequest(new { message = "User not found" });

        var comment = new Comment
        {
            Id = Guid.NewGuid().ToString(),
            MediaId = request.PhotoId,
            UserId = userId,
            Username = user.Username,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };

        await _db.Comments.InsertOneAsync(comment);

        return Ok(new CommentResponse
        {
            Id = comment.Id,
            UserId = comment.UserId,
            Username = comment.Username,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt
        });
    }

    [HttpPost("like")]
    public async Task<IActionResult> LikeMedia([FromBody] LikeMediaRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var existing = await _db.Likes.Find(l => l.MediaId == request.PhotoId && l.UserId == userId).AnyAsync();
        if (existing)
            return Ok();

        var media = await _db.MediaItems.Find(m => m.Id == request.PhotoId).FirstOrDefaultAsync();
        if (media == null)
            return NotFound();

        await _db.Likes.InsertOneAsync(new Like
        {
            Id = Guid.NewGuid().ToString(),
            MediaId = request.PhotoId,
            UserId = userId
        });

        var update = Builders<Media>.Update.Inc(m => m.Likes, 1);
        await _db.MediaItems.UpdateOneAsync(m => m.Id == request.PhotoId, update);

        return Ok();
    }
}
