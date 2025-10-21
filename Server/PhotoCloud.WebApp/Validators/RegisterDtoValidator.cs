using System.Text.RegularExpressions;
using FluentValidation;
using Infractructure.DTO.WebAppClientDTO;

namespace PhotoCloud.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(30);
        
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("At least one uppercase letter is required.")
            .Matches("[0-9]").WithMessage("At least one number is required.");
        
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
            .WithMessage("Passwords do not match.");
        
        RuleFor(x => x).Must(x => !string.IsNullOrWhiteSpace(x.Email)
                                  || !string.IsNullOrWhiteSpace(x.UserName)
                                  || !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Need to enter a valid email, phone number or username");
        
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
            RuleFor(x => x.Email).EmailAddress());

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
            RuleFor(x => x.PhoneNumber)
                .MinimumLength(10).WithMessage("PhoneNumber must not be less than 10 characters.")
                .MaximumLength(20).WithMessage("PhoneNumber must not exceed 50 characters.")
                .Matches(new Regex(@"((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}")).WithMessage("PhoneNumber not valid"));
    }
}