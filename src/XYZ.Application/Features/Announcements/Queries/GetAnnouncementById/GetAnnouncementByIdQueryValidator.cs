using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Announcements.Queries.GetAnnouncementById
{
    public class GetAnnouncementByIdQueryValidator
        : AbstractValidator<GetAnnouncementByIdQuery>
    {
        public GetAnnouncementByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
