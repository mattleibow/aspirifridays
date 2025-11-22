using System.ComponentModel.DataAnnotations;

namespace BingoBoard.Admin.Models;

/// <summary>
/// DTO for bingo square responses
/// </summary>
public record BingoSquareDto
{
    [Required(ErrorMessage = "ID is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "ID must be between 1 and 100 characters")]
    public required string Id { get; set; }
    
    [Required(ErrorMessage = "Label is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Label must be between 1 and 200 characters")]
    public required string Label { get; set; }
    
    [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
    public string? Type { get; set; }
}
