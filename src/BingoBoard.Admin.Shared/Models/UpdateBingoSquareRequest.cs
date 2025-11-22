using System.ComponentModel.DataAnnotations;

namespace BingoBoard.Admin.Models;

/// <summary>
/// Request model for updating a bingo square
/// </summary>
public record UpdateBingoSquareRequest
{
    /// <summary>
    /// Display label for the square
    /// </summary>
    [Required(ErrorMessage = "Label is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Label must be between 1 and 200 characters")]
    public required string Label { get; init; }

    /// <summary>
    /// Optional type/category
    /// </summary>
    [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
    public string? Type { get; init; }
}
