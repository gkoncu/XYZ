using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Queries.GetClassSessions
{
    public class GetClassSessionsQueryHandler
        : IRequestHandler<GetClassSessionsQuery, PaginationResult<ClassSessionListItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetClassSessionsQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<PaginationResult<ClassSessionListItemDto>> Handle(
            GetClassSessionsQuery request,
            CancellationToken ct)
        {
            var classesQuery = _dataScope.Classes();

            var query =
                from cs in _context.ClassSessions
                join cls in classesQuery on cs.ClassId equals cls.Id
                select new { cs, cls };

            var onlyActive = request.OnlyActive ?? true;
            if (onlyActive)
            {
                query = query.Where(x => x.cs.IsActive);
            }

            if (request.ClassId.HasValue)
            {
                query = query.Where(x => x.cs.ClassId == request.ClassId.Value);
            }

            if (request.BranchId.HasValue)
            {
                query = query.Where(x => x.cls.BranchId == request.BranchId.Value);
            }

            if (request.Status.HasValue)
            {
                var status = request.Status.Value;
                query = query.Where(x => x.cs.Status == status);
            }

            if (request.From.HasValue)
            {
                var from = request.From.Value;
                query = query.Where(x => x.cs.Date >= from);
            }

            if (request.To.HasValue)
            {
                var to = request.To.Value;
                query = query.Where(x => x.cs.Date <= to);
            }

            var totalCount = await query.CountAsync(ct);

            var asc = !string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
            var sortBy = request.SortBy ?? "Date";

            query = sortBy switch
            {
                "Id" => asc
                    ? query.OrderBy(x => x.cs.Id)
                    : query.OrderByDescending(x => x.cs.Id),

                "ClassName" => asc
                    ? query.OrderBy(x => x.cls.Name)
                    : query.OrderByDescending(x => x.cls.Name),

                "StartTime" => asc
                    ? query.OrderBy(x => x.cs.StartTime)
                    : query.OrderByDescending(x => x.cs.StartTime),

                "EndTime" => asc
                    ? query.OrderBy(x => x.cs.EndTime)
                    : query.OrderByDescending(x => x.cs.EndTime),

                "CreatedAt" => asc
                    ? query.OrderBy(x => x.cs.CreatedAt)
                    : query.OrderByDescending(x => x.cs.CreatedAt),

                _ => asc
                    ? query.OrderBy(x => x.cs.Date).ThenBy(x => x.cs.StartTime)
                    : query.OrderByDescending(x => x.cs.Date).ThenByDescending(x => x.cs.StartTime),
            };

            var page = request.PageNumber;
            var size = request.PageSize;

            var items = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new ClassSessionListItemDto
                {
                    Id = x.cs.Id,
                    ClassId = x.cs.ClassId,
                    ClassName = x.cls.Name,
                    BranchId = x.cls.BranchId,
                    BranchName = x.cls.Branch != null
                        ? x.cls.Branch.Name
                        : string.Empty,
                    Date = x.cs.Date,
                    StartTime = x.cs.StartTime,
                    EndTime = x.cs.EndTime,
                    Title = x.cs.Title,
                    Status = x.cs.Status,
                    IsActive = x.cs.IsActive,
                    HasAttendance = x.cs.Attendances.Any(),
                    TotalStudents = x.cs.Attendances.Count,
                    PresentCount = x.cs.Attendances.Count(a =>
                        a.Status == AttendanceStatus.Present),
                    AbsentCount = x.cs.Attendances.Count(a =>
                        a.Status == AttendanceStatus.Absent ||
                        a.Status == AttendanceStatus.Excused)
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return new PaginationResult<ClassSessionListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }
    }
}
