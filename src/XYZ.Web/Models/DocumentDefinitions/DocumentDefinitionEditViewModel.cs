namespace XYZ.Web.Models.DocumentDefinitions
{
    public class DocumentDefinitionEditViewModel
    {
        public int Id { get; set; }

        public int Target { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 100;
    }
}
