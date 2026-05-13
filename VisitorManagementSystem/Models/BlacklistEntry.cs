namespace VisitorManagementSystem.Models;

public sealed class BlacklistEntry
{
    public int BlacklistEntryId { get; set; }
    public int VisitorId { get; set; }
    public Visitor Visitor { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
}
