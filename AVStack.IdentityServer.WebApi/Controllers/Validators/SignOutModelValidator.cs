using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class SignOutModelValidator : AbstractValidator<SignOutModel>
    {
        public SignOutModelValidator()
        {
            RuleFor(model => model.LogoutId).NotEmpty().WithMessage("LogoutId is required.");
        }
    }
}