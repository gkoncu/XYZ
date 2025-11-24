using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Queries.GetClassSessionById
{
    public class GetClassSessionByIdQueryHandler
        : IRequestHandler<GetClassSessionByIdQuery, ClassSessionDetailDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetClassSessionByIdQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<ClassSessionDetailDto> Handle(
            GetClassSessionByIdQuery request,
            CancellationToken ct)
        {
            var classesQuery = _dataScope.Classes();

            var query =
                from cs in _context.ClassSessions
                join cls in classesQuery on cs.ClassId equals cls.Id
                where cs.Id == request.Id && cs.IsActive
                select new { cs, cls };

            var dto = await query
                .Select(x => new ClassSessionDetailDto
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
                    Description = x.cs.Description,
                    Location = x.cs.Location,
                    Status = x.cs.Status,
                    CoachNote = x.cs.CoachNote,
                    TotalStudents = x.cs.Attendances.Count,
                    PresentCount = x.cs.Attendances.Count(a =>
                        a.Status == AttendanceStatus.Present),
                    AbsentCount = x.cs.Attendances.Count(a =>
                        a.Status == AttendanceStatus.Absent ||
                        a.Status == AttendanceStatus.Excused)
                })
                .AsNoTracking()
                .SingleOrDefaultAsync(ct);

            if (dto is null)
                throw new NotFoundException("ClassSession", request.Id);

            return dto;
        }
    }
}
