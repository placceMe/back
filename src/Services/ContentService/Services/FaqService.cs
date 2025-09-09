using ContentService.DTOs;
using ContentService.Models;
using ContentService.Repositories;

namespace ContentService.Services;

public interface IFaqService
{
    Task<IEnumerable<FaqDto>> GetAllFaqsAsync();
    Task<IEnumerable<FaqDto>> GetVisibleFaqsAsync();
    Task<FaqDto?> GetFaqByIdAsync(int id);
    Task<FaqDto> CreateFaqAsync(CreateFaqDto createFaqDto, string userId);
    Task<FaqDto?> UpdateFaqAsync(int id, UpdateFaqDto updateFaqDto, string userId);
    Task<bool> DeleteFaqAsync(int id);
}

public class FaqService : IFaqService
{
    private readonly IFaqRepository _faqRepository;

    public FaqService(IFaqRepository faqRepository)
    {
        _faqRepository = faqRepository;
    }

    public async Task<IEnumerable<FaqDto>> GetAllFaqsAsync()
    {
        var faqs = await _faqRepository.GetAllAsync();
        return faqs.Select(MapToDto);
    }

    public async Task<IEnumerable<FaqDto>> GetVisibleFaqsAsync()
    {
        var faqs = await _faqRepository.GetVisibleAsync();
        return faqs.Select(MapToDto);
    }

    public async Task<FaqDto?> GetFaqByIdAsync(int id)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        return faq != null ? MapToDto(faq) : null;
    }

    public async Task<FaqDto> CreateFaqAsync(CreateFaqDto createFaqDto, string userId)
    {
        var faq = new Faq
        {
            Question = createFaqDto.Question,
            Content = createFaqDto.Content,
            IsVisible = createFaqDto.IsVisible,
            Order = createFaqDto.Order,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        var createdFaq = await _faqRepository.CreateAsync(faq);
        return MapToDto(createdFaq);
    }

    public async Task<FaqDto?> UpdateFaqAsync(int id, UpdateFaqDto updateFaqDto, string userId)
    {
        var existingFaq = await _faqRepository.GetByIdAsync(id);
        if (existingFaq == null)
            return null;

        existingFaq.Question = updateFaqDto.Question;
        existingFaq.Content = updateFaqDto.Content;
        existingFaq.IsVisible = updateFaqDto.IsVisible;
        existingFaq.Order = updateFaqDto.Order;
        existingFaq.UpdatedBy = userId;

        var updatedFaq = await _faqRepository.UpdateAsync(existingFaq);
        return MapToDto(updatedFaq);
    }

    public async Task<bool> DeleteFaqAsync(int id)
    {
        return await _faqRepository.DeleteAsync(id);
    }

    private static FaqDto MapToDto(Faq faq)
    {
        return new FaqDto
        {
            Id = faq.Id,
            Question = faq.Question,
            Content = faq.Content,
            IsVisible = faq.IsVisible,
            Order = faq.Order,
            CreatedAt = faq.CreatedAt,
            UpdatedAt = faq.UpdatedAt,
            CreatedBy = faq.CreatedBy,
            UpdatedBy = faq.UpdatedBy
        };
    }
}
