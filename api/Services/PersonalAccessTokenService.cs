using System.Security.Cryptography;
using System.Text;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Planera.Data;
using Planera.Data.Dto;

namespace Planera.Services;

public class PersonalAccessTokenService(DataContext context)
{
    private readonly DataContext _context = context;
    private readonly PasswordHasher<string> _hasher = new();

    public async Task<ErrorOr<PersonalAccessTokenMetadataDto>> GetMetadata(string userId)
    {
        var token = await _context.PersonalAccessTokens.FindAsync(userId);
        if (token == null)
            return Error.NotFound();

        return new PersonalAccessTokenMetadataDto
        {
            UserId = userId,
            CreatedAtUtc = token.CreatedAtUtc,
        };
    }

    public async Task<ErrorOr<string>> CreateAsync(string userId)
    {
        var secret = GenerateSecret();
        var entry = new PersonalAccessToken
        {
            UserId = userId,
            Secret = _hasher.HashPassword(userId, secret),
            CreatedAtUtc = DateTime.UtcNow,
        };

        if (await _context.PersonalAccessTokens.AnyAsync(x => x.UserId == userId))
        {
            _context.PersonalAccessTokens.Update(entry);
        }
        else
        {
            await _context.PersonalAccessTokens.AddAsync(entry);
        }

        await _context.SaveChangesAsync();

        return Convert.ToBase64String(Encoding.Default.GetBytes($"{secret}\n{userId}"));
    }

    public async Task<ErrorOr<Deleted>> RevokeAsync(string userId)
    {
        var token = await _context.PersonalAccessTokens.FindAsync(userId);
        if (token == null)
            return Error.NotFound();

        _context.PersonalAccessTokens.Remove(token);
        await _context.SaveChangesAsync();

        return new ErrorOr.Deleted();
    }

    public async Task<string?> ValidateAsync(string token)
    {
        var parts = Encoding.Default.GetString(Convert.FromBase64String(token)).Split('\n');
        var secret = parts[0];
        var userId = parts[1];

        var entry = await _context.PersonalAccessTokens.FindAsync(userId);
        if (entry == null)
            return null;

        var result = _hasher.VerifyHashedPassword(userId, entry.Secret, secret);
        if (result == PasswordVerificationResult.Success)
            return userId;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            entry.Secret = _hasher.HashPassword(userId, secret);
            _context.PersonalAccessTokens.Update(entry);
            await _context.SaveChangesAsync();

            return userId;
        }

        return null;
    }

    public static string GenerateSecret()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        return Convert.ToHexString(bytes);
    }
}
