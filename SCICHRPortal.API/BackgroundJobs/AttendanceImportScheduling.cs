using Hangfire;
using Hangfire.PostgreSql;
using SCICHRPortal.Utility.Helpers;

namespace SCICHRPortal.API.BackgroundJobs;

public static class AttendanceImportScheduling
{
    private const string EnabledKey = "AttendanceImport:Enabled";
    private const string DashboardEnabledKey = "HangfireDashboard:Enabled";

    public static void AddAttendanceImports(this IServiceCollection services, IConfiguration configuration)
    {
        var workerEnabled = configuration.GetValue<bool>(EnabledKey);
        var dashboardEnabled = configuration.GetValue<bool>(DashboardEnabledKey);
        if (!workerEnabled && !dashboardEnabled) return;

        var connection = configuration.GetConnectionString("HangfireConnection");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("ConnectionStrings:HangfireConnection is required when the attendance worker or Hangfire dashboard is enabled.");

        services.AddHangfire(options => options
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(storage => storage.UseNpgsqlConnection(connection),
                new PostgreSqlStorageOptions { SchemaName = "hangfire", PrepareSchemaIfNecessary = false }));

        if (dashboardEnabled) services.AddAntiforgery();
        if (!workerEnabled) return;

        services.AddScoped<NightlyAttendanceJobs>();
        services.AddHangfireServer(options =>
        {
            options.Queues = [NightlyAttendanceJobs.QueueName];
            options.WorkerCount = 1;
        });
    }

    public static void UseAttendanceDashboard(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>(DashboardEnabledKey)) return;

        var username = app.Configuration["HangfireDashboard:Username"];
        var password = app.Configuration["HangfireDashboard:Password"];
        if (string.IsNullOrWhiteSpace(username) || username.Contains(':') || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Configure HangfireDashboard:Username without a colon and HangfireDashboard:Password before enabling the dashboard.");

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            AppPath = null,
            DashboardTitle = "SCIC Hangfire Dashboard",
            Authorization = [new HangfireDashboardAuthorizationFilter(username, password)]
        });
    }

    public static void RegisterAttendanceImports(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>(EnabledKey)) return;

        app.Services.GetRequiredService<IRecurringJobManager>().AddOrUpdate<NightlyAttendanceJobs>(
            NightlyAttendanceJobs.RecurringId,
            NightlyAttendanceJobs.QueueName,
            job => job.DispatchAsync(null, CancellationToken.None),
            Cron.Daily(),
            new RecurringJobOptions { TimeZone = PhilippineTime.Zone, MisfireHandling = MisfireHandlingMode.Strict });
    }
}
