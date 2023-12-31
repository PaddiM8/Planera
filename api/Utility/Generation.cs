using System.Security.Cryptography;

namespace Planera.Utility;

public static class Generation
{
    public static string GenerateJwtKey()
    {
        byte[] keyBytes = new byte[32]; // 256 bit key
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(keyBytes);

        return Convert.ToBase64String(keyBytes);
    }
}