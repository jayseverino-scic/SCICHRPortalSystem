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
        public DbSet<STimeLogs> TimeLogs { get; set; }
        public DbSet<SPersonnels> Personnels { get; set; }
        public DbSet<SZKDevices> ZKDevices { get; set; }
        public DbSet<SGroups> SGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the entity to map to existing SQL Server tables
            modelBuilder.Entity<STimeLogs>(entity =>
            {
                entity.ToTable("TimeLogs", "dbo");
                entity.HasKey(e => e.Id);
                // Configure other properties
            });
            modelBuilder.Entity<SPersonnels>(entity =>
            {
                entity.ToTable("Personnels", "dbo");
                entity.HasKey(e => e.Id);
                // Configure other properties
            });
            modelBuilder.Entity<SZKDevices>(entity =>
            {
                entity.ToTable("ZKDevices", "dbo");
                entity.HasKey(e =>e.Id);
                //entity.Property(e => e.SerialNumber).HasMaxLength(64);
            });
            modelBuilder.Entity<SGroups>(entity =>
            {
                entity.ToTable("Groups", "dbo");
                entity.HasKey(e => e.Id);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
