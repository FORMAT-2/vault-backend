namespace vault_backend.Models.DTOs.Safety;

public class TriggerSosRequest
{
    public string UserId { get; set; } = string.Empty;
    public LocationDto Location { get; set; } = new();
    public bool NotifyPartner { get; set; }
}
