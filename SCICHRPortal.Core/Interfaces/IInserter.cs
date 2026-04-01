namespace SCICHRPortal.Core.Interfaces
{
    public interface IInserter<Entity>
    {
        Task InsertAsync(Entity entity);
    }
}
