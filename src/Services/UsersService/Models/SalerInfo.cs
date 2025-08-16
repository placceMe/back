namespace UsersService.Models;

public class SalerInfo
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public string Schedule { get; set; }
    public List<Contact> Contacts { get; set; } = new();
    public Guid UserId { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Contact
{
    public string Type { get; set; } // e.g., Email, Phone
    public string Value { get; set; }
}