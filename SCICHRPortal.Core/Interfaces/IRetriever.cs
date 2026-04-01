namespace SCICHRPortal.Core.Interfaces
{
    public interface IRetriever<Entity, Key>
    {
        Task<Entity> GetAsync(Key id);
    }
}
