namespace VisitorManagementSystem.Services;

public sealed class IdentityValidationService
{
    public bool TryValidate(string identityNumber, out string message)
    {
        identityNumber = identityNumber.Trim();
        if (identityNumber.Length != 11 || identityNumber.Any(c => !char.IsDigit(c)))
        {
            message = "Identity number must contain exactly 11 digits.";
            return false;
        }

        if (identityNumber.StartsWith("99", StringComparison.Ordinal))
        {
            message = "Foreign identity number accepted.";
            return true;
        }

        if (identityNumber[0] == '0')
        {
            message = "Turkish TC identity number cannot start with 0.";
            return false;
        }

        var digits = identityNumber.Select(c => c - '0').ToArray();
        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        var tenth = ((oddSum * 7) - evenSum) % 10;
        if (tenth < 0)
        {
            tenth += 10;
        }

        var eleventh = digits.Take(10).Sum() % 10;
        if (digits[9] != tenth || digits[10] != eleventh)
        {
            message = "Turkish TC identity checksum is invalid.";
            return false;
        }

        message = "Identity number is valid.";
        return true;
    }
}
