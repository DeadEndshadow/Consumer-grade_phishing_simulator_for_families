using System.Security.Cryptography;

namespace PhishingSimulator.Web;

static class PasswordHelper
{
    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored)
    {
        var sep = stored.IndexOf(':');
        if (sep < 0) return false;
        var salt = Convert.FromBase64String(stored[..sep]);
        var expected = Convert.FromBase64String(stored[(sep + 1)..]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
