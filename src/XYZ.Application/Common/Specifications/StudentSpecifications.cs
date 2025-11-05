using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Specifications
{
    public class StudentWithDetailsSpecification : BaseSpecification<Student>
    {
        public StudentWithDetailsSpecification(int studentId) : base(s => s.Id == studentId)
        {
            AddInclude(s => s.User);
            AddInclude(s => s.Class);
            AddInclude($"{nameof(Student.Class)}.{nameof(Class.Coaches)}.{nameof(Coach.User)}");
        }
    }

    public class StudentSearchSpecification : BaseSpecification<Student>
    {
        public StudentSearchSpecification(string searchTerm, int? classId, string branch)
        {
            Expression<Func<Student, bool>> criteria = s => true;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                criteria = CombineCriteria(criteria, s =>
                    s.User.FirstName.Contains(searchTerm) ||
                    s.User.LastName.Contains(searchTerm) ||
                    s.User.Email.Contains(searchTerm) ||
                    s.IdentityNumber.Contains(searchTerm));
            }

            if (classId.HasValue)
            {
                criteria = CombineCriteria(criteria, s => s.ClassId == classId.Value);
            }

            if (!string.IsNullOrEmpty(branch))
            {
                criteria = CombineCriteria(criteria, s => s.Branch == branch);
            }

            AddCriteria(criteria);
            AddInclude(s => s.User);
            AddInclude(s => s.Class);
            AddInclude($"{nameof(Student.Class)}.{nameof(Class.Coaches)}.{nameof(Coach.User)}");

            ApplyOrderBy(s => s.User.LastName);
            ApplyOrderBy(s => s.User.FirstName);
        }

        private static Expression<Func<Student, bool>> CombineCriteria(
            Expression<Func<Student, bool>> left,
            Expression<Func<Student, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(Student));
            var combined = Expression.AndAlso(
                Expression.Invoke(left, parameter),
                Expression.Invoke(right, parameter)
            );
            return Expression.Lambda<Func<Student, bool>>(combined, parameter);
        }
    }

    public class StudentDataScopeSpecification : BaseSpecification<Student>
    {
        public StudentDataScopeSpecification(ICurrentUserService currentUserService)
        {
            var user = currentUserService;
            if (user == null)
            {
                AddCriteria(s => false);
                return;
            }

            Expression<Func<Student, bool>> criteria = user.Role switch
            {
                "SuperAdmin" => s => true,
                "Admin" => s => s.TenantId == user.TenantId,
                "Coach" => s => s.TenantId == user.TenantId &&
                               s.Class != null &&
                               s.Class.Coaches.Any(c => c.Id == user.CoachId),
                "Student" => s => s.Id == user.StudentId,
                _ => s => false
            };

            AddCriteria(criteria);
        }
    }
}
