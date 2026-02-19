using MongoDB.Driver;
using vault_backend.Models.Entities;

namespace vault_backend.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString is not configured.");
        var databaseName = configuration["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName is not configured.");
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Media> MediaItems => _database.GetCollection<Media>("media");
    public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");
    public IMongoCollection<Like> Likes => _database.GetCollection<Like>("likes");
    public IMongoCollection<FriendRequest> FriendRequests => _database.GetCollection<FriendRequest>("friendRequests");
    public IMongoCollection<Message> Messages => _database.GetCollection<Message>("messages");
    public IMongoCollection<EmergencyContact> EmergencyContacts => _database.GetCollection<EmergencyContact>("emergencyContacts");
    public IMongoCollection<OtpRecord> OtpRecords => _database.GetCollection<OtpRecord>("otpRecords");
}
