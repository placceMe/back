using Microsoft.AspNetCore.Mvc;
using UsersService.Models;
using UsersService.Services;
using UsersService.DTOs;

namespace UsersService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{

    private readonly IUserService _service;
    private readonly ISalerInfoService _salerInfoService;
    private readonly ILogger<UsersController> _logger;
    private readonly IConfiguration _configuration;

    public UsersController(IUserService service, ISalerInfoService salerInfoService, ILogger<UsersController> logger, IConfiguration configuration)
    {
        _service = service;
        _salerInfoService = salerInfoService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("config-test")]
    public async Task<ActionResult<object>> ConfigTest()
    {
        return Ok(new
        {
            FrontendBaseUrl = _configuration["Frontend:BaseUrl"],
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            FrontendEnvVar = Environment.GetEnvironmentVariable("Frontend__BaseUrl"),
            AllowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS"),
            AllConfiguration = _configuration.AsEnumerable().Where(x => x.Key.Contains("Frontend"))
        });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll() =>
        Ok(await _service.GetAllAsync());

    [HttpGet("with-saler-info")]
    public async Task<ActionResult<PaginatedUsersWithSellerInfoDto>> GetAllWithSellerInfo([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Limit max page size

        var (users, totalCount) = await _service.GetAllWithSellerInfoAsync(page, pageSize);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedUsersWithSellerInfoDto
        {
            Users = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> Get(Guid id)
    {
        var user = await _service.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("by-ids")]
    public async Task<ActionResult<IEnumerable<User>>> GetByIds([FromBody] GetUsersByIdsRequest request)
    {
        if (request?.Ids == null || !request.Ids.Any())
        {
            return BadRequest("At least one user ID must be provided.");
        }

        var users = await _service.GetByIdsAsync(request.Ids);
        return Ok(users);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, User user)
    {
        if (id != user.Id) return BadRequest();
        var updated = await _service.UpdateAsync(user);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _service.SoftDeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
    public class MakeSellerRequest
    {
        public Guid Id { get; set; }
        public UpdateSalerInfoDto SalerInfo { get; set; } = new UpdateSalerInfoDto();
    }

    [HttpPut("make-saler")]
    public async Task<ActionResult> MakeSeller([FromBody] MakeSellerRequest request)
    {
        var updated = await _service.MakeSellerAsync(request.Id);

        var salerInfo = new SalerInfo
        {
            UserId = request.Id,
            Description = request.SalerInfo.Description,
            CompanyName = request.SalerInfo.CompanyName,
            Schedule = request.SalerInfo.Schedule,
            Contacts = request.SalerInfo.Contacts.Select(c => new Contact
            {
                Type = c.Type,
                Value = c.Value
            }).ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _salerInfoService.CreateAsync(salerInfo);
        if (!updated)
            return NotFound();

        return NoContent();
    }
    [HttpPut("update-roles")]
    public async Task<ActionResult> UpdateRoles([FromBody] UpdateRolesRequest request)
    {
        var updated = await _service.UpdateRolesAsync(request.UserId, request.Roles);
        if (!updated)
            return NotFound();

        return NoContent();
    }
    [HttpPut("{id}/state")]
    public async Task<ActionResult> ChangeState(Guid id, [FromBody] string newState)
    {
        var updated = await _service.ChangeStateAsync(id, newState);
        if (!updated)
            return NotFound();

        return NoContent();
    }
    [HttpGet("{id}/with-seller-info")]
    public async Task<ActionResult<UserInfoWithSellerInfo>> GetWithSellerInfo(Guid id)
    {
        var userWithSellerInfo = await _service.GetWithSellerInfoAsync(id);

        return userWithSellerInfo is null ? NotFound() : Ok(userWithSellerInfo);
    }

}

public class UpdateRolesRequest
{
    public Guid UserId { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class GetUsersByIdsRequest
{
    public List<Guid> Ids { get; set; } = new();
}