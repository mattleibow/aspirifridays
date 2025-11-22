using System.ComponentModel.DataAnnotations;

namespace BingoBoard.Admin.Models;

/// <summary>
/// Request model for creating a bingo square
/// </summary>
public record CreateBingoSquareRequest
{
    /// <summary>
    /// Unique identifier for the square (e.g., "coffee-mention")
    /// </summary>
    [Required(ErrorMessage = "ID is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "ID must be between 1 and 100 characters")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "ID must contain only lowercase letters, numbers, and hyphens")]
    public required string Id { get; init; }

    /// <summary>
    /// Display label for the square (e.g., "Coffee mentioned")
    /// </summary>
    [Required(ErrorMessage = "Label is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Label must be between 1 and 200 characters")]
    public required string Label { get; init; }

    /// <summary>
    /// Optional type/category (e.g., "meta", "quote", "dev")
    /// </summary>
    [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
    public string? Type { get; init; }
}
