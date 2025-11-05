using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Common.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    }
}
