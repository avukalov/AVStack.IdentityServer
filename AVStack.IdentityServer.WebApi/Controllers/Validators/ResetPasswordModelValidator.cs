using AVStack.IdentityServer.WebApi.Models.Requests;
using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class ResetPasswordModelValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordModelValidator()
        {
            RuleFor(p => p.UserId).NotNull().WithMessage("UserId is required");
            RuleFor(p => p.Token).NotEmpty().WithMessage("Token is required");
            RuleFor(model => model.Password).NotEmpty().WithMessage("Password is required.");
            RuleFor(model => model.Password).Equal(model => model.ConfirmPassword)
                .WithMessage("The password and confirmation password do not match.");
        }
    }
}