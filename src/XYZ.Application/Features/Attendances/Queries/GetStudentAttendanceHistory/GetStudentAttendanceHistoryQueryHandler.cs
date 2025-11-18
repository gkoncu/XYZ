using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory
{
    public class GetStudentAttendanceHistoryQueryHandler
        : IRequestHandler<GetStudentAttendanceHistoryQuery, IList<StudentAttendanceHistoryItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetStudentAttendanceHistoryQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<IList<StudentAttendanceHistoryItemDto>> Handle(
            GetStudentAttendanceHistoryQuery request,
            CancellationToken ct)
        {
            await _dataScope.EnsureStudentAccessAsync(request.StudentId, ct);

            var list = await _dataScope.Attendances()
                .Where(a => a.StudentId == request.StudentId)
                .OrderByDescending(a => a.ClassSession.Date)
                .Select(a => new StudentAttendanceHistoryItemDto
                {
                    AttendanceId = a.Id,
                    ClassSessionId = a.ClassSessionId,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    Date = a.ClassSession.Date,
                    StartTime = a.ClassSession.StartTime,
                    EndTime = a.ClassSession.EndTime,
                    Title = a.ClassSession.Title,
                    Status = a.Status,
                    Note = a.Note,
                    Score = a.Score,
                    CoachComment = a.CoachComment
                })
                .ToListAsync(ct);

            return list;
        }
    }
}
