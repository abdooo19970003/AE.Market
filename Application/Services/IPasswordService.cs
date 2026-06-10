namespace AE.Market.Application.Services
{
    public interface IPasswordService
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string password, string Hash);
    }
}
