using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vault_backend.Data;
using vault_backend.Models.DTOs.Media;
using vault_backend.Models.Entities;

namespace vault_backend.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly AppDbContext _db;

    public MediaController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var items = _db.MediaItems
            .Include(m => m.Comments)
            .Select(m => new MediaResponse
            {
                Id = m.Id,
                UserId = m.UserId,
                Username = m.Username,
                Url = m.Url,
                Type = m.Type,
                Caption = m.Caption,
                CreatedAt = m.CreatedAt,
                Likes = m.Likes,
                Comments = m.Comments.Select(c => new CommentResponse
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Username = c.Username,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }).ToList()
            })
            .ToList();

        return Ok(items);
    }

    [HttpPost("upload")]
    public IActionResult Upload([FromBody] UploadMediaRequest request)
    {
        var media = new Media
        {
            Id = Guid.NewGuid().ToString(),
            UserId = request.UserId,
            Username = request.Username,
            Url = request.MediaData,
            Type = request.Type,
            Caption = request.Caption,
            CreatedAt = DateTime.UtcNow
        };

        _db.MediaItems.Add(media);
        _db.SaveChanges();

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
    public IActionResult AddComment([FromBody] AddCommentRequest request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == request.UserId);
        if (user == null)
            return BadRequest(new { message = "User not found" });

        var comment = new Comment
        {
            Id = Guid.NewGuid().ToString(),
            MediaId = request.PhotoId,
            UserId = request.UserId,
            Username = user.Username,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        _db.SaveChanges();

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
    public IActionResult LikeMedia([FromBody] LikeMediaRequest request)
    {
        var existing = _db.Likes.FirstOrDefault(l => l.MediaId == request.PhotoId && l.UserId == request.UserId);
        if (existing != null)
            return Ok();

        var media = _db.MediaItems.FirstOrDefault(m => m.Id == request.PhotoId);
        if (media == null)
            return NotFound();

        _db.Likes.Add(new Like
        {
            Id = Guid.NewGuid().ToString(),
            MediaId = request.PhotoId,
            UserId = request.UserId
        });
        media.Likes++;
        _db.SaveChanges();

        return Ok();
    }
}
