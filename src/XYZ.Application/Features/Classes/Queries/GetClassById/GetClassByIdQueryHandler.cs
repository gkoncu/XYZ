using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Classes.Queries.GetAllClasses;
using Microsoft.EntityFrameworkCore;

namespace XYZ.Application.Features.Classes.Queries.GetClassById
{
    public class GetClassByIdQueryHandler
        : IRequestHandler<GetClassByIdQuery, ClassDetailDto>
    {
        private readonly IDataScopeService _dataScope;

        public GetClassByIdQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<ClassDetailDto> Handle(
            GetClassByIdQuery request,
            CancellationToken ct)
        {
            var dto = await _dataScope.Classes()
                .Where(c => c.Id == request.ClassId)
                .Select(c => new ClassDetailDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    TenantId = c.TenantId,

                    BranchId = c.BranchId,
                    BranchName = c.Branch.Name,

                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,

                    StudentsCount = c.Students.Count,
                    CoachesCount = c.Coaches.Count,

                    Coaches = c.Coaches
                        .Select(co => new ClassCoachItemDto
                        {
                            Id = co.Id,
                            FullName = co.User.FirstName + " " + co.User.LastName
                        })
                        .ToList(),

                    Students = c.Students
                        .Select(s => new ClassStudentItemDto
                        {
                            Id = s.Id,
                            FullName = s.User.FirstName + " " + s.User.LastName,
                            IsActive = s.IsActive && s.User.IsActive
                        })
                        .ToList()
                })
                .AsNoTracking()
                .SingleOrDefaultAsync(ct);

            if (dto is null)
                throw new NotFoundException("Class", request.ClassId);

            return dto;
        }
    }
}
