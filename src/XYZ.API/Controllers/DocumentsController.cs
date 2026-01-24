using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Documents.Commands.CreateDocument;
using XYZ.Application.Features.Documents.Commands.DeleteDocument;
using XYZ.Application.Features.Documents.Commands.UpdateDocument;
using XYZ.Application.Features.Documents.Queries.GetDocumentById;
using XYZ.Application.Features.Documents.Queries.GetUserDocuments;
using XYZ.Domain.Enums;

namespace XYZ.API.Controllers
{
    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int DocumentDefinitionId { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFileService _fileService;

        public DocumentsController(IMediator mediator, IFileService fileService)
        {
            _mediator = mediator;
            _fileService = fileService;
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<ActionResult<DocumentDetailDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetDocumentByIdQuery { Id = id }, cancellationToken);
            return Ok(result);
        }

        [HttpGet("student/{studentId:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<ActionResult<IList<DocumentListItemDto>>> GetByStudent(
            int studentId,
            [FromQuery] int? documentDefinitionId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserDocumentsQuery
            {
                Target = DocumentTarget.Student,
                OwnerId = studentId,
                DocumentDefinitionId = documentDefinitionId
            }, cancellationToken);

            return Ok(result);
        }

        [HttpGet("coach/{coachId:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<ActionResult<IList<DocumentListItemDto>>> GetByCoach(
            int coachId,
            [FromQuery] int? documentDefinitionId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserDocumentsQuery
            {
                Target = DocumentTarget.Coach,
                OwnerId = coachId,
                DocumentDefinitionId = documentDefinitionId
            }, cancellationToken);

            return Ok(result);
        }

        [HttpPost("student/{studentId:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        [RequestSizeLimit(52_428_800)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<int>> UploadForStudent(
            int studentId,
            [FromForm] UploadDocumentRequest model,
            CancellationToken cancellationToken)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("Dosya yüklenmedi.");

            await using var stream = model.File.OpenReadStream();
            var filePath = await _fileService.UploadFileAsync(stream, model.File.FileName);

            var command = new CreateDocumentCommand
            {
                StudentId = studentId,
                DocumentDefinitionId = model.DocumentDefinitionId,
                Name = string.IsNullOrWhiteSpace(model.Name) ? model.File.FileName : model.Name!,
                Description = model.Description,
                FilePath = filePath
            };

            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPost("coach/{coachId:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [RequestSizeLimit(52_428_800)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<int>> UploadForCoach(
            int coachId,
            [FromForm] UploadDocumentRequest model,
            CancellationToken cancellationToken)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("Dosya yüklenmedi.");

            await using var stream = model.File.OpenReadStream();
            var filePath = await _fileService.UploadFileAsync(stream, model.File.FileName);

            var command = new CreateDocumentCommand
            {
                CoachId = coachId,
                DocumentDefinitionId = model.DocumentDefinitionId,
                Name = string.IsNullOrWhiteSpace(model.Name) ? model.File.FileName : model.Name!,
                Description = model.Description,
                FilePath = filePath
            };

            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<ActionResult<int>> Update(int id, [FromBody] UpdateDocumentCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<ActionResult<int>> Delete(int id, CancellationToken cancellationToken)
        {
            var deletedId = await _mediator.Send(new DeleteDocumentCommand { Id = id }, cancellationToken);
            return Ok(deletedId);
        }

        [HttpGet("{id:int}/download")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
        {
            var detail = await _mediator.Send(new GetDocumentByIdQuery { Id = id }, cancellationToken);

            var stream = await _fileService.DownloadFileAsync(detail.FilePath);
            var contentType = "application/octet-stream";
            var fileName = string.IsNullOrWhiteSpace(detail.Name)
                ? Path.GetFileName(detail.FilePath)
                : detail.Name;

            return File(stream, contentType, fileName);
        }
    }
}
