using Amazon.S3;
using Amazon.S3.Model;

namespace vault_backend.Services;

public class StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrl;

    public StorageService(IConfiguration configuration)
    {
        var accessKey = configuration["CloudflareR2:AccessKeyId"]
            ?? throw new InvalidOperationException("CloudflareR2:AccessKeyId is not configured.");
        var secretKey = configuration["CloudflareR2:SecretAccessKey"]
            ?? throw new InvalidOperationException("CloudflareR2:SecretAccessKey is not configured.");
        var accountId = configuration["CloudflareR2:AccountId"]
            ?? throw new InvalidOperationException("CloudflareR2:AccountId is not configured.");
        _bucketName = configuration["CloudflareR2:BucketName"]
            ?? throw new InvalidOperationException("CloudflareR2:BucketName is not configured.");
        _publicUrl = configuration["CloudflareR2:PublicUrl"]
            ?? throw new InvalidOperationException("CloudflareR2:PublicUrl is not configured.");

        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true
        };
        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadAsync(string fileName, Stream fileStream, string contentType)
    {
        // Sanitize to only the base file name to prevent path traversal issues
        var safeName = Path.GetFileName(fileName);
        var key = $"{Guid.NewGuid()}/{safeName}";
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };
        await _s3Client.PutObjectAsync(request);
        return $"{_publicUrl}/{key}";
    }
}
