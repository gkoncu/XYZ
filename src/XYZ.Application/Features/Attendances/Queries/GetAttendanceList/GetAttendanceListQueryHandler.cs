using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceList
{
    public class GetAttendanceListQueryHandler(IDataScopeService dataScope)
        : IRequestHandler<GetAttendanceListQuery, PaginationResult<AttendanceListItemDto>>
    {
        private readonly IDataScopeService _dataScope = dataScope;

        public async Task<PaginationResult<AttendanceListItemDto>> Handle(
            GetAttendanceListQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dataScope
                .Attendances()
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Class)
                .Include(a => a.ClassSession)
                .AsQueryable();


            if (request.StudentId.HasValue)
            {
                query = query.Where(a => a.StudentId == request.StudentId.Value);
            }

            if (request.ClassId.HasValue)
            {
                query = query.Where(a => a.ClassId == request.ClassId.Value);
            }

            if (request.ClassSessionId.HasValue)
            {
                query = query.Where(a => a.ClassSessionId == request.ClassSessionId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(a => a.Status == request.Status.Value);
            }

            if (request.From.HasValue)
            {
                query = query.Where(a => a.ClassSession.Date >= request.From.Value);
            }

            if (request.To.HasValue)
            {
                query = query.Where(a => a.ClassSession.Date <= request.To.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var size = request.PageSize <= 0 ? 50 : request.PageSize;

            query = query
                .OrderByDescending(a => a.ClassSession.Date)
                .ThenBy(a => a.Class.Name)
                .ThenBy(a => a.Student.User.FullName);

            var items = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(a => new AttendanceListItemDto
                {
                    Id = a.Id,
                    ClassSessionId = a.ClassSessionId,
                    SessionDate = a.ClassSession.Date,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    StudentId = a.StudentId,
                    StudentFullName = a.Student.User.FullName,
                    Status = a.Status,
                    Score = a.Score,
                    Note = a.Note,
                    CoachComment = a.CoachComment
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PaginationResult<AttendanceListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }
    }
}
