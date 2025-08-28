using System.ComponentModel.DataAnnotations;

namespace ProductsService.DTOs;

public class UpdateFeedbackStatusDto
{
    [Required]
    public string Status { get; set; }

}

