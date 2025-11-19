using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById
{
    public class GetProgressRecordByIdQuery : IRequest<ProgressRecordDetailDto>
    {
        public int Id { get; set; }
    }
}
