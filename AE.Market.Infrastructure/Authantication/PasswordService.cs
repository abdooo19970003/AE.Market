using AE.Market.Application.Common.Interfaces;
using System.Security.Cryptography;

namespace AE.Market.Infrastructure.Authantication
{
    public sealed class PasswordService : IPasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 64;
        private const int Iterations = 350_000;

        private readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;
        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password,salt,Iterations,Algorithm,HashSize);

            return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
        }
            
        public bool VerifyPassword(string password, string Hash)
        {
            string[] parts = Hash.Split("-");
            byte[] hash = Convert.FromHexString(parts[0]);
            byte[] salt = Convert.FromHexString(parts[1]);

            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
            //return hash.SequenceEqual(inputHash);
            return CryptographicOperations.FixedTimeEquals(hash, inputHash);
        }
    }
}
