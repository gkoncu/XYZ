using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Students.DTOs;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Students.Queries
{
    public class GetStudentByIdQuery : IRequest<StudentDto>
    {
        public int Id { get; set; }
    }

    public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataScopeService _dataScopeService;

        public GetStudentByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IDataScopeService dataScopeService)
        {
            _context = context;
            _mapper = mapper;
            _dataScopeService = dataScopeService;
        }

        public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
        {
            var student = await _dataScopeService.GetScopedStudents()
                .Include(s => s.User)
                .Include(s => s.Class)
                    .ThenInclude(c => c.Coaches)
                        .ThenInclude(co => co.User)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (student == null)
            {
                throw new NotFoundException(nameof(Student), request.Id);
            }

            return _mapper.Map<StudentDto>(student);
        }
    }
}
