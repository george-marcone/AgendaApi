using FluentValidation;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Interfaces;

namespace CoreFlow.Application.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserService userService)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid")
            .MustAsync(async (email, cancellationToken) => !await userService.EmailExistsAsync(email, cancellationToken: cancellationToken))
            .WithMessage("Email already exists");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches("^\\+?[0-9\\- ]{6,20}$").WithMessage("Phone is invalid")
            .MustAsync(async (phone, cancellationToken) => !await userService.PhoneExistsAsync(phone, cancellationToken: cancellationToken))
            .WithMessage("Phone already exists");
    }
}

