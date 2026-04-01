namespace SCICHRPortal.Core.Interfaces
{
    public interface IRangeDeleter<Entity>
    {
        Task DeleteRangeAsync(List<Entity> entities);
    }
}
