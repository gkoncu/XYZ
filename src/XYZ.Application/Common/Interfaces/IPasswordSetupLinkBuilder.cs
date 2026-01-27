namespace XYZ.Application.Common.Interfaces;

public interface IPasswordSetupLinkBuilder
{
    string? Build(string userId, string token);
}
