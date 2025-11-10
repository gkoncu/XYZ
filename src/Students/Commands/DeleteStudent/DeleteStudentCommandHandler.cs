using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScopeService;
        private readonly ILogger<DeleteStudentCommandHandler> _logger;

        public DeleteStudentCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScopeService,
            ILogger<DeleteStudentCommandHandler> logger)
        {
            _context = context;
            _dataScopeService = dataScopeService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Soft deleting student with ID: {StudentId}", request.Id);

                var student = await GetStudentWithAuthorization(request.Id);

                student.IsActive = false;
                student.UpdatedAt = DateTime.UtcNow;

                _context.Students.Update(student);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully soft deleted student with ID: {StudentId}", request.Id);

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while soft deleting student with ID: {StudentId}", request.Id);
                throw;
            }
        }

        private async Task<Student> GetStudentWithAuthorization(int studentId)
        {
            var canAccess = await _dataScopeService.CanAccessStudentAsync(studentId);
            if (!canAccess)
            {
                throw new UnauthorizedAccessException($"You don't have permission to delete student with ID {studentId}.");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId && s.IsActive);

            if (student == null)
            {
                throw new ArgumentException($"Active student with ID {studentId} not found.");
            }

            return student;
        }
    }
}
