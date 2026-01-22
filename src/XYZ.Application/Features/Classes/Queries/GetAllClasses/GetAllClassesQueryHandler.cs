using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Classes.Queries.GetAllClasses
{
    public class GetAllClassesQueryHandler
        : IRequestHandler<GetAllClassesQuery, PaginationResult<ClassListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetAllClassesQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<PaginationResult<ClassListItemDto>> Handle(
            GetAllClassesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dataScope.Classes();

            if (request.BranchId.HasValue)
                query = query.Where(c => c.BranchId == request.BranchId.Value);

            if (request.IsActive.HasValue)
            {
                var isActive = request.IsActive.Value;
                query = query.Where(c => c.IsActive == isActive);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = $"%{request.SearchTerm.Trim()}%";
                query = query.Where(c =>
                    EF.Functions.Like(c.Name, term) ||
                    (c.Branch.Name != null && EF.Functions.Like(c.Branch.Name, term)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var asc = !string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = (request.SortBy ?? "Name") switch
            {
                "Id" => asc ? query.OrderBy(c => c.Id)
                                       : query.OrderByDescending(c => c.Id),

                "Name" => asc ? query.OrderBy(c => c.Name)
                                       : query.OrderByDescending(c => c.Name),

                "BranchName" => asc ? query.OrderBy(c => c.Branch.Name)
                                       : query.OrderByDescending(c => c.Branch.Name),

                "StudentsCount" => asc ? query.OrderBy(c => c.Students.Count)
                                       : query.OrderByDescending(c => c.Students.Count),

                "CoachesCount" => asc ? query.OrderBy(c => c.Coaches.Count)
                                       : query.OrderByDescending(c => c.Coaches.Count),

                "CreatedAt" => asc ? query.OrderBy(c => c.CreatedAt)
                                       : query.OrderByDescending(c => c.CreatedAt),

                _ => asc ? query.OrderBy(c => c.Name)
                                       : query.OrderByDescending(c => c.Name),
            };

            var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var size = request.PageSize <= 0 ? 20 : request.PageSize;
            var skip = (page - 1) * size;

            var items = await query
                .Skip(skip)
                .Take(size)
                .Select(c => new ClassListItemDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    BranchName = c.Branch.Name,
                    TenantName = c.Tenant.Name,
                    StudentsCount = c.Students.Count,
                    CoachesCount = c.Coaches.Count,
                    IsActive = c.IsActive
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PaginationResult<ClassListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }
    }
}
