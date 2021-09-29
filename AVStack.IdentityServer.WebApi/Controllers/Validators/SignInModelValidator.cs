using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class SignInModelValidator : AbstractValidator<SignInModel>
    {
        public SignInModelValidator()
        {
            RuleFor(model => model.UserName).NotEmpty().WithMessage("Email or Username is required.");
            RuleFor(model => model.Password).NotEmpty().WithMessage("Password is required.");
            RuleFor(model => model.ReturnUrl).NotEmpty();
        }
    }
}