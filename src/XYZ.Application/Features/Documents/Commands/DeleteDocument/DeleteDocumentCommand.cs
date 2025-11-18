using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}
