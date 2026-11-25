using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Utility.Securities;

namespace SCICHRPortal.Repository
{
    public class XscribeContext : DbContext
    {
        private readonly ProjectDataScope _projectDataScope;
        private bool ProjectDataRestricted => _projectDataScope.Access.IsRestricted;
        private string? ScopedProjectCode => _projectDataScope.Access.ProjectCode;
        private string? ScopedProjectName => _projectDataScope.Access.ProjectName;

        public XscribeContext(DbContextOptions<XscribeContext> options, ProjectDataScope? projectDataScope = null) : base(options)
        {
            _projectDataScope = projectDataScope ?? new();
        }

        // DbSets for the tables you want to read from SQL Server
        public DbSet<XCompany_Branch> Company_Branch { get; set; }
        public DbSet<XDepartment> Department { get; set; }
        public DbSet<XEmployee> Employee { get; set; }
        public DbSet<XCompany_Position> Company_Position { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<XCompany_Branch>(entity =>
            {
                entity.ToView("Company_Branch");
                entity.HasKey(e =>e.Id);
            });
            modelBuilder.Entity<XDepartment>(entity =>
            {
                entity.ToView("Department");
                entity.HasKey(e => e.Id);
            });
            modelBuilder.Entity<XEmployee>(entity =>
            {
                entity.ToView("Employee");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.Department_Id)
                      .HasConstraintName("FK_XEmployee_Department");

                entity.HasOne(e => e.Company_Branch)
                      .WithMany()
                      .HasForeignKey(e => e.Company_Branch_Id) // Use the property with underscore
                      .HasConstraintName("FK_XEmployee_Company_Branch");

                entity.HasOne(e => e.Company_Position)
                      .WithMany()
                      .HasForeignKey(e => e.Company_Position_Id)
                      .HasConstraintName("FK_XEmployee_Company_Position");
            });
            modelBuilder.Entity<XCompany_Position>(entity =>
            {
                entity.ToView("Company_Position");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<XCompany_Branch>().HasQueryFilter("ProjectDataScope", row =>
                !ProjectDataRestricted || ScopedProjectCode != null && row.Code != null && row.Code.Trim().ToUpper() == ScopedProjectCode ||
                    ScopedProjectCode == null && ScopedProjectName != null && row.Name != null && row.Name.Trim().ToUpper() == ScopedProjectName);
            modelBuilder.Entity<XEmployee>().HasQueryFilter("ProjectDataScope", row =>
                !ProjectDataRestricted || Company_Branch.Any(branch => !branch._Deleted && branch.Id == row.Company_Branch_Id));
            base.OnModelCreating(modelBuilder);
        }
    }
}
