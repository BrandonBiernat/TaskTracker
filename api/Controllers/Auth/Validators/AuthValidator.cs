using api.Controllers.Auth.Models;
using FluentValidation;

namespace api.Controllers.Auth.Validators;

public class Register_RequestValidator : AbstractValidator<Register_RequestModel>
{
    public Register_RequestValidator() {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class LoginRequestValidator : AbstractValidator<Login_RequestModel>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}