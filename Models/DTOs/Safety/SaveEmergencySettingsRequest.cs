namespace vault_backend.Models.DTOs.Safety;

public class SaveEmergencySettingsRequest
{
    public List<EmergencyContactDto> Contacts { get; set; } = new();
    public string CustomMessage { get; set; } = string.Empty;
}
