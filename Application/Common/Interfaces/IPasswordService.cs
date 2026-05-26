namespace AE.Market.Application.Common.Interfaces
{
    public interface IPasswordService
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string password, string Hash);
    }
}
