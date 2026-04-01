namespace SCICHRPortal.Core.Interfaces
{
    public interface IDeleter<Entity>
    {
        Task DeleteAsync(Entity entity);
    }
}
