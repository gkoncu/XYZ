using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Attendances.Queries.GetSessionAttendance
{
    public class GetSessionAttendanceQuery : IRequest<SessionAttendanceDto>
    {
        public int ClassSessionId { get; set; }
    }
}
