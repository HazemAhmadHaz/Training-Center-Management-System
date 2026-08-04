using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Services.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateAsync(int personId);

    Task<RefreshToken?> GetActiveTokenAsync(
        string token);

    Task RevokeAsync(
        RefreshToken refreshToken);
}