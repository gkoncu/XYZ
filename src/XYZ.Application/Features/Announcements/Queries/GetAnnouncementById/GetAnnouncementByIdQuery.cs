using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Announcements.Queries.GetAnnouncementById
{
    public class GetAnnouncementByIdQuery : IRequest<AnnouncementDetailDto>
    {
        public int Id { get; set; }
    }
}
