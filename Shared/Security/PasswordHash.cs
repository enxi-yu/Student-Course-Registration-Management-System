using System.Security.Cryptography;
using System.Text;

namespace StudentCourse.Shared.Security;

public static class PasswordHash
{
    private const int Iterations = 210000;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string stored, string password, out bool needsUpgrade)
    {
        needsUpgrade = !stored.StartsWith("PBKDF2$", StringComparison.Ordinal);
        if (needsUpgrade)
        {
            return string.Equals(stored, password, StringComparison.Ordinal)
                || string.Equals(stored, Md5(password), StringComparison.OrdinalIgnoreCase);
        }

        string[] parts = stored.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations)) return false;
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(parts[2]), iterations, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, Convert.FromBase64String(parts[3]));
    }

    private static string Md5(string value) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
