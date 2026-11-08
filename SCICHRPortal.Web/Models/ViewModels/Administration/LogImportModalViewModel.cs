namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class LogImportModalViewModel
    {
        public required string Id { get; init; }
        public required string Title { get; init; }
        public bool IsFileImport { get; init; }
    }
}
