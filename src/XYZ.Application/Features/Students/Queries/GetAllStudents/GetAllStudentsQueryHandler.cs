using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace XYZ.Application.Features.Students.Queries.GetAllStudents
{
    public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, List<StudentListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetAllStudentsQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<List<StudentListItemDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
        {
            var query = _dataScope.Students();

            if (request.ClassId.HasValue)
                query = query.Where(s => s.ClassId == request.ClassId.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = "%" + request.SearchTerm.Trim() + "%";
                query = query.Where(s =>
                    EF.Functions.Like(s.User.FullName, term) ||
                    (s.User.Email != null && EF.Functions.Like(s.User.Email, term)) ||
                    (s.IdentityNumber != null && EF.Functions.Like(s.IdentityNumber, term)));
            }

            var list = await query
                .OrderBy(s => s.User.FirstName)
                .ThenBy(s => s.User.LastName)
                .Select(s => new StudentListItemDto
                {
                    Id = s.Id,
                    FullName = s.User.FullName,
                    Email = s.User.Email ?? string.Empty,
                    PhoneNumber = s.User.PhoneNumber,
                    ClassId = s.ClassId,
                    ClassName = s.Class != null ? s.Class.Name : null,
                    BranchName = s.Class != null ? s.Class.Branch.Name : null,
                    IsActive = s.User.IsActive
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return list;
        }
    }
}
