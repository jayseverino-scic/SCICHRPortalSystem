namespace SCICHRPortal.Core.Interfaces
{
    public interface IRangeInserter<Entity>
    {
        Task InsertRangeAsync(List<Entity> entities);
    }
}
