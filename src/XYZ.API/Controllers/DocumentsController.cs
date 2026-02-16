using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Documents.Commands.CreateDocument;
using XYZ.Application.Features.Documents.Commands.DeleteDocument;
using XYZ.Application.Features.Documents.Commands.UpdateDocument;
using XYZ.Application.Features.Documents.Queries.DocumentStatus;
using XYZ.Application.Features.Documents.Queries.DocumentStatus.GetCoachDocumentStatus;
using XYZ.Application.Features.Documents.Queries.DocumentStatus.GetCoachesDocumentStatus;
using XYZ.Application.Features.Documents.Queries.DocumentStatus.GetStudentDocumentStatus;
using XYZ.Application.Features.Documents.Queries.DocumentStatus.GetStudentsDocumentStatus;
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
        public async Task<ActionResult<DocumentDetailDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetDocumentByIdQuery { Id = id }, cancellationToken);
            return Ok(result);
        }

        [HttpGet("student/{studentId:int}")]
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

            try
            {
                var id = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch
            {
                await _fileService.DeleteFileAsync(filePath);
                throw;
            }
        }

        [HttpPost("coach/{coachId:int}")]
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

            try
            {
                var id = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch
            {
                await _fileService.DeleteFileAsync(filePath);
                throw;
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<int>> Update(int id, [FromBody] UpdateDocumentCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<int>> Delete(int id, CancellationToken cancellationToken)
        {
            var deletedId = await _mediator.Send(new DeleteDocumentCommand { Id = id }, cancellationToken);
            return Ok(deletedId);
        }

        [HttpGet("{id:int}/download")]
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

        [HttpGet("student/{studentId:int}/status")]
        [ProducesResponseType(typeof(UserDocumentStatusDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDocumentStatusDto>> GetStudentStatus(int studentId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetStudentDocumentStatusQuery { StudentId = studentId },
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("coach/{coachId:int}/status")]
        [ProducesResponseType(typeof(UserDocumentStatusDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDocumentStatusDto>> GetCoachStatus(
            int coachId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetCoachDocumentStatusQuery { CoachId = coachId },
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("students/status")]
        [ProducesResponseType(typeof(IList<StudentDocumentStatusListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IList<StudentDocumentStatusListItemDto>>> GetStudentsStatus(
            [FromQuery] bool onlyIncomplete,
            [FromQuery] string? searchTerm,
            [FromQuery] int take = 200,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new GetStudentsDocumentStatusQuery
                {
                    OnlyIncomplete = onlyIncomplete,
                    SearchTerm = searchTerm,
                    Take = take
                },
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("coaches/status")]
        [ProducesResponseType(typeof(IList<CoachDocumentStatusListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IList<CoachDocumentStatusListItemDto>>> GetCoachesStatus(
            [FromQuery] bool onlyIncomplete,
            [FromQuery] string? searchTerm,
            [FromQuery] int take = 200,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new GetCoachesDocumentStatusQuery
                {
                    OnlyIncomplete = onlyIncomplete,
                    SearchTerm = searchTerm,
                    Take = take
                },
                cancellationToken);

            return Ok(result);
        }
    }
}
