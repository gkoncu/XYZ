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
            var query = _dataScope.GetScopedStudents()
                .Include(s => s.User)
                .Include(s => s.Class)
                .ThenInclude(c => c.Branch)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(s =>
                    (s.User.FirstName + " " + s.User.LastName).ToLower().Contains(term) ||
                    (s.User.Email ?? "").ToLower().Contains(term) ||
                    (s.IdentityNumber ?? "").Contains(term));
            }

            if (request.ClassId.HasValue)
                query = query.Where(s => s.ClassId == request.ClassId.Value);

            var students = await query
                .OrderBy(s => s.User.FirstName)
                .ThenBy(s => s.User.LastName)
                .ToListAsync(cancellationToken);

            return students.Select(s => new StudentListItemDto
            {
                Id = s.Id,
                FullName = $"{s.User.FirstName} {s.User.LastName}",
                Email = s.User.Email ?? string.Empty,
                PhoneNumber = s.User.PhoneNumber,
                ClassName = s.Class?.Name,
                Branch = s.User.Branch,
                IsActive = s.User.IsActive
            }).ToList();
        }
    }
}
