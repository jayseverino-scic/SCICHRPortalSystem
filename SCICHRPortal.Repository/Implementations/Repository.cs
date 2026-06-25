namespace SCICHRPortal.Repository.Implementations
{
    public class Repository
    {
        protected ApplicationContext Context { get; }
        protected XscribeContext XscribeContext { get; }
        protected TimekeepingContext TimekeepingContext { get; }
        

        public Repository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
        {
            Context = context;
            XscribeContext = xscribeContext;
            TimekeepingContext = timekeepingContext;
        }

        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync();
        }
    }
}
