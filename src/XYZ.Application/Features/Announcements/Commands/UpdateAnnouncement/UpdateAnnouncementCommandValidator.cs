using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Announcements.Commands.UpdateAnnouncement
{
    public class UpdateAnnouncementCommandValidator
        : AbstractValidator<UpdateAnnouncementCommand>
    {
        public UpdateAnnouncementCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Content)
                .NotEmpty()
                .MaximumLength(4000);

            RuleFor(x => x.ClassId)
                .GreaterThan(0)
                .When(x => x.ClassId.HasValue);

            RuleFor(x => x.ExpiryDate)
                .GreaterThanOrEqualTo(x => x.PublishDate)
                .When(x => x.ExpiryDate.HasValue);
        }
    }
}
