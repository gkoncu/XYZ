using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Students.DTOs;

namespace XYZ.Application.Features.Students.Queries.GetStudentsById
{
    public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDetailDto>
    {
        private readonly IDataScopeService _dataScopeService;
        private readonly ILogger<GetStudentByIdQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetStudentByIdQueryHandler(
            IDataScopeService dataScopeService,
            ILogger<GetStudentByIdQueryHandler> logger,
            IMapper mapper)
        {
            _dataScopeService = dataScopeService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<StudentDetailDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting student by ID: {StudentId}", request.Id);

                var query = _dataScopeService.GetScopedStudents();

                var student = await query
                    .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

                if (student == null)
                {
                    _logger.LogWarning("Student with ID {StudentId} not found or access denied", request.Id);
                    throw new Exception($"Student with ID {request.Id} not found or you don't have access");
                }

                _logger.LogInformation("Successfully retrieved student with ID {StudentId}", request.Id);

                return _mapper.Map<StudentDetailDto>(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting student by ID: {StudentId}", request.Id);
                throw;
            }
        }
    }
}
