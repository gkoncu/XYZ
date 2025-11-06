using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Students.DTOs;
using XYZ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace XYZ.Application.Features.Students.Queries.GetStudents
{
    public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, StudentListResponse>
    {
        private readonly IDataScopeService _dataScopeService;
        private readonly ILogger<GetStudentsQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetStudentsQueryHandler(
            IDataScopeService dataScopeService,
            ILogger<GetStudentsQueryHandler> logger,
            IMapper mapper)
        {
            _dataScopeService = dataScopeService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<StudentListResponse> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Getting students list with filters: IsActive={IsActive}, ClassId={ClassId}, Branch={Branch}, " +
                    "SearchTerm={SearchTerm}, Page={Page}, Size={Size}",
                    request.IsActive, request.ClassId, request.Branch, request.SearchTerm,
                    request.PageNumber, request.PageSize);

                var query = _dataScopeService.GetScopedStudents();

                if (request.IsActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == request.IsActive.Value);
                }

                if (request.ClassId.HasValue)
                {
                    query = query.Where(s => s.ClassId == request.ClassId.Value);
                }

                if (!string.IsNullOrEmpty(request.Branch))
                {
                    query = query.Where(s => s.User.Branch == request.Branch);
                }

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.Trim().ToLower();
                    query = query.Where(s =>
                        s.User.FirstName.ToLower().Contains(searchTerm) ||
                        s.User.LastName.ToLower().Contains(searchTerm) ||
                        s.User.FullName.ToLower().Contains(searchTerm) ||
                        s.User.Email.ToLower().Contains(searchTerm) ||
                        s.User.PhoneNumber.Contains(searchTerm) ||
                        (s.Parent1FirstName != null && s.Parent1FirstName.ToLower().Contains(searchTerm)) ||
                        (s.Parent1LastName != null && s.Parent1LastName.ToLower().Contains(searchTerm)) ||
                        (s.Parent2FirstName != null && s.Parent2FirstName.ToLower().Contains(searchTerm)) ||
                        (s.Parent2LastName != null && s.Parent2LastName.ToLower().Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                query = ApplySorting(query, request.SortBy, request.SortDescending);

                var students = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} students out of {TotalCount}", students.Count, totalCount);

                var studentDtos = _mapper.Map<List<StudentListDto>>(students);

                return new StudentListResponse
                {
                    Students = studentDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting students list");
                throw;
            }
        }

        private static IQueryable<Student> ApplySorting(IQueryable<Student> query, string sortBy, bool sortDescending)
        {
            return (sortBy?.ToLower(), sortDescending) switch
            {
                ("firstname", false) => query.OrderBy(s => s.User.FirstName),
                ("firstname", true) => query.OrderByDescending(s => s.User.FirstName),
                ("email", false) => query.OrderBy(s => s.User.Email),
                ("email", true) => query.OrderByDescending(s => s.User.Email),
                ("createdat", false) => query.OrderBy(s => s.CreatedAt),
                ("createdat", true) => query.OrderByDescending(s => s.CreatedAt),
                ("birthdate", false) => query.OrderBy(s => s.User.BirthDate),
                ("birthdate", true) => query.OrderByDescending(s => s.User.BirthDate),
                ("classname", false) => query.OrderBy(s => s.Class.Name),
                ("classname", true) => query.OrderByDescending(s => s.Class.Name),
                (_, false) => query.OrderBy(s => s.User.LastName).ThenBy(s => s.User.FirstName),
                (_, true) => query.OrderByDescending(s => s.User.LastName).ThenByDescending(s => s.User.FirstName)
            };
        }
    }
}
