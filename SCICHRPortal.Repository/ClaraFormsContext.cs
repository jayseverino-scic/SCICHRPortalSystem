using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Repository
{
    public sealed class ClaraFormsContext(DbContextOptions<ClaraFormsContext> options) : DbContext(options)
    {
        public DbSet<ClaraFormOTRequest> OvertimeRequests => Set<ClaraFormOTRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClaraFormOTRequest>().HasNoKey();
        }
    }
}
