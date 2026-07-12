namespace AE.Market.Domain.Exceptions
{
    public static class Auth
    {
        public static readonly DomainException TokenExpiredOrRevoked = new(
            "Auth.Token.ExpiredOrRevoked",
            "The refresh token is invalid or has been expired."
        );

        public static readonly DomainException ReplayAttackDetected = new(
            "Auth.Token.ReplayAttackDetected",
            "Token reuse detected. All sessions revoked."
        );
    }
}
