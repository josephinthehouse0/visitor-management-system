using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services;

public sealed class BlacklistService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public BlacklistService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<bool> IsBlacklistedAsync(int visitorId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.BlacklistEntries.AnyAsync(b => b.VisitorId == visitorId && b.IsActive);
    }

    public async Task<List<BlacklistEntry>> GetEntriesAsync(string? search = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.BlacklistEntries.Include(b => b.Visitor).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b => b.Visitor.FullName.Contains(search) || b.Visitor.IdentityNumber.Contains(search) || b.Reason.Contains(search));
        }

        return await query.OrderByDescending(b => b.AddedAt).ToListAsync();
    }

    public async Task<(bool Success, string Message)> AddAsync(Visitor visitor, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Blacklist reason is required.");
        }

        if (await IsBlacklistedAsync(visitor.VisitorId))
        {
            return (false, "This visitor is already actively blacklisted.");
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.BlacklistEntries.Add(new BlacklistEntry { VisitorId = visitor.VisitorId, Reason = reason.Trim(), AddedAt = DateTime.Now, IsActive = true });
        await db.SaveChangesAsync();
        return (true, "Visitor added to blacklist.");
    }

    public async Task RemoveAsync(int blacklistEntryId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entry = await db.BlacklistEntries.FindAsync(blacklistEntryId);
        if (entry is not null)
        {
            entry.IsActive = false;
            await db.SaveChangesAsync();
        }
    }
}
