using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class ForgotPasswordModelValidator : AbstractValidator<ForgotPasswordModel>
    {
        public ForgotPasswordModelValidator()
        {
            RuleFor(model => model.Email).NotEmpty().WithMessage("Email is required.");
            RuleFor(model => model.Email).EmailAddress().WithMessage("Invalid email format.");
        }
    }
}