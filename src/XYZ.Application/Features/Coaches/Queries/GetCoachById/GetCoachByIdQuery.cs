using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;

namespace XYZ.Application.Features.Coaches.Queries.GetCoachById
{
    public class GetCoachByIdQuery : IRequest<CoachDetailDto>
    {
        public int CoachId { get; set; }
    }
}
