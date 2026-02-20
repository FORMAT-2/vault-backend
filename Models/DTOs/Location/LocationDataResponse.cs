namespace vault_backend.Models.DTOs.Location;

public class LocationDataResponse
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double Accuracy { get; set; }
    public string Timestamp { get; set; } = string.Empty;
}
