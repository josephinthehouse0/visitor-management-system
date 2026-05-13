using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services;

public sealed class AppState
{
    public Employee? CurrentEmployee { get; set; }
    public Stack<string> OperationHistory { get; } = new();
    public Queue<Visit> EntryQueue { get; } = new();

    public void Log(string operation)
    {
        OperationHistory.Push($"{DateTime.Now:g} - {operation}");
    }
}
