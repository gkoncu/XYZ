using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }

    public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScopeService;

        public DeleteStudentCommandHandler(IApplicationDbContext context, IDataScopeService dataScopeService)
        {
            _context = context;
            _dataScopeService = dataScopeService;
        }

        public async Task<Unit> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
        {
            var student = await _dataScopeService.GetScopedStudents()
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (student == null)
            {
                throw new NotFoundException(nameof(Student), request.Id);
            }

            student.IsDeleted = true;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
