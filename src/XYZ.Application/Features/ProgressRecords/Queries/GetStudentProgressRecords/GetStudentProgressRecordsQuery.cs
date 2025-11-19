using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords
{
    public class GetStudentProgressRecordsQuery
        : IRequest<IList<ProgressRecordListItemDto>>
    {
        public int StudentId { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
