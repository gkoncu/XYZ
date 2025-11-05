using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<(IReadOnlyList<Student> Items, int TotalCount)> GetPagedStudentsWithDetailsAsync(
            ISpecification<Student> specification,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<Student?> GetStudentWithDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
