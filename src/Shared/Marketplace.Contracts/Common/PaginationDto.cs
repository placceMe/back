using System.ComponentModel.DataAnnotations;

namespace Marketplace.Contracts.Common;

/// <summary>
/// Base DTO for pagination parameters
/// </summary>
public class PaginationDto
{
    /// <summary>
    /// Number of items to skip (default: 0)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative")]
    public int Offset { get; set; } = 0;

    /// <summary>
    /// Number of items to return (default: 20, max: 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
    public int Limit { get; set; } = 20;
}

/// <summary>
/// Pagination metadata for responses
/// </summary>
public class PaginationInfo
{
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

/// <summary>
/// Generic response wrapper with pagination
/// </summary>
/// <typeparam name="T">Type of data items</typeparam>
public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public PaginationInfo Pagination { get; set; } = new();
}