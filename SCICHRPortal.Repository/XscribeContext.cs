using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;

namespace SCICHRPortal.Repository
{
    public class XscribeContext : DbContext
    {
        public XscribeContext(DbContextOptions<XscribeContext> options) : base(options) { }

        // DbSets for the tables you want to read from SQL Server
        public DbSet<Company_Branch> Company_Branch{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company_Branch>(entity =>
            {
                entity.ToTable("Company_Branch", "dbo");
                entity.HasKey(e =>e.Id);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
