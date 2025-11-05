using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Students.DTOs;

namespace XYZ.Application.Features.Students.Queries.GetStudents
{
    public class GetStudentsQuery : IRequest<PaginationResult<StudentDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? ClassId { get; set; }
        public int? CoachId { get; set; }
    }

    public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, PaginationResult<StudentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataScopeService _dataScopeService;

        public GetStudentsQueryHandler(IApplicationDbContext context, IMapper mapper, IDataScopeService dataScopeService)
        {
            _context = context;
            _mapper = mapper;
            _dataScopeService = dataScopeService;
        }

        public async Task<PaginationResult<StudentDto>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
        {
            var baseQuery = _dataScopeService.GetScopedStudents();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                baseQuery = baseQuery.Where(s =>
                    s.FirstName.Contains(request.SearchTerm) ||
                    s.LastName.Contains(request.SearchTerm) ||
                    s.Email.Contains(request.SearchTerm));
            }

            if (request.ClassId.HasValue)
            {
                baseQuery = baseQuery.Where(s => s.ClassId == request.ClassId);
            }

            if (request.CoachId.HasValue)
            {
                baseQuery = baseQuery.Where(s => s.CoachId == request.CoachId);
            }

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            var students = await baseQuery
                .Include(s => s.Class)
                .Include(s => s.Coach)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var studentDtos = _mapper.Map<List<StudentDto>>(students);

            return new PaginationResult<StudentDto>
            {
                Items = studentDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
