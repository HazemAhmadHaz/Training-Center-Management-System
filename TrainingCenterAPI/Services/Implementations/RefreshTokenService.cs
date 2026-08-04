using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Services.Interfaces;


namespace TrainingCenterAPI.Services.Implementations;


public class RefreshTokenService : IRefreshTokenService
{

    private readonly TrainingCenterDbContext _context;


    public RefreshTokenService(
        TrainingCenterDbContext context)
    {
        _context = context;
    }



    public async Task<RefreshToken> CreateAsync(
        int personId)
    {

        var token =
            Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));


        var refreshToken = new RefreshToken
        {
            Token = token,

            PersonId = personId,

            CreatedAt = DateTime.UtcNow,

            ExpiresAt =
                DateTime.UtcNow.AddDays(7)
        };


        await _context.RefreshTokens
            .AddAsync(refreshToken);


        await _context.SaveChangesAsync();


        return refreshToken;
    }




    public async Task<RefreshToken?> GetActiveTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.Person)
            .FirstOrDefaultAsync(rt =>
                rt.Token == token &&
                rt.RevokedAt == null &&
                rt.ExpiresAt > DateTime.UtcNow);
    }





    public async Task RevokeAsync(
        RefreshToken refreshToken)
    {

        refreshToken.RevokedAt =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();
    }

}