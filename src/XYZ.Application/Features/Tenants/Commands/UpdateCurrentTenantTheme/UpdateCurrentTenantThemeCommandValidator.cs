using FluentValidation;
using System.Text.RegularExpressions;

namespace XYZ.Application.Features.Tenants.Commands.UpdateCurrentTenantTheme;

public sealed class UpdateCurrentTenantThemeCommandValidator : AbstractValidator<UpdateCurrentTenantThemeCommand>
{
    private static readonly Regex HexColor = new("^#([0-9a-fA-F]{6})$");

    public UpdateCurrentTenantThemeCommandValidator()
    {
        RuleFor(x => x.PrimaryColor)
            .NotEmpty()
            .Must(v => HexColor.IsMatch(v))
            .WithMessage("PrimaryColor '#RRGGBB' formatında olmalı.");

        RuleFor(x => x.SecondaryColor)
            .NotEmpty()
            .Must(v => HexColor.IsMatch(v))
            .WithMessage("SecondaryColor '#RRGGBB' formatında olmalı.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500);
    }
}
