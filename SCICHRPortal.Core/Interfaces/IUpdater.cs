namespace SCICHRPortal.Core.Interfaces
{
    public interface IUpdater<Entity>
    {
        Task UpdateAsync(Entity entity);
    }
}
