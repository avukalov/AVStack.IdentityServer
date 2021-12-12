using AVStack.IdentityServer.WebApi.Models.Requests;
using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class SignOutModelValidator : AbstractValidator<SignOutRequest>
    {
        public SignOutModelValidator()
        {
            RuleFor(model => model.LogoutId).NotEmpty().WithMessage("LogoutId is required.");
        }
    }
}