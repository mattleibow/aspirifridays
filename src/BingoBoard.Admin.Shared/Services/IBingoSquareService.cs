using BingoBoard.Admin.Models;

namespace BingoBoard.Admin.Services;

/// <summary>
/// Service for managing bingo squares in file storage
/// </summary>
public interface IBingoSquareService
{
    Task<List<BingoSquareDto>> GetAllSquaresAsync();
    Task<BingoSquareDto?> GetSquareByIdAsync(string id);
    Task<BingoSquareDto> CreateSquareAsync(BingoSquareDto square);
    Task<BingoSquareDto?> UpdateSquareAsync(string id, BingoSquareDto square);
    Task<bool> DeleteSquareAsync(string id);
}
