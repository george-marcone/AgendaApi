using FluentValidation;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Interfaces;

namespace CoreFlow.Application.Validators;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(IUserService userService)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid")
            .MustAsync(async (command, email, cancellationToken) => !await userService.EmailExistsAsync(email, command.Id, cancellationToken))
            .WithMessage("Email already exists");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches("^\\+[0-9]{13}$").WithMessage("Phone is invalid")
            .MustAsync(async (command, phone, cancellationToken) => !await userService.PhoneExistsAsync(phone, command.Id, cancellationToken))
            .WithMessage("Phone already exists");
    }
}
