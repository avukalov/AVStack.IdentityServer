using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordModel>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(model => model.Email).NotEmpty().WithMessage("Email is required.");
        }
    }
}