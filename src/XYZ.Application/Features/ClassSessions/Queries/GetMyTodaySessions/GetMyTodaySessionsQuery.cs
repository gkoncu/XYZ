using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ClassSessions.Queries.GetMyTodaySessions
{
    public sealed class GetMyTodaySessionsQuery
        : IRequest<IList<MyTodaySessionListItemDto>>
    {
        public DateOnly? Date { get; set; }
    }
}
