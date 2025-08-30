using Microsoft.AspNetCore.Mvc;
using UsersService.DTOs;
using UsersService.Models;
using UsersService.Services;

namespace UsersService.Controllers;

[ApiController]
[Route("api/salerinfo")]
public class SalerInfoController : ControllerBase
{
    private readonly ISalerInfoService _service;

    public SalerInfoController(ISalerInfoService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalerInfoResponseDto>>> GetAll()
    {
        var salerInfos = await _service.GetAllAsync();
        var response = salerInfos.Select(MapToResponseDto);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalerInfoResponseDto>> Get(Guid id)
    {
        var salerInfo = await _service.GetByIdAsync(id);
        return salerInfo is null ? NotFound() : Ok(MapToResponseDto(salerInfo));
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<SalerInfoResponseDto>> GetByUserId(Guid userId)
    {
        var salerInfo = await _service.GetByUserIdAsync(userId);
        return salerInfo is null ? NotFound() : Ok(MapToResponseDto(salerInfo));
    }

    [HttpPost]
    public async Task<ActionResult<SalerInfoResponseDto>> Create(CreateSalerInfoDto createDto)
    {
        var salerInfo = new SalerInfo
        {
            Description = createDto.Description,
            Schedule = createDto.Schedule,
            Contacts = createDto.Contacts.Select(c => new Contact
            {
                Type = c.Type,
                Value = c.Value
            }).ToList(),
            UserId = createDto.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _service.CreateAsync(salerInfo);
        var response = MapToResponseDto(salerInfo);
        return CreatedAtAction(nameof(Get), new { id = salerInfo.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateSalerInfoDto updateDto)
    {
        var existingSalerInfo = await _service.GetByIdAsync(id);
        if (existingSalerInfo is null) return NotFound();

        existingSalerInfo.Description = updateDto.Description;
        existingSalerInfo.Schedule = updateDto.Schedule;
        existingSalerInfo.Contacts = updateDto.Contacts.Select(c => new Contact
        {
            Type = c.Type,
            Value = c.Value
        }).ToList();
        existingSalerInfo.UpdatedAt = DateTime.UtcNow;

        var updated = await _service.UpdateAsync(existingSalerInfo);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private static SalerInfoResponseDto MapToResponseDto(SalerInfo salerInfo) => new()
    {
        Id = salerInfo.Id,
        CompanyName = salerInfo.CompanyName,
        Description = salerInfo.Description,
        Schedule = salerInfo.Schedule,
        Contacts = salerInfo.Contacts?.Select(c => new ContactDto
        {
            Type = c.Type,
            Value = c.Value
        }).ToList() ?? new List<ContactDto>(),
        UserId = salerInfo.UserId,
        CreatedAt = salerInfo.CreatedAt,
        UpdatedAt = salerInfo.UpdatedAt
    };
}