namespace SCICHRPortal.Repository.Implementations
{
    public class Repository
    {
        protected ApplicationContext Context { get; }

        public Repository(ApplicationContext context)
        {
            Context = context;
        }

        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync();
        }
    }
}
