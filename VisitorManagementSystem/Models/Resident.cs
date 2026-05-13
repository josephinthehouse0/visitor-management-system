namespace VisitorManagementSystem.Models;

public sealed class Resident
{
    public int ResidentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public string? RoomNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();

    public override string ToString() => $"{FullName} - {RoomNumber}";
}
