using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.UpdateClass
{
    public class UpdateClassCommand : IRequest<int>
    {
        public int ClassId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? AgeGroupMin { get; set; }

        public int? AgeGroupMax { get; set; }

        public int MaxCapacity { get; set; }

        public int BranchId { get; set; }
    }
}
