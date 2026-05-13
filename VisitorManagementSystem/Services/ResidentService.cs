using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services;

public sealed class ResidentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IdentityValidationService _identityValidationService;

    public ResidentService(IDbContextFactory<AppDbContext> dbFactory, IdentityValidationService identityValidationService)
    {
        _dbFactory = dbFactory;
        _identityValidationService = identityValidationService;
    }

    public async Task<List<Resident>> GetResidentsAsync(string? search = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Residents.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.FullName.Contains(search) || r.IdentityNumber.Contains(search) || (r.RoomNumber != null && r.RoomNumber.Contains(search)));
        }

        return await query.OrderBy(r => r.FullName).ToListAsync();
    }

    public async Task<int> TotalResidentsCountAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Residents.CountAsync();
    }

    public async Task<(bool Success, string Message)> SaveAsync(Resident resident)
    {
        if (!_identityValidationService.TryValidate(resident.IdentityNumber, out var message))
        {
            return (false, message);
        }

        if (string.IsNullOrWhiteSpace(resident.FullName) || string.IsNullOrWhiteSpace(resident.RoomNumber))
        {
            return (false, "Resident full name and room number are required.");
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var duplicate = await db.Residents.AnyAsync(r => r.IdentityNumber == resident.IdentityNumber && r.ResidentId != resident.ResidentId);
        if (duplicate)
        {
            return (false, "A resident with this identity number already exists.");
        }

        if (resident.ResidentId == 0)
        {
            db.Residents.Add(resident);
        }
        else
        {
            db.Residents.Update(resident);
        }

        await db.SaveChangesAsync();
        return (true, "Resident saved successfully.");
    }

    public async Task DeleteAsync(int residentId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var resident = await db.Residents.FindAsync(residentId);
        if (resident is not null)
        {
            db.Residents.Remove(resident);
            await db.SaveChangesAsync();
        }
    }
}
