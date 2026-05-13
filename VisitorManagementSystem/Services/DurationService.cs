namespace VisitorManagementSystem.Services;

public sealed class DurationService
{
    public int CalculateMinutes(DateTime entryTime, DateTime exitTime)
    {
        return Math.Max(0, (int)Math.Round((exitTime - entryTime).TotalMinutes));
    }

    public string FormatDuration(int? minutes)
    {
        if (minutes is null)
        {
            return "";
        }

        var total = Math.Max(0, minutes.Value);
        var days = total / 1440;
        var hours = total % 1440 / 60;
        var mins = total % 60;

        if (days > 0)
        {
            return $"{days} day{Plural(days)} {hours} hour{Plural(hours)} {mins} minute{Plural(mins)}";
        }

        if (hours > 0)
        {
            return $"{hours} hour{Plural(hours)} {mins} minute{Plural(mins)}";
        }

        return $"{mins} minute{Plural(mins)}";
    }

    private static string Plural(int value) => value == 1 ? "" : "s";
}
