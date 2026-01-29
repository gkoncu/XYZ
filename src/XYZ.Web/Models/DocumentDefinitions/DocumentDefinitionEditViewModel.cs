using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.DocumentDefinitions
{
    public class DocumentDefinitionEditViewModel
    {
        [Range(0, int.MaxValue, ErrorMessage = "Geçersiz kayıt.")]
        public int Id { get; set; }

        [Range(1, 2, ErrorMessage = "Geçersiz hedef tipi.")]
        public int Target { get; set; }

        [Required(ErrorMessage = "Belge adı zorunludur.")]
        [StringLength(80, ErrorMessage = "Belge adı en fazla 80 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, 10000, ErrorMessage = "Sıra değeri geçersiz.")]
        public int SortOrder { get; set; } = 100;
    }
}
