using System.Security.Cryptography;

namespace Business.OrderMethod
{
    public class PasswordSecurity
    {
        // Parola hashleme metodu
        public static string HashPassword(string password, byte[] salt = null)
        {
            salt ??= RandomNumberGenerator.GetBytes(16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            var result = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, result, salt.Length, hash.Length);

            return Convert.ToBase64String(result);
        }

        // Parola doğrulama metodu
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            byte[] fullHash;

            try
            {
                fullHash = Convert.FromBase64String(hashedPassword);
            }
            catch
            {
                // Base64 formatında değilse direkt false döner
                return false;
            }

            // Beklenen minimum uzunluk: 16 salt + 32 hash = 48 byte
            if (fullHash.Length < 48)
                return false;

            var salt = new byte[16];
            Buffer.BlockCopy(fullHash, 0, salt, 0, 16);

            var storedHash = new byte[32];
            Buffer.BlockCopy(fullHash, 16, storedHash, 0, 32);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
    }
}
