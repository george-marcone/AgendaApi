using FluentValidation;
using CoreFlow.Application.Commands;

namespace CoreFlow.Application.Validators;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).Matches("^\\+?[0-9\- ]{6,20}$").When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
