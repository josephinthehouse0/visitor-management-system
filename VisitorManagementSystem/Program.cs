using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Forms;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;

namespace VisitorManagementSystem;

static class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.example.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, configuration);
        Services = serviceCollection.BuildServiceProvider();

        using var scope = Services.CreateScope();
        var databaseFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        try
        {
            using var database = databaseFactory.CreateDbContext();
            if (!SharedTablesAlreadyExist(database))
            {
                database.Database.Migrate();
            }
            SeedRuntimeData(database);
        }
        catch (Exception ex)
        {
            if (IsExistingDatabaseSchemaError(ex))
            {
                try
                {
                    using var database = databaseFactory.CreateDbContext();
                    SeedRuntimeData(database);
                }
                catch (Exception seedEx)
                {
                    MessageBox.Show(
                        "The Azure SQL database already has tables, but seed data could not be verified.\n\n" + seedEx.Message,
                        "Azure SQL Connection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                    "Database connection failed. Check appsettings.json, Azure SQL firewall rules, and your password.\n\n" + ex.Message,
                    "Azure SQL Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        using var login = Services.GetRequiredService<LoginForm>();
        if (login.ShowDialog() == DialogResult.OK)
        {
            Application.Run(Services.GetRequiredService<MainDashboardForm>());
        }

        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static bool IsExistingDatabaseSchemaError(Exception exception)
    {
        if (exception.GetBaseException() is SqlException sqlException)
        {
            return sqlException.Errors.Cast<SqlError>().Any(error => error.Number == 2714);
        }

        return exception.Message.Contains("already an object named", StringComparison.OrdinalIgnoreCase);
    }

    private static void SeedRuntimeData(AppDbContext database)
    {
        var admin = database.Employees.SingleOrDefault(e => e.Username == "admin");
        if (admin is null)
        {
            database.Employees.Add(new Employee
            {
                Username = "admin",
                PasswordHash = PasswordHashService.HashStatic("Admin123!"),
                FullName = "System Administrator",
                Role = "Admin",
                CreatedAt = DateTime.Now,
                IsActive = true
            });
        }
        else
        {
            admin.PasswordHash = PasswordHashService.HashStatic("Admin123!");
            admin.FullName = string.IsNullOrWhiteSpace(admin.FullName) ? "System Administrator" : admin.FullName;
            admin.Role = string.IsNullOrWhiteSpace(admin.Role) ? "Admin" : admin.Role;
            admin.IsActive = true;
        }

        AddResidentIfMissing(database, "Ayse Demir", "10000000146", "A-101");
        AddResidentIfMissing(database, "Mehmet Yilmaz", "10000000214", "B-204");
        AddResidentIfMissing(database, "Elif Kaya", "10000000382", "C-310");

        database.SaveChanges();
    }

    private static void AddResidentIfMissing(AppDbContext database, string fullName, string identityNumber, string roomNumber)
    {
        if (!database.Residents.Any(r => r.IdentityNumber == identityNumber))
        {
            database.Residents.Add(new Resident
            {
                FullName = fullName,
                IdentityNumber = identityNumber,
                RoomNumber = roomNumber,
                CreatedAt = DateTime.Now,
                IsActive = true
            });
        }
    }

    private static bool SharedTablesAlreadyExist(AppDbContext database)
    {
        const string sql = """
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_NAME IN ('Employees', 'Visitors', 'Residents', 'Visits', 'BlacklistEntries')
""";
        using var command = database.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        if (command.Connection?.State != System.Data.ConnectionState.Open)
        {
            command.Connection?.Open();
        }

        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        services.AddScoped<PasswordHashService>();
        services.AddScoped<AuthService>();
        services.AddScoped<IdentityValidationService>();
        services.AddScoped<DurationService>();
        services.AddScoped<DataMaskingService>();
        services.AddScoped<ImageService>();
        services.AddScoped<VisitorService>();
        services.AddScoped<ResidentService>();
        services.AddScoped<BlacklistService>();
        services.AddScoped<VisitService>();
        services.AddSingleton<AppState>();

        services.AddTransient<LoginForm>();
        services.AddTransient<MainDashboardForm>();
        services.AddTransient<VisitorsForm>();
        services.AddTransient<VisitorEntryForm>();
        services.AddTransient<VisitorExitForm>();
        services.AddTransient<ActiveVisitorsForm>();
        services.AddTransient<VisitHistoryForm>();
        services.AddTransient<ResidentsForm>();
        services.AddTransient<BlacklistForm>();
        services.AddTransient<SettingsForm>();
    }
}
