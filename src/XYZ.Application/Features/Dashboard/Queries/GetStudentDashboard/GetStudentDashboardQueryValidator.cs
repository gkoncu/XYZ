using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard
{
    public class GetStudentDashboardQueryValidator
        : AbstractValidator<GetStudentDashboardQuery>
    {
        public GetStudentDashboardQueryValidator()
        {
        }
    }
}
