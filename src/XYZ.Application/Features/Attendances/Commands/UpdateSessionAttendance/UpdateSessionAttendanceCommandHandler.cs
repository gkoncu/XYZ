using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Attendances.Commands.UpdateSessionAttendance
{
    public class UpdateSessionAttendanceCommandHandler
        : IRequestHandler<UpdateSessionAttendanceCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;

        public UpdateSessionAttendanceCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context)
        {
            _dataScope = dataScope;
            _context = context;
        }

        public async Task<int> Handle(
            UpdateSessionAttendanceCommand request,
            CancellationToken ct)
        {
            if (request.Items == null || request.Items.Count == 0)
                return request.SessionId;

            var attendanceIds = request.Items
                .Select(i => i.AttendanceId)
                .Distinct()
                .ToList();

            if (attendanceIds.Count == 0)
                return request.SessionId;

            var attendances = await _dataScope.Attendances()
                .Where(a =>
                    a.ClassSessionId == request.SessionId &&
                    attendanceIds.Contains(a.Id))
                .ToListAsync(ct);

            if (attendances.Count == 0)
                throw new NotFoundException("Attendance", request.SessionId);

            var attendanceById = attendances.ToDictionary(a => a.Id);

            foreach (var item in request.Items)
            {
                if (!attendanceById.TryGetValue(item.AttendanceId, out var entity))
                    continue;

                entity.Status = item.Status;
                entity.Note = item.Note;
                entity.Score = item.Score;
                entity.CoachComment = item.CoachComment;
                entity.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);

            return request.SessionId;
        }
    }
}
