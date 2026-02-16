using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Queries.GetMyTodaySessions
{
    public sealed class GetMyTodaySessionsQuery
        : IRequest<IList<MyTodaySessionListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ClassSessions.Read;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public DateOnly? Date { get; set; }
    }
}
