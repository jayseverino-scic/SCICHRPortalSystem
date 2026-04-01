namespace SCICHRPortal.Core.Interfaces
{
    public interface IListRetriever<Entity>
    {
        Task<IEnumerable<Entity>> GetAllAsync();
    }
}
