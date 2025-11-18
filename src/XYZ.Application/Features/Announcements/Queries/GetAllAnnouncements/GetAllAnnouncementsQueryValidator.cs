using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements
{
    public class GetAllAnnouncementsQueryValidator
        : AbstractValidator<GetAllAnnouncementsQuery>
    {
        public GetAllAnnouncementsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(200);

            RuleFor(x => x.ClassId)
                .GreaterThan(0)
                .When(x => x.ClassId.HasValue);
        }
    }
}
