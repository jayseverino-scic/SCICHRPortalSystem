namespace SCICHRPortal.Utility.HttpContext.Interfaces
{
    public interface IHttpContextService
    {
        public string? GetUser();
        public string? GetReferrer();
        public string? GetSystem();
    }
}
