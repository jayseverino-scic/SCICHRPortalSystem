using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Mappings;
using SCICHRPortal.Data.Mappings.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.AuditTrail.Contexts;
using SCICHRPortal.Utility.HttpContext.Interfaces;

namespace SCICHRPortal.Repository
{
    public class ApplicationContext : AuditableContext
    {
        private readonly IHttpContextService? _httpContextService;
        //static ApplicationContext()
        //{
        //    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        //}
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
        public ApplicationContext(DbContextOptions<ApplicationContext> options, IHttpContextService httpContextService)
           : base(options, httpContextService) {
            _httpContextService = httpContextService;
        }
        public DbSet<Announcement>? Announcement { get; set; }
        public DbSet<User>? User { get; set; }
        public DbSet<Role>? Role { get; set; }
        public DbSet<UserRole>? UserRole { get; set; }
       
        public DbSet<Module>? Module { get; set; }
        public DbSet<Holiday>? Holiday { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Shift> Shift { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeShift> EmployeeShift { get; set; }
        public DbSet<EmployeeTimeLog> EmployeeTimeLog { get; set; }
        public DbSet<EmployeeAttendance> EmployeeAttendance { get; set; }
        public DbSet<CutOff> CutOff { get; set; }
        public DbSet<LeaveType> LeaveType { get; set; }
        public DbSet<LeaveRequest> LeaveRequest { get; set; }
        public DbSet<BiometricsLog> BiometricsLog { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<EmployeeShiftDevice> EmployeeShiftDevice { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<XCompany_Branch>();
            modelBuilder.Ignore<XCompany_Position>();
            modelBuilder.Ignore<XDepartment>();
            modelBuilder.Ignore<XEmployee>();
            modelBuilder.Ignore<SZKDevices>();
            //modelBuilder.Entity<XCompany_Branch>()
            //     .ToView("XCompany_Branch", schema: "external")
            //     .HasNoKey();
            //modelBuilder.Entity<XDepartment>()
            //     .ToView("XDepartment", schema: "external")
            //     .HasNoKey();
            //modelBuilder.Entity<XEmployee>()
            //     .ToView("XEmployee", schema: "external")
            //     .HasNoKey();
            //modelBuilder.Entity<SZKDevices>()
            //     .ToView("SZKDevices", schema: "external")
            //     .HasNoKey();
            //modelBuilder.Entity<XCompany_Position>()
            //     .ToView("XCompany_Position", schema: "external")
            //     .HasNoKey();
            new AnnouncementMap(modelBuilder.Entity<Announcement>());
            new UserMap(modelBuilder.Entity<User>());
            new RoleMap(modelBuilder.Entity<Role>());
            new UserRoleMap(modelBuilder.Entity<UserRole>());
            new ModuleMap(modelBuilder.Entity<Module>());
            new HolidayMap(modelBuilder.Entity<Holiday>());
            new DepartmentMap(modelBuilder.Entity<Department>());
            new ShiftMap(modelBuilder.Entity<Shift>());
            new EmployeeMap(modelBuilder.Entity<Employee>());
            new EmployeeShiftMap(modelBuilder.Entity<EmployeeShift>());
            new EmployeeTimeLogMap(modelBuilder.Entity<EmployeeTimeLog>());
            new EmployeeAttendanceMap(modelBuilder.Entity<EmployeeAttendance>());
            new CutOffMap(modelBuilder.Entity<CutOff>());
            new LeaveTypeMap(modelBuilder.Entity<LeaveType>());
            new LeaveRequestMap(modelBuilder.Entity<LeaveRequest>());
            new BiometricsLogMap(modelBuilder.Entity<BiometricsLog>());
            new PositionMap(modelBuilder.Entity<Position>());
            new EmployeeShiftDeviceMap(modelBuilder.Entity<EmployeeShiftDevice>());
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {

                var boolProps = entity.GetProperties()
                    .Where(p => p.PropertyInfo?.PropertyType == typeof(bool));
                var dateTimeProps = entity.GetProperties()
                    .Where(p => p.PropertyInfo?.PropertyType == typeof(DateTime));
                if (dateTimeProps.Any())
                    foreach (var prop in dateTimeProps)
                    {
                        if (prop.Name == "CreatedAt")
                        {
                            modelBuilder.Entity(entity.Name).Property(prop.Name).ValueGeneratedOnAdd();
                        }
                        if (prop.Name == "UpdatedAt")
                        {
                            modelBuilder.Entity(entity.Name).Property(prop.Name).ValueGeneratedOnUpdate();
                        }
                    }
                foreach (var prop in boolProps)
                {
                    if (prop.Name == "Active")
                    {
                        modelBuilder.Entity(entity.Name).Property(prop.Name).HasDefaultValue(true);
                    }
                    if (prop.Name == "Deleted")
                    {
                        modelBuilder.Entity(entity.Name).Property(prop.Name).HasDefaultValue(false);
                    }
                }
            }
        }

        public new TEntity? Update<TEntity>(TEntity value) where TEntity : BaseEntity
        {
            DbSet<TEntity> dbSet = base.Set<TEntity>();

            TEntity? record = dbSet.Find(GetPrimaryKey(value));
            if (record is not null)
            {
                base.Entry(record!).CurrentValues.SetValues(value);
                record.UpdatedAt = DateTime.UtcNow;
                base.Entry(record!).Property(x => x.Deleted).IsModified = false;
                base.Entry(record!).Property(x => x.CreatedAt).IsModified = false;
                base.Entry(record!).Property(x => x.CreatedBy).IsModified = false;
            }
            return record;
        }

        private object GetPrimaryKey<TEntity>(TEntity entity) where TEntity : class
        {
            string? primaryKeyName = base.Model!.FindEntityType(typeof(TEntity))!.FindPrimaryKey()!.Properties.Select(x => x.Name).Single();
            var id = entity.GetType().GetProperty(primaryKeyName)!.GetValue(entity);
            return id!;
        }
    }
}
