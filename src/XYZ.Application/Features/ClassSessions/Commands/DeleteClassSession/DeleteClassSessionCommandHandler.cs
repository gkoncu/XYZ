using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Commands.DeleteClassSession
{
    public class DeleteClassSessionCommandHandler
        : IRequestHandler<DeleteClassSessionCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public DeleteClassSessionCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(
            DeleteClassSessionCommand request,
            CancellationToken ct)
        {
            var classesQuery = _dataScope.Classes();

            var query =
                from cs in _context.ClassSessions
                join cls in classesQuery on cs.ClassId equals cls.Id
                where cs.Id == request.Id && cs.IsActive
                select cs;

            var session = await query.SingleOrDefaultAsync(ct);

            if (session is null)
                throw new NotFoundException("ClassSession", request.Id);

            session.IsActive = false;
            session.Status = SessionStatus.Cancelled;
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return session.Id;
        }
    }
}
