using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ClassSessions.Commands.UpdateClassSession
{
    public class UpdateClassSessionCommandHandler
        : IRequestHandler<UpdateClassSessionCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public UpdateClassSessionCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(
            UpdateClassSessionCommand request,
            CancellationToken ct)
        {
            var classesQuery = _dataScope.Classes();

            var query =
                from cs in _context.ClassSessions
                join cls in classesQuery on cs.ClassId equals cls.Id
                where cs.Id == request.SessionId && cs.IsActive
                select cs;

            var session = await query.SingleOrDefaultAsync(ct);

            if (session is null)
                throw new NotFoundException("ClassSession", request.SessionId);

            session.Date = request.Date;
            session.StartTime = request.StartTime;
            session.EndTime = request.EndTime;
            session.Title = request.Title;
            session.Description = request.Description;
            session.Location = request.Location;
            session.CoachNote = request.CoachNote;
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return session.Id;
        }
    }
}
