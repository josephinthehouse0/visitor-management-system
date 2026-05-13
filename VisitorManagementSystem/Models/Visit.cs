namespace VisitorManagementSystem.Models;

public sealed class Visit
{
    public int VisitId { get; set; }
    public int VisitorId { get; set; }
    public Visitor Visitor { get; set; } = null!;
    public int ResidentId { get; set; }
    public Resident Resident { get; set; } = null!;
    public string VisitReason { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public int? DurationMinutes { get; set; }
    public VisitStatus Status { get; set; } = VisitStatus.Active;
}
