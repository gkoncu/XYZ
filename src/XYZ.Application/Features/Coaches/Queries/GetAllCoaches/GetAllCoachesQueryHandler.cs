using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Coaches.Queries.GetAllCoaches
{
    public class GetAllCoachesQueryHandler
        : IRequestHandler<GetAllCoachesQuery, PaginationResult<CoachListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetAllCoachesQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<PaginationResult<CoachListItemDto>> Handle(
            GetAllCoachesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dataScope.Coaches();

            if (request.IsActive.HasValue)
            {
                var isActive = request.IsActive.Value;
                query = query.Where(c => c.IsActive == isActive && c.User.IsActive == isActive);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = $"%{request.SearchTerm.Trim()}%";

                query = query.Where(c =>
                    EF.Functions.Like(c.User.FirstName, term) ||
                    EF.Functions.Like(c.User.LastName, term) ||
                    (c.User.Email != null && EF.Functions.Like(c.User.Email, term)) ||
                    (c.IdentityNumber != null && EF.Functions.Like(c.IdentityNumber, term)) ||
                    (c.LicenseNumber != null && EF.Functions.Like(c.LicenseNumber, term)) ||
                    (c.Branch != null && EF.Functions.Like(c.Branch, term)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var asc = !string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = (request.SortBy ?? "FullName") switch
            {
                "Id" => asc ? query.OrderBy(c => c.Id)
                                    : query.OrderByDescending(c => c.Id),

                "FirstName" => asc ? query.OrderBy(c => c.User.FirstName)
                                        .ThenBy(c => c.User.LastName)
                                    : query.OrderByDescending(c => c.User.FirstName)
                                        .ThenByDescending(c => c.User.LastName),

                "LastName" => asc ? query.OrderBy(c => c.User.LastName)
                                        .ThenBy(c => c.User.FirstName)
                                    : query.OrderByDescending(c => c.User.LastName)
                                        .ThenByDescending(c => c.User.FirstName),

                "FullName" => asc ? query.OrderBy(c => c.User.FirstName)
                                        .ThenBy(c => c.User.LastName)
                                    : query.OrderByDescending(c => c.User.FirstName)
                                        .ThenByDescending(c => c.User.LastName),

                "Email" => asc ? query.OrderBy(c => c.User.Email)
                                    : query.OrderByDescending(c => c.User.Email),

                "BranchName" => asc ? query.OrderBy(c => c.Branch)
                                    : query.OrderByDescending(c => c.Branch),

                "ClassesCount" => asc ? query.OrderBy(c => c.Classes.Count)
                                      : query.OrderByDescending(c => c.Classes.Count),

                "CreatedAt" => asc ? query.OrderBy(c => c.CreatedAt)
                                    : query.OrderByDescending(c => c.CreatedAt),

                _ => asc ? query.OrderBy(c => c.User.FirstName)
                                        .ThenBy(c => c.User.LastName)
                                    : query.OrderByDescending(c => c.User.FirstName)
                                        .ThenByDescending(c => c.User.LastName),
            };

            var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var size = request.PageSize <= 0 ? 20 : request.PageSize;
            var skip = (page - 1) * size;

            var items = await query
                .Skip(skip)
                .Take(size)
                .Select(c => new CoachListItemDto
                {
                    Id = c.Id,
                    FullName = c.User.FirstName + " " + c.User.LastName,
                    Email = c.User.Email ?? string.Empty,
                    PhoneNumber = c.User.PhoneNumber,
                    BranchName = c.Branch ?? string.Empty,
                    ClassesCount = c.Classes.Count,
                    IsActive = c.IsActive && c.User.IsActive
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PaginationResult<CoachListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }
    }
}
