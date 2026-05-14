using FluentValidation;
using CoreFlow.Application.Commands;

namespace CoreFlow.Application.Validators;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Email is invalid");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is required").Matches("^\\+?[0-9\\- ]{6,20}$").WithMessage("Phone is invalid");
    }
}
