using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Queries.GetCurrentTenantTheme
{
    public class TenantThemeDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "XYZ Club";

        public string PrimaryColor { get; set; } = "#3B82F6";
        public string SecondaryColor { get; set; } = "#1E40AF";

        public string? LogoUrl { get; set; }
    }
}
