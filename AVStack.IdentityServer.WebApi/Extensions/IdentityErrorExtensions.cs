using Microsoft.AspNetCore.Identity;

namespace AVStack.IdentityServer.WebApi.Extensions
{
    public static class IdentityErrorExtensions
    {
        public static IdentityError InvalidCredentials => new ()
        {
            Code = "Login",
            Description = "Invalid credentials."
        };

        public static IdentityError EmailNotConfirmed => new ()
        {
            Code = "Login",
            Description = "Email address is not confirmed yet."
        };

        public static IdentityError EmailConfirmed => new ()
        {
            Code = "EmailConfirmation",
            Description = "Email address is already confirmed."
        };

        public static IdentityError EmailNotExist => new ()
        {
            Code = "PasswordReset",
            Description = "Email address doesn't exist."
        };

        public static IdentityError LockedOut => new ()
        {
            Code = "Login",
            Description = "User is locked out."
        };

        public static IdentityError NotAllowed => new ()
        {
            Code = "Login",
            Description = "User is not allowed to sign-in."
        };

        public static IdentityError RequiresTwoFactor => new ()
        {
            Code = "Login",
            Description = "Sign-in requires two-factor."
        };
    }
}