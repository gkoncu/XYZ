using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Announcements.Commands.DeleteAnnouncement
{
    public class DeleteAnnouncementCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}
