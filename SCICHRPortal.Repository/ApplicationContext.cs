using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Mappings;
using SCICHRPortal.Data.Mappings.Metadatas;
using SCICHRPortal.Repository.AuditTrail.Contexts;
using SCICHRPortal.Utility.HttpContext.Interfaces;
using SCICHRPortal.Utility.Securities;

namespace SCICHRPortal.Repository
{
    public partial class ApplicationContext : AuditableContext
    {
        private readonly IHttpContextService? _httpContextService;
        private readonly ProjectDataScope _projectDataScope = new();
        //static ApplicationContext()
        //{
        //    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        //}
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
        public ApplicationContext(DbContextOptions<ApplicationContext> options, IHttpContextService httpContextService,
            ProjectDataScope? projectDataScope = null)
           : base(options, httpContextService) {
            _httpContextService = httpContextService;
            _projectDataScope = projectDataScope ?? new();
        }
        public DbSet<Announcement>? Announcement { get; set; }
        public DbSet<User>? User { get; set; }
        public DbSet<Role>? Role { get; set; }
        public DbSet<UserRole>? UserRole { get; set; }
       
        public DbSet<Module>? Module { get; set; }
        public DbSet<Holiday>? Holiday { get; set; }
        public DbSet<HolidayProject> HolidayProject { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Shift> Shift { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeShift> EmployeeShift { get; set; }
        public DbSet<EmployeeTimeLog> EmployeeTimeLog { get; set; }
        public DbSet<TimeLogAttachment> TimeLogAttachment { get; set; }
        public DbSet<EmployeeAttendance> EmployeeAttendance { get; set; }
        public DbSet<EmployeeDailyAttendanceComputation> EmployeeDailyAttendanceComputation { get; set; }
        public DbSet<EmployeeDailyAttendanceImportCheckpoint> EmployeeDailyAttendanceImportCheckpoint { get; set; }
        public DbSet<CutOff> CutOff { get; set; }
        public DbSet<LeaveType> LeaveType { get; set; }
        public DbSet<LeaveRequest> LeaveRequest { get; set; }
        public DbSet<OfficialBusiness> OfficialBusiness { get; set; }
        public DbSet<Leave> Leave { get; set; }
        public DbSet<BiometricsLog> BiometricsLog { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<EmployeeShiftDevice> EmployeeShiftDevice { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<Project> Project { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            new AnnouncementMap(modelBuilder.Entity<Announcement>());
            new UserMap(modelBuilder.Entity<User>());
            // The generated value also covers legacy/employee writers that do not normalize email.
            modelBuilder.Entity<User>().Property<string>("NormalizedEmail")
                .HasComputedColumnSql("lower(btrim(\"Email\"))", stored: true);
            modelBuilder.Entity<User>().HasIndex("NormalizedEmail").IsUnique()
                .HasFilter("\"Deleted\" = false");
            new RoleMap(modelBuilder.Entity<Role>());
            new UserRoleMap(modelBuilder.Entity<UserRole>());
            new ModuleMap(modelBuilder.Entity<Module>());
            new HolidayMap(modelBuilder.Entity<Holiday>());
            new HolidayProjectMap(modelBuilder.Entity<HolidayProject>());
            new DepartmentMap(modelBuilder.Entity<Department>());
            new ShiftMap(modelBuilder.Entity<Shift>());
            new EmployeeMap(modelBuilder.Entity<Employee>());
            new EmployeeShiftMap(modelBuilder.Entity<EmployeeShift>());
            // Provider-specific period constraints live with ApplicationContext;
            // the Data project intentionally depends only on EF Core.
            var shiftAssignment = modelBuilder.Entity<EmployeeShift>();
            shiftAssignment.HasIndex(row => row.EmployeeId);
            shiftAssignment.Property(row => row.EffectiveStartDate).HasColumnType("timestamp without time zone");
            shiftAssignment.Property(row => row.EffectiveEndDate).HasColumnType("timestamp without time zone");
            shiftAssignment.HasIndex(row => new { row.EmployeeId, row.EffectiveStartDate }).IsUnique()
                .HasFilter("\"Deleted\" = false AND \"EffectiveStartDate\" IS NOT NULL");
            shiftAssignment.ToTable("EmployeeShift", table => table.HasCheckConstraint("CK_EmployeeShift_EffectivePeriod",
                "\"EffectiveEndDate\" IS NULL OR \"EffectiveStartDate\" IS NULL OR \"EffectiveEndDate\" > \"EffectiveStartDate\""));
            new EmployeeTimeLogMap(modelBuilder.Entity<EmployeeTimeLog>());
            modelBuilder.Entity<EmployeeTimeLog>().Property(e => e.Version).HasDefaultValue(0L);
            modelBuilder.Entity<EmployeeTimeLog>().Property(e => e.IsOB).HasDefaultValue(false);
            modelBuilder.Entity<TimeLogAttachment>().HasKey(e => e.TimeLogAttachmentId);
            modelBuilder.Entity<TimeLogAttachment>().HasIndex(e => e.TimeLogId).IsUnique();
            new EmployeeAttendanceMap(modelBuilder.Entity<EmployeeAttendance>());
            var dailyComputation = modelBuilder.Entity<EmployeeDailyAttendanceComputation>();
            new EmployeeDailyAttendanceComputationMap(dailyComputation);
            dailyComputation.HasIndex(row => new { row.AttendanceDate, row.ProjectId, row.TimeIn, row.TimeLogId })
                .HasFilter("\"Deleted\" = false");
            dailyComputation.Property(row => row.TimeIn).HasColumnType("timestamp without time zone");
            dailyComputation.Property(row => row.TimeOut).HasColumnType("timestamp without time zone");
            dailyComputation.Property(row => row.ShiftStart).HasColumnType("timestamp without time zone");
            dailyComputation.Property(row => row.ShiftEnd).HasColumnType("timestamp without time zone");
            dailyComputation.Property(row => row.CreatedAt).HasColumnType("timestamp with time zone");
            dailyComputation.Property(row => row.UpdatedAt).HasColumnType("timestamp with time zone");
            new EmployeeDailyAttendanceImportCheckpointMap(modelBuilder.Entity<EmployeeDailyAttendanceImportCheckpoint>());
            new CutOffMap(modelBuilder.Entity<CutOff>());
            new LeaveTypeMap(modelBuilder.Entity<LeaveType>());
            new LeaveRequestMap(modelBuilder.Entity<LeaveRequest>());
            var officialBusiness = modelBuilder.Entity<OfficialBusiness>();
            new OfficialBusinessMap(officialBusiness);
            // These are Manila business times, independent of the legacy timestamp switch.
            officialBusiness.Property(row => row.From).HasColumnType("timestamp without time zone");
            officialBusiness.Property(row => row.To).HasColumnType("timestamp without time zone");
            officialBusiness.Property(row => row.CreatedAt).HasColumnType("timestamp with time zone")
                .HasConversion(value => value.ToUniversalTime(), value => value.ToUniversalTime());
            officialBusiness.Property(row => row.UpdatedAt).HasColumnType("timestamp with time zone")
                .HasConversion(value => value.HasValue ? value.Value.ToUniversalTime() : (DateTime?)null,
                    value => value.HasValue ? value.Value.ToUniversalTime() : (DateTime?)null);
            officialBusiness.ToTable("OfficialBusiness", table => table.HasCheckConstraint("CK_OfficialBusiness_Period", "\"To\" > \"From\""));
            var leave = modelBuilder.Entity<Leave>();
            new LeaveMap(leave);
            leave.Property(row => row.From).HasColumnType("timestamp without time zone");
            leave.Property(row => row.To).HasColumnType("timestamp without time zone");
            leave.Property(row => row.CreatedAt).HasColumnType("timestamp with time zone")
                .HasConversion(value => value.ToUniversalTime(), value => value.ToUniversalTime());
            leave.Property(row => row.UpdatedAt).HasColumnType("timestamp with time zone")
                .HasConversion(value => value.HasValue ? value.Value.ToUniversalTime() : (DateTime?)null,
                    value => value.HasValue ? value.Value.ToUniversalTime() : (DateTime?)null);
            leave.ToTable("Leave", table => table.HasCheckConstraint("CK_Leave_Period", "\"To\" > \"From\""));
            new BiometricsLogMap(modelBuilder.Entity<BiometricsLog>());
            new PositionMap(modelBuilder.Entity<Position>());
            new EmployeeShiftDeviceMap(modelBuilder.Entity<EmployeeShiftDevice>());
            new DeviceMap(modelBuilder.Entity<Device>());
            new ProjectMap(modelBuilder.Entity<Project>());
            ConfigureProjectDataScope(modelBuilder);
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
