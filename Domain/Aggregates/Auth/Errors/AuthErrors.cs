using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.Errors
{
    public static class AuthErrors
    {
        public static readonly Error TokenNotFound = new(
            "Auth.Token.NotFound",
            "The provided refresh token was not found."
        );
        public static readonly Error TokenExpiredOrRevoked = new(
            "Auth.Token.ExpiredOrRevoked",
            "The refresh token is invalid or has been expired."
        );
        public static readonly Error ReplayAttackDetected = new(
            "Auth.Token.ReplayAttackDetected",
            "Token reuse detected. All sessions revoked."
        );
        public static readonly Error InvalidEmailAddress = new(
            "Auth.Email.Invalid",
            "Invalid email address. Email should be of format 'abc@def.xyz'"
        );
        public static readonly Error EmptyEmailAddress = new(
            "Auth.Email.Empty",
            "Invalid email address. Email should be of format 'abc@def.xyz'"
        );
        public static readonly Error EmailAlreadyExists = new(
            "Auth.Email.AlreadyExist",
            "Invalid email address.This email is already exist in our database try login"
        );
        public static readonly Error UserNotFound = new(
            "Auth.User.NotFound",
            "Invalid Credentials"
        );
    }
}
