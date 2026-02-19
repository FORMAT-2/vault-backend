using vault_backend.Models.Entities;

namespace vault_backend.Models.DTOs.Safety;

public class EmergencySettingsResponse
{
    public List<EmergencyContact> Contacts { get; set; } = new();
    public string CustomMessage { get; set; } = string.Empty;
}
