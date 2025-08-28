using UsersService.DTOs;

public class UserInfoWithSellerInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public SalerInfoResponseDto? SalerInfo { get; set; }
}

public class PaginatedUsersWithSellerInfoDto
{
    public IEnumerable<UserInfoWithSellerInfo> Users { get; set; } = new List<UserInfoWithSellerInfo>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}