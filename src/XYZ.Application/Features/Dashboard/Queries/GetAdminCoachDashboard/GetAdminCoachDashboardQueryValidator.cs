using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard
{
    public class GetAdminCoachDashboardQueryValidator
        : AbstractValidator<GetAdminCoachDashboardQuery>
    {
        public GetAdminCoachDashboardQueryValidator()
        {
        }
    }
}
