using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services;

public sealed class AuthService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly PasswordHashService _passwordHashService;

    public AuthService(IDbContextFactory<AppDbContext> dbFactory, PasswordHashService passwordHashService)
    {
        _dbFactory = dbFactory;
        _passwordHashService = passwordHashService;
    }

    public async Task<Employee?> LoginAsync(string username, string password)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var employee = await db.Employees.SingleOrDefaultAsync(e => e.Username == username && e.IsActive);
        if (employee is null || !_passwordHashService.Verify(password, employee.PasswordHash))
        {
            return null;
        }

        return employee;
    }
}
