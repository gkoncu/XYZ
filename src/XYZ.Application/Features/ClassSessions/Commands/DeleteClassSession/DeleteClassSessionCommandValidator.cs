using FluentValidation;

namespace XYZ.Application.Features.ClassSessions.Commands.DeleteClassSession
{
    public class DeleteClassSessionCommandValidator
        : AbstractValidator<DeleteClassSessionCommand>
    {
        public DeleteClassSessionCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);
        }
    }
}
