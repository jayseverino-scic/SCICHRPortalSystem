using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using SCICHRPortal.Data.XscribeTables;

namespace SCICHRPortal.Repository
{
    public class XscribeContext : DbContext
    {
        public XscribeContext(DbContextOptions<XscribeContext> options) : base(options) { }

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

            base.OnModelCreating(modelBuilder);
        }
    }
}
