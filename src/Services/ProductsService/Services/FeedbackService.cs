using ProductsService.DTOs;
using ProductsService.Models;
using ProductsService.Extensions;
using ProductsService.Repositories.Interfaces;
using ProductsService.Services.Interfaces;
using ProductsService.Services; // For UsersServiceClient

namespace ProductsService.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IProductsRepository _productRepository;
    private readonly UsersServiceClient _usersServiceClient;

    public FeedbackService(IFeedbackRepository feedbackRepository, IProductsRepository productRepository, UsersServiceClient usersServiceClient)
    {
        _feedbackRepository = feedbackRepository;
        _productRepository = productRepository;
        _usersServiceClient = usersServiceClient;
    }

    public async Task<IEnumerable<FeedbackDto>> GetAllFeedbacksAsync()
    {
        var feedbacks = await _feedbackRepository.GetAllAsync();
        var dtos = new List<FeedbackDto>();
        foreach (var feedback in feedbacks)
        {
            var dto = await MapToDtoAsync(feedback);
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<IEnumerable<FeedbackDto>> GetAllFeedbacksAsync(int offset, int limit)
    {
        var feedbacks = await _feedbackRepository.GetAllFeedbacksAsync(offset, limit);
        var dtos = new List<FeedbackDto>();
        foreach (var feedback in feedbacks)
        {
            var dto = await MapToDtoAsync(feedback);
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<FeedbackDto?> GetFeedbackByIdAsync(Guid id)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(id);
        return feedback != null ? await MapToDtoAsync(feedback) : null;
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksByProductIdAsync(Guid productId)
    {
        var feedbacks = await _feedbackRepository.GetByProductIdAsync(productId);
        var dtos = new List<FeedbackDto>();
        foreach (var feedback in feedbacks)
        {
            var dto = await MapToDtoAsync(feedback);
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksByProductIdAsync(Guid productId, int offset, int limit)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksByProductIdAsync(productId, offset, limit);
        var dtos = new List<FeedbackDto>();
        foreach (var feedback in feedbacks)
        {
            var dto = await MapToDtoAsync(feedback);
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksByUserIdAsync(Guid userId)
    {
        var feedbacks = await _feedbackRepository.GetByUserIdAsync(userId);
        var dtos = new List<FeedbackDto>();
        foreach (var feedback in feedbacks)
        {
            var dto = await MapToDtoAsync(feedback);
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksByUserIdAsync(Guid userId, int offset, int limit)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksByUserIdAsync(userId, offset, limit);
        var dtos = new List<FeedbackDto>();
        foreach (var feedback in feedbacks)
        {
            var dto = await MapToDtoAsync(feedback);
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackDto createFeedbackDto)
    {
        var ratingAverage = (createFeedbackDto.RatingService + createFeedbackDto.RatingSpeed +
                           createFeedbackDto.RatingDescription + createFeedbackDto.RatingAvailable) / 4;

        var feedback = new Feedback
        {
            Content = createFeedbackDto.Content,
            RatingService = createFeedbackDto.RatingService,
            RatingSpeed = createFeedbackDto.RatingSpeed,
            RatingDescription = createFeedbackDto.RatingDescription,
            RatingAvailable = createFeedbackDto.RatingAvailable,
            RatingAverage = ratingAverage,
            ProductId = createFeedbackDto.ProductId,
            UserId = createFeedbackDto.UserId
        };

        var createdFeedback = await _feedbackRepository.CreateAsync(feedback);
        return await MapToDtoAsync(createdFeedback);
    }

    public async Task<FeedbackDto> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto updateFeedbackDto)
    {
        var existingFeedback = await _feedbackRepository.GetByIdAsync(id);
        if (existingFeedback == null)
        {
            throw new ArgumentException("Feedback not found");
        }

        existingFeedback.Content = updateFeedbackDto.Content;
        existingFeedback.RatingService = updateFeedbackDto.RatingService;
        existingFeedback.RatingSpeed = updateFeedbackDto.RatingSpeed;
        existingFeedback.RatingDescription = updateFeedbackDto.RatingDescription;
        existingFeedback.RatingAvailable = updateFeedbackDto.RatingAvailable;
        existingFeedback.RatingAverage = (updateFeedbackDto.RatingService + updateFeedbackDto.RatingSpeed +
                                         updateFeedbackDto.RatingDescription + updateFeedbackDto.RatingAvailable) / 4;

        var updatedFeedback = await _feedbackRepository.UpdateAsync(existingFeedback);
        return await MapToDtoAsync(updatedFeedback);
    }

    public async Task<bool> DeleteFeedbackAsync(Guid id)
    {
        return await _feedbackRepository.DeleteAsync(id);
    }

    public async Task<FeedbackSummaryDto> GetFeedbackSummaryByProductIdAsync(Guid productId)
    {
        var averageRating = await _feedbackRepository.GetAverageRatingByProductIdAsync(productId);
        var totalFeedbacks = await _feedbackRepository.GetFeedbackCountByProductIdAsync(productId);
        var recentFeedbacks = await _feedbackRepository.GetByProductIdAsync(productId);

        var recentFeedbackDtos = new List<FeedbackDto>();
        foreach (var feedback in recentFeedbacks.Take(5))
        {
            var dto = await MapToDtoAsync(feedback);
            recentFeedbackDtos.Add(dto);
        }

        return new FeedbackSummaryDto
        {
            ProductId = productId,
            AverageRating = averageRating,
            TotalFeedbacks = totalFeedbacks,
            RecentFeedbacks = recentFeedbackDtos
        };
    }

    private async Task<FeedbackDto> MapToDtoAsync(Feedback feedback)
    {
        var userDto = await _usersServiceClient.GetUserByIdAsync(feedback.UserId.ToString());
        FeedbackUserDto? feedbackUserDto = userDto != null
            ? new FeedbackUserDto
            {
                Id = userDto.Id,
                Name = userDto.Name,
                Surname = userDto.Surname
            }
            : null;

        return new FeedbackDto
        {
            Id = feedback.Id,
            Content = feedback.Content,
            RatingService = feedback.RatingService,
            RatingSpeed = feedback.RatingSpeed,
            RatingDescription = feedback.RatingDescription,
            RatingAvailable = feedback.RatingAvailable,
            RatingAverage = feedback.RatingAverage,
            ProductId = feedback.ProductId,
            ProductName = feedback.Product?.Title,
            UserId = feedback.UserId,
            CreatedAt = feedback.CreatedAt,
            User = feedbackUserDto
        };
    }
}