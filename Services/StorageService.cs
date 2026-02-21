using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace vault_backend.Services;

public class StorageService
{
    private readonly BlobContainerClient _containerClient;

    public StorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"]
            ?? throw new InvalidOperationException("AzureBlobStorage:ConnectionString is not configured.");
        var containerName = configuration["AzureBlobStorage:ContainerName"]
            ?? throw new InvalidOperationException("AzureBlobStorage:ContainerName is not configured.");

        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadAsync(string fileName, Stream fileStream, string contentType)
    {
        // Sanitize to only the base file name to prevent path traversal issues
        var safeName = Path.GetFileName(fileName);
        var blobName = $"{Guid.NewGuid()}/{safeName}";

        var blobClient = _containerClient.GetBlobClient(blobName);
        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = headers });

        return blobClient.Uri.ToString();
    }
}
