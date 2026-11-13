namespace CoreSolution.Shared.Constants;

public static class PhoneNumbers
{
    private const string InternationalPrefix = "+";

    public static string Normalize(string phone)
    {
        return InternationalPrefix + GetDigits(phone);
    }

    public static string GetDigits(string phone)
    {
        return new string(phone.Where(char.IsDigit).ToArray());
    }
}
