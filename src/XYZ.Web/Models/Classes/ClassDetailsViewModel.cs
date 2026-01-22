using Microsoft.AspNetCore.Mvc.Rendering;
using XYZ.Application.Features.Classes.Queries.GetAllClasses;

namespace XYZ.Web.Models.Classes
{
    public sealed class ClassDetailsViewModel
    {
        public ClassDetailDto Class { get; init; } = default!;
        public List<SelectListItem> AvailableStudents { get; init; } = new();
        public List<SelectListItem> AvailableCoaches { get; init; } = new();
    }
}
