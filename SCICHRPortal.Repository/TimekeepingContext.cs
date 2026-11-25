using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Utility.Securities;

namespace SCICHRPortal.Repository
{
    public class TimekeepingContext : DbContext
    {
        private readonly ProjectDataScope _projectDataScope;
        private bool ProjectDataRestricted => _projectDataScope.Access.IsRestricted;
        private string? ScopedProjectName => _projectDataScope.Access.ProjectName;

        public TimekeepingContext(DbContextOptions<TimekeepingContext> options, ProjectDataScope? projectDataScope = null) : base(options)
        {
            _projectDataScope = projectDataScope ?? new();
        }

        // DbSets for the tables you want to read from SQL Server
        public DbSet<STimeLogs> TimeLogs { get; set; }
        public DbSet<SPersonnels> SPersonnels { get; set; }
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
            modelBuilder.Entity<SGroups>().HasQueryFilter("ProjectDataScope", row =>
                !ProjectDataRestricted || ScopedProjectName != null && row.Description != null && row.Description.Trim().ToUpper() == ScopedProjectName);
            modelBuilder.Entity<SZKDevices>().HasQueryFilter("ProjectDataScope", row =>
                !ProjectDataRestricted || ScopedProjectName != null && row.Name != null &&
                    SGroups.Any(group => !group.IsDeleted && group.Name != null && group.Name.Trim().ToUpper() == row.Name.Trim().ToUpper()) &&
                    !SGroups.IgnoreQueryFilters().Any(group => !group.IsDeleted && group.Name != null && group.Name.Trim().ToUpper() == row.Name.Trim().ToUpper() &&
                        group.Description != null && group.Description.Trim().ToUpper() != ScopedProjectName));
            modelBuilder.Entity<STimeLogs>().HasQueryFilter("ProjectDataScope", row =>
                !ProjectDataRestricted || row.DeviceSerialNumber != null &&
                    ZKDevices.Any(device => !device.IsDeleted && device.SerialNumber == row.DeviceSerialNumber));
            modelBuilder.Entity<SPersonnels>().HasQueryFilter("ProjectDataScope", row =>
                !ProjectDataRestricted || row.AccessNumber != null &&
                    TimeLogs.Any(log => !log.IsDeleted && log.AccessNumber == row.AccessNumber));
            base.OnModelCreating(modelBuilder);
        }
    }
}
