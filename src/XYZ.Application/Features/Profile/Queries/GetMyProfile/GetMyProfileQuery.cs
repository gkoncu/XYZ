using MediatR;

namespace XYZ.Application.Features.Profile.Queries.GetMyProfile;

public sealed record GetMyProfileQuery : IRequest<MyProfileDto>;
