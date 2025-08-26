namespace UsersService.DTOs;

public class UpdateSalerInfoDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public List<ContactDto> Contacts { get; set; } = new();
}