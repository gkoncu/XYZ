using MediatR;

namespace XYZ.Application.Features.Tenants.Commands.UpdateCurrentTenantTheme;

public sealed class UpdateCurrentTenantThemeCommand : IRequest
{
    public string PrimaryColor { get; set; } = "#3B82F6";
    public string SecondaryColor { get; set; } = "#1E40AF";
    public string? LogoUrl { get; set; }
}
