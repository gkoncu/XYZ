using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Application.Common.Specifications;
using XYZ.Application.Features.Students.DTOs;

namespace XYZ.Application.Features.Students.Queries
{
    public class GetStudentsQuery : IRequest<PaginationResult<StudentDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? ClassId { get; set; }
        public string? Branch { get; set; }
    }

    public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, PaginationResult<StudentDto>>
    {
        private readonly IMapper _mapper;
        private readonly IDataScopeService _dataScopeService;

        public GetStudentsQueryHandler(IMapper mapper, IDataScopeService dataScopeService)
        {
            _mapper = mapper;
            _dataScopeService = dataScopeService;
        }

        public async Task<PaginationResult<StudentDto>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
        {
            var specification = new StudentSearchSpecification(
                request.SearchTerm,
                request.ClassId,
                request.Branch);

            var (students, totalCount) = await _dataScopeService.GetPagedScopedStudentsAsync(
                specification,
                request.PageNumber,
                request.PageSize);

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
