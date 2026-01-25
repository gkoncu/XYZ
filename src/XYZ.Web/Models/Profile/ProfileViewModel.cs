using XYZ.Application.Features.Profile.Queries.GetMyProfile;

namespace XYZ.Web.Models.Profile
{
    public sealed class ProfileViewModel
    {
        public MyProfileDto Profile { get; set; } = new();
        public string Initials { get; set; } = "?";

        public string FullName =>
            $"{Profile.FirstName} {Profile.LastName}".Trim();
    }
}
