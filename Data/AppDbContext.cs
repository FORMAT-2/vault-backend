using Microsoft.EntityFrameworkCore;
using vault_backend.Models.Entities;

namespace vault_backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Media> MediaItems { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<FriendRequest> FriendRequests { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    public DbSet<OtpRecord> OtpRecords { get; set; }
}
