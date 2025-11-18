using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Announcements.Commands.DeleteAnnouncement
{
    public class DeleteAnnouncementCommandValidator
        : AbstractValidator<DeleteAnnouncementCommand>
    {
        public DeleteAnnouncementCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
