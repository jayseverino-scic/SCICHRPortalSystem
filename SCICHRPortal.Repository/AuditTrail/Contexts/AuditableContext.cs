using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Utility.HttpContext.Interfaces;

namespace SCICHRPortal.Repository.AuditTrail.Contexts
{
    public abstract class AuditableContext : DbContext
    {
        private readonly IHttpContextService? _httpContextService;

        public AuditableContext(DbContextOptions options) : base(options)
        {
        }

        public AuditableContext(DbContextOptions options, IHttpContextService httpContextService) : base(options)
        {
            _httpContextService = httpContextService;
        }

        public DbSet<Audit>? AuditLogs { get; set; }

        public virtual async Task<int> SaveChangesAsync()
        {
            var requestOrigin = _httpContextService!.GetReferrer();
            var userId = _httpContextService!.GetUser();
            var systemName = _httpContextService!.GetSystem();
            var auditEntries = OnBeforeSaveChanges(userId, requestOrigin!, systemName);
            var result = await base.SaveChangesAsync();
            await OnAfterSaveChanges(auditEntries);

            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges(string? userId, string? requestOrigin, string? systemName)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Audit || entry.State == EntityState.Detached || (entry.State == EntityState.Unchanged))
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Entity.GetType().Name,
                    UserName = userId,
                    SystemName = systemName,
                    RequestOrigin = requestOrigin
                };

                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;

                            if (propertyName == "Active")
                            {
                                auditEntry.NewValues[propertyName] = true;
                            }

                            if (propertyName == "CreatedAt")
                            {
                                auditEntry.NewValues[propertyName] = DateTime.UtcNow;
                            }

                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;

                            if (propertyName == "UpdatedAt")
                            {
                                auditEntry.NewValues[propertyName] = DateTime.UtcNow;
                            }

                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue!;
                                auditEntry.NewValues[propertyName] = property.CurrentValue!;

                                if (propertyName == "UpdatedAt")
                                {
                                    auditEntry.NewValues[propertyName] = DateTime.UtcNow;
                                }
                            }
                            break;
                    }
                }
                // Since we are only implementing soft delete, here's how we overrride it
                if (entry.State == EntityState.Deleted && entry.Metadata.GetSchema() != "dbo")
                    entry.State = EntityState.Modified;
            }

            if (!string.IsNullOrEmpty(requestOrigin) && !(string.IsNullOrEmpty(userId)))
            {
                foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
                {
                    AuditLogs!.Add(auditEntry.ToAudit());
                }
            }

            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
          
            if ((auditEntries == null || auditEntries.Count == 0))
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                }
                AuditLogs!.Add(auditEntry.ToAudit());
            }
            return SaveChangesAsync();
        }
    }
}
