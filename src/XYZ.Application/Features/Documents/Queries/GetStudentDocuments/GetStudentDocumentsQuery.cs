using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.GetStudentDocuments
{
    public class GetStudentDocumentsQuery
        : IRequest<IList<DocumentListItemDto>>
    {
        public int StudentId { get; set; }
        public DocumentType? Type { get; set; }
    }
}
