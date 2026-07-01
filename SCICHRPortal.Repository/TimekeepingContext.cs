using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using SCICHRPortal.Data.TimekeepingTables;

namespace SCICHRPortal.Repository
{
    public class TimekeepingContext : DbContext
    {
        public TimekeepingContext(DbContextOptions<TimekeepingContext> options) : base(options) { }

        // DbSets for the tables you want to read from SQL Server
        public DbSet<TimeLogs> TimeLogs { get; set; }
        public DbSet<Personnels> Personnels { get; set; }
        public DbSet<ZKDevices> ZKDevices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the entity to map to existing SQL Server tables
            modelBuilder.Entity<TimeLogs>(entity =>
            {
                entity.ToTable("TimeLogs", "dbo");
                entity.HasKey(e => e.Id);
                // Configure other properties
            });
            modelBuilder.Entity<Personnels>(entity =>
            {
                entity.ToTable("Personnels", "dbo");
                entity.HasKey(e => e.Id);
                // Configure other properties
            });
            modelBuilder.Entity<ZKDevices>(entity =>
            {
                entity.ToTable("ZKDevices", "dbo");
                entity.HasKey(e =>e.Id);
            });
            modelBuilder.Entity<ZKDevices>(entity =>
            {
                entity.ToTable("Groups", "dbo");
                entity.HasKey(e => e.Id);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
