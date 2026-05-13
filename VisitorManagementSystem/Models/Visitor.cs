namespace VisitorManagementSystem.Models;

public sealed class Visitor
{
    public int VisitorId { get; set; }
    public string IdentityNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();

    public override string ToString() => $"{FullName} ({IdentityNumber})";
}
