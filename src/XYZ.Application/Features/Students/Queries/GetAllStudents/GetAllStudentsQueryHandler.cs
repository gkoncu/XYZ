using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Students.Queries.GetAllStudents
{
    public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, PaginationResult<StudentListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetAllStudentsQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<PaginationResult<StudentListItemDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
        {
            var query = _dataScope.Students();

            if (request.ClassId.HasValue)
                query = query.Where(s => s.ClassId == request.ClassId.Value);

            if (request.IsActive.HasValue)
            {
                var isActive = request.IsActive.Value;
                query = query.Where(s => s.IsActive == isActive && s.User.IsActive == isActive);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = $"%{request.SearchTerm.Trim()}%";
                query = query.Where(s =>
                    EF.Functions.Like(s.User.FirstName, term) ||
                    EF.Functions.Like(s.User.LastName, term) ||
                    (s.User.Email != null && EF.Functions.Like(s.User.Email, term)) ||
                    (s.IdentityNumber != null && EF.Functions.Like(s.IdentityNumber, term)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var asc = !string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = (request.SortBy ?? "FullName") switch
            {
                "Id" => asc ? query.OrderBy(s => s.Id) : query.OrderByDescending(s => s.Id),
                "FirstName" => asc ? query.OrderBy(s => s.User.FirstName).ThenBy(s => s.User.LastName)
                                    : query.OrderByDescending(s => s.User.FirstName).ThenByDescending(s => s.User.LastName),
                "LastName" => asc ? query.OrderBy(s => s.User.LastName).ThenBy(s => s.User.FirstName)
                                    : query.OrderByDescending(s => s.User.LastName).ThenByDescending(s => s.User.FirstName),
                "FullName" => asc ? query.OrderBy(s => s.User.FirstName).ThenBy(s => s.User.LastName)
                                    : query.OrderByDescending(s => s.User.FirstName).ThenByDescending(s => s.User.LastName),
                "Email" => asc ? query.OrderBy(s => s.User.Email)
                                    : query.OrderByDescending(s => s.User.Email),
                "ClassName" => asc ? query.OrderBy(s => s.Class!.Name)
                                    : query.OrderByDescending(s => s.Class!.Name),
                "BranchName" => asc ? query.OrderBy(s => s.Class!.Branch.Name)
                                    : query.OrderByDescending(s => s.Class!.Branch.Name),
                "CreatedAt" => asc ? query.OrderBy(s => s.CreatedAt)
                                    : query.OrderByDescending(s => s.CreatedAt),
                _ => asc ? query.OrderBy(s => s.User.FirstName).ThenBy(s => s.User.LastName)
                                    : query.OrderByDescending(s => s.User.FirstName).ThenByDescending(s => s.User.LastName),
            };

            var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var size = request.PageSize <= 0 ? 20 : request.PageSize;
            var skip = (page - 1) * size;

            var items = await query
                .Skip(skip)
                .Take(size)
                .Select(s => new StudentListItemDto
                {
                    Id = s.Id,
                    FullName = s.User.FirstName + " " + s.User.LastName,
                    Email = s.User.Email ?? string.Empty,
                    PhoneNumber = s.User.PhoneNumber,
                    ClassId = s.ClassId,
                    ClassName = s.Class != null ? s.Class.Name : null,
                    BranchName = s.Class != null ? s.Class.Branch.Name : null,
                    IsActive = s.IsActive && s.User.IsActive
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PaginationResult<StudentListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }
    }
}
