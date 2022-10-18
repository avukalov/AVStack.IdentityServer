using AVStack.IdentityServer.WebApi.Models.Requests;
using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Api.Validators
{
    public class ChangeUserPasswordRequestValidator : AbstractValidator<ChangeUserPasswordRequest>
    {
        public ChangeUserPasswordRequestValidator()
        {
            RuleFor(model => model.UserId).NotEmpty().WithMessage("UserId is required.");
            RuleFor(model => model.Password).NotEmpty().WithMessage("Password is required.");
            RuleFor(model => model.Password)
                .Equal(model => model.ConfirmPassword)
                .WithMessage("The password and confirm password do not match.");
        }
    }
}