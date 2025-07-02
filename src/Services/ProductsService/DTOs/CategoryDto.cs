namespace ProductsService.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}