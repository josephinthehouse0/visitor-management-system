namespace VisitorManagementSystem.Models;

public sealed class VisitorListItem
{
    public int VisitorId { get; set; }
    public string ReservationNumber => $"VMS-{VisitorId:000000}";
    public string FullName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public int VisitCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
