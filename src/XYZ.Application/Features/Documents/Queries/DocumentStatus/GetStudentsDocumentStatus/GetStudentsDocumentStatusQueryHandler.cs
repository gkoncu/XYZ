using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetStudentsDocumentStatus
{
    public class GetStudentsDocumentStatusQueryHandler
        : IRequestHandler<GetStudentsDocumentStatusQuery, IList<StudentDocumentStatusListItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public GetStudentsDocumentStatusQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService current)
        {
            _context = context;
            _dataScope = dataScope;
            _current = current;
        }

        public async Task<IList<StudentDocumentStatusListItemDto>> Handle(GetStudentsDocumentStatusQuery request, CancellationToken ct)
        {
            var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var requiredIds = await _context.DocumentDefinitions
                .Where(d => d.TenantId == tenantId
                            && d.Target == DocumentTarget.Student
                            && d.IsActive
                            && d.IsRequired)
                .Select(d => d.Id)
                .ToListAsync(ct);

            if (requiredIds.Count == 0)
            {
                IQueryable<Student> baseQ = _dataScope.Students();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var st = request.SearchTerm.Trim();
                    baseQ = baseQ.Where(s => s.User.FirstName.Contains(st) || s.User.LastName.Contains(st));
                }

                return await baseQ
                    .Include(s => s.User)
                    .OrderBy(s => s.User.FirstName)
                    .ThenBy(s => s.User.LastName)
                    .Take(Math.Clamp(request.Take, 1, 1000))
                    .Select(s => new StudentDocumentStatusListItemDto
                    {
                        StudentId = s.Id,
                        FullName = s.User.FullName,
                        IsComplete = true,
                        MissingCount = 0
                    })
                    .ToListAsync(ct);
            }

            var studentsQ = _dataScope.Students()
                .Include(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var st = request.SearchTerm.Trim();
                studentsQ = studentsQ.Where(s => s.User.FullName.Contains(st));
            }

            var students = await studentsQ
                .OrderBy(s => s.User.FullName)
                .Take(Math.Clamp(request.Take, 1, 1000))
                .Select(s => new { s.Id, s.User.FullName })
                .ToListAsync(ct);

            var studentIds = students.Select(x => x.Id).ToList();

            var uploadedPairs = await _dataScope.Documents()
                .Where(d => d.IsActive && d.StudentId != null && studentIds.Contains(d.StudentId.Value))
                .Select(d => new { StudentId = d.StudentId!.Value, d.DocumentDefinitionId })
                .Distinct()
                .ToListAsync(ct);

            var uploadedMap = uploadedPairs
                .GroupBy(x => x.StudentId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.DocumentDefinitionId).ToHashSet());

            var result = new List<StudentDocumentStatusListItemDto>(students.Count);

            foreach (var s in students)
            {
                uploadedMap.TryGetValue(s.Id, out var uploadedSet);
                uploadedSet ??= new HashSet<int>();

                var missingCount = requiredIds.Count(id => !uploadedSet.Contains(id));
                var isComplete = missingCount == 0;

                if (request.OnlyIncomplete && isComplete)
                    continue;

                result.Add(new StudentDocumentStatusListItemDto
                {
                    StudentId = s.Id,
                    FullName = s.FullName,
                    IsComplete = isComplete,
                    MissingCount = missingCount
                });
            }

            return result;
        }
    }
}
