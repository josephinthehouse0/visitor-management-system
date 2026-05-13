using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services;

public sealed class VisitorService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IdentityValidationService _identityValidationService;

    public VisitorService(IDbContextFactory<AppDbContext> dbFactory, IdentityValidationService identityValidationService)
    {
        _dbFactory = dbFactory;
        _identityValidationService = identityValidationService;
    }

    public async Task<List<Visitor>> GetVisitorsAsync(string? search = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Visitors.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.FullName.Contains(search) || v.IdentityNumber.Contains(search));
        }

        return await query.OrderBy(v => v.FullName).ToListAsync();
    }

    public async Task<List<VisitorListItem>> GetVisitorListItemsAsync(string? search = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Visitors.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.FullName.Contains(search) || v.IdentityNumber.Contains(search));
        }

        var visitors = await query
            .OrderBy(v => v.FullName)
            .ToListAsync();

        var visitorIds = visitors.Select(v => v.VisitorId).ToList();
        var visitCounts = await db.Visits
            .Where(visit => visitorIds.Contains(visit.VisitorId))
            .GroupBy(visit => visit.VisitorId)
            .Select(group => new { VisitorId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.VisitorId, item => item.Count);

        return visitors.Select(v => new VisitorListItem
        {
            VisitorId = v.VisitorId,
            FullName = v.FullName,
            IdentityNumber = v.IdentityNumber,
            CreatedAt = v.CreatedAt,
            VisitCount = visitCounts.GetValueOrDefault(v.VisitorId)
        }).ToList();
    }

    public async Task<int> TotalVisitorsCountAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Visitors.CountAsync();
    }

    public async Task<Dictionary<string, Visitor>> GetVisitorLookupAsync()
    {
        var visitors = await GetVisitorsAsync();
        return visitors.ToDictionary(v => v.IdentityNumber, v => v);
    }

    public Task<Visitor?> FindByIdentityAsync(string identityNumber)
    {
        return FindByIdentityCoreAsync(identityNumber);
    }

    public async Task<(bool Success, string Message, Visitor? Visitor)> SaveAsync(Visitor visitor)
    {
        if (!_identityValidationService.TryValidate(visitor.IdentityNumber, out var message))
        {
            return (false, message, null);
        }

        if (string.IsNullOrWhiteSpace(visitor.FullName))
        {
            return (false, "Visitor full name is required.", null);
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var duplicate = await db.Visitors
            .AnyAsync(v => v.IdentityNumber == visitor.IdentityNumber && v.VisitorId != visitor.VisitorId);
        if (duplicate)
        {
            return (false, "A visitor with this identity number already exists.", null);
        }

        if (visitor.VisitorId == 0)
        {
            db.Visitors.Add(visitor);
        }
        else
        {
            db.Visitors.Update(visitor);
        }

        await db.SaveChangesAsync();
        return (true, "Visitor saved successfully.", visitor);
    }

    public async Task DeleteAsync(int visitorId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var visitor = await db.Visitors.FindAsync(visitorId);
        if (visitor is not null)
        {
            db.Visitors.Remove(visitor);
            await db.SaveChangesAsync();
        }
    }

    private async Task<Visitor?> FindByIdentityCoreAsync(string identityNumber)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Visitors.FirstOrDefaultAsync(v => v.IdentityNumber == identityNumber);
    }
}
