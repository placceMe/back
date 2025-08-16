namespace UsersService.DTOs;

public class CreateSalerInfoDto
{
    public string Description { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public List<ContactDto> Contacts { get; set; } = new();
    public Guid UserId { get; set; }
}

public class ContactDto
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}