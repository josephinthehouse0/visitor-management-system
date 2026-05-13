namespace VisitorManagementSystem.Services;

public sealed class DataMaskingService
{
    public string MaskIdentityNumber(string identityNumber)
    {
        if (string.IsNullOrWhiteSpace(identityNumber) || identityNumber.Length < 3)
        {
            return string.Empty;
        }

        return identityNumber[..2] + new string('*', identityNumber.Length - 3) + identityNumber[^1];
    }
}
