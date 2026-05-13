using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services;

public sealed class VisitService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly BlacklistService _blacklistService;
    private readonly DurationService _durationService;
    private readonly AppState _appState;

    public VisitService(IDbContextFactory<AppDbContext> dbFactory, BlacklistService blacklistService, DurationService durationService, AppState appState)
    {
        _dbFactory = dbFactory;
        _blacklistService = blacklistService;
        _durationService = durationService;
        _appState = appState;
    }

    public async Task<List<Visit>> GetActiveVisitsAsync(string? search = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Visits.Include(v => v.Visitor).Include(v => v.Resident)
            .Where(v => v.Status == VisitStatus.Active)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.Visitor.FullName.Contains(search) || v.Visitor.IdentityNumber.Contains(search) || v.Resident.FullName.Contains(search));
        }

        return await query.OrderByDescending(v => v.EntryTime).ToListAsync();
    }

    public async Task<List<Visit>> GetCompletedVisitsAsync(string? search = null, DateTime? date = null, int? residentId = null, int? visitorId = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Visits.Include(v => v.Visitor).Include(v => v.Resident)
            .Where(v => v.Status == VisitStatus.Completed)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.Visitor.FullName.Contains(search) || v.Visitor.IdentityNumber.Contains(search) || v.Resident.FullName.Contains(search) || v.VisitReason.Contains(search));
        }

        if (date.HasValue)
        {
            var day = date.Value.Date;
            query = query.Where(v => v.EntryTime >= day && v.EntryTime < day.AddDays(1));
        }

        if (residentId.HasValue && residentId.Value > 0)
        {
            query = query.Where(v => v.ResidentId == residentId);
        }

        if (visitorId.HasValue && visitorId.Value > 0)
        {
            query = query.Where(v => v.VisitorId == visitorId);
        }

        return await query.OrderByDescending(v => v.EntryTime).ToListAsync();
    }

    public async Task<int> TodayVisitsCountAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        return await db.Visits.CountAsync(v => v.EntryTime >= today && v.EntryTime < today.AddDays(1));
    }

    public async Task<int> ActiveVisitsCountAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Visits.CountAsync(v => v.Status == VisitStatus.Active);
    }

    public async Task<(bool Success, string Message, Visit? Visit)> SignInAsync(Visitor visitor, int residentId, string reason)
    {
        if (await _blacklistService.IsBlacklistedAsync(visitor.VisitorId))
        {
            return (false, "Blacklisted visitors cannot be signed in.", null);
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        if (await db.Visits.AnyAsync(v => v.VisitorId == visitor.VisitorId && v.Status == VisitStatus.Active))
        {
            return (false, "This visitor already has an active visit. Please sign them out first or verify whether they are still inside.", null);
        }

        if (residentId <= 0 || string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Resident and visit reason are required.", null);
        }

        var visit = new Visit
        {
            VisitorId = visitor.VisitorId,
            ResidentId = residentId,
            VisitReason = reason.Trim(),
            EntryTime = DateTime.Now,
            Status = VisitStatus.Active
        };

        db.Visits.Add(visit);
        await db.SaveChangesAsync();
        _appState.EntryQueue.Enqueue(visit);
        _appState.Log($"Signed in visitor {visitor.FullName}");
        return (true, "Visitor signed in successfully.", visit);
    }

    public async Task<(bool Success, string Message)> SignOutAsync(int visitId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var visit = await db.Visits.FindAsync(visitId);
        if (visit is null || visit.Status != VisitStatus.Active)
        {
            return (false, "Active visit could not be found.");
        }

        visit.ExitTime = DateTime.Now;
        visit.DurationMinutes = _durationService.CalculateMinutes(visit.EntryTime, visit.ExitTime.Value);
        visit.Status = VisitStatus.Completed;
        await db.SaveChangesAsync();
        _appState.Log($"Signed out visit #{visit.VisitId}");
        return (true, "Visitor signed out successfully.");
    }
}
