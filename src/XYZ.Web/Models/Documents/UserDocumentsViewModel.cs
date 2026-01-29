using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using XYZ.Application.Features.Documents.Queries.DocumentStatus;
using XYZ.Application.Features.Documents.Queries.GetUserDocuments;

namespace XYZ.Web.Models.Documents
{
    public class UserDocumentsViewModel : IValidatableObject
    {
        private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
        private const long MaxFileBytes = 52_428_800;

        public string Title { get; set; } = string.Empty;

        public int OwnerId { get; set; }
        public string OwnerDisplayName { get; set; } = string.Empty;

        public UserDocumentStatusDto Status { get; set; } = new();

        public IList<DocumentListItemDto> Documents { get; set; } = new List<DocumentListItemDto>();

        public int? TypeFilter { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Lütfen belge türü seçiniz.")]
        public int DocumentDefinitionId { get; set; }

        [Required(ErrorMessage = "Lütfen bir dosya seçiniz.")]
        public IFormFile? File { get; set; }

        [StringLength(80, ErrorMessage = "Belge adı en fazla 80 karakter olabilir.")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        public bool IsCoach { get; set; }

        public IList<SelectListItem> DocumentDefinitionOptions { get; set; } = new List<SelectListItem>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (File is null)
                yield break;

            if (File.Length <= 0)
                yield return new ValidationResult("Dosya boş olamaz.", new[] { nameof(File) });

            if (File.Length > MaxFileBytes)
                yield return new ValidationResult("Dosya boyutu en fazla 50 MB olabilir.", new[] { nameof(File) });

            var ext = Path.GetExtension(File.FileName)?.ToLowerInvariant() ?? string.Empty;
            if (!AllowedExtensions.Contains(ext))
                yield return new ValidationResult("Sadece PDF/JPG/PNG dosyaları yüklenebilir.", new[] { nameof(File) });

            if (!string.IsNullOrWhiteSpace(Name) && Name.Length > 80)
                yield return new ValidationResult("Belge adı en fazla 80 karakter olabilir.", new[] { nameof(Name) });

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length > 500)
                yield return new ValidationResult("Açıklama en fazla 500 karakter olabilir.", new[] { nameof(Description) });
        }
    }
}
