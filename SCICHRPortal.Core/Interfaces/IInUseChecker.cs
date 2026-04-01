namespace SCICHRPortal.Core.Interfaces
{
    public interface IInUseChecker<Key>
    {
        Task<bool> IsInUseAsync(Key id);
    }
}
