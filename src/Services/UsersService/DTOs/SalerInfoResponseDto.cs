namespace UsersService.DTOs;

public class SalerInfoResponseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public List<ContactDto> Contacts { get; set; } = new();
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}