namespace AE.Market.Domain.Exceptions
{
    public static class Auth
    {
        public static readonly DomainException TokenNotFound = new(
            "Auth.Token.NotFound",
            "The provided refresh token was not found."
        );

        public static readonly DomainException TokenExpiredOrRevoked = new(
            "Auth.Token.ExpiredOrRevoked",
            "The refresh token is invalid or has been expired."
        );

        public static readonly DomainException ReplayAttackDetected = new(
            "Auth.Token.ReplayAttackDetected",
            "Token reuse detected. All sessions revoked."
        );

        public static readonly DomainException InvalidEmailAddress = new(
            "Auth.Email.Invalid",
            "Invalid email address. Email should be of format 'abc@def.xyz'"
        );

        public static readonly DomainException EmptyEmailAddress = new(
            "Auth.Email.Empty",
            "Invalid email address. Email should be of format 'abc@def.xyz'"
        );

        public static readonly DomainException EmailAlreadyExists = new(
            "Auth.Email.AlreadyExist",
            "Invalid email address. This email already exists in our database, try login."
        );
    }

}
