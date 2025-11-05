using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Infrastructure.Data.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Students.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Student?> FirstOrDefaultAsync(ISpecification<Student> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Student>> ListAsync(ISpecification<Student> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Student>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Students.ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(ISpecification<Student> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).CountAsync(cancellationToken);
        }

        public async Task<Student> AddAsync(Student entity, CancellationToken cancellationToken = default)
        {
            _context.Students.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task UpdateAsync(Student entity, CancellationToken cancellationToken = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Student entity, CancellationToken cancellationToken = default)
        {
            _context.Students.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<Student> Items, int TotalCount)> GetPagedStudentsWithDetailsAsync(
            ISpecification<Student> specification,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(specification);
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Student?> GetStudentWithDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                    .ThenInclude(c => c.Coaches)
                        .ThenInclude(co => co.User)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        private IQueryable<Student> ApplySpecification(ISpecification<Student> specification)
        {
            return SpecificationEvaluator<Student>.GetQuery(_context.Students.AsQueryable(), specification);
        }
    }
}
