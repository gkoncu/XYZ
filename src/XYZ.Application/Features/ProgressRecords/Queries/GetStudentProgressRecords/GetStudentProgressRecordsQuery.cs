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
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public int? BranchId { get; set; }
    }
}
