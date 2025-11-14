using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Features.Classes.Queries.GetAllClasses;

namespace XYZ.Application.Features.Classes.Queries.GetClassById
{
    public class GetClassByIdQuery : IRequest<ClassDetailDto>
    {
        public int ClassId { get; set; }
    }
}
