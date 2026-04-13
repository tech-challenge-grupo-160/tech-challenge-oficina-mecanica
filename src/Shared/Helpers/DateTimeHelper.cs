namespace Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

public static class DateTimeHelper
{
    private static readonly TimeSpan BrazilOffset = TimeSpan.FromHours(-3);

    public static DateTime UTCBrazilNow()
    {
        var local = DateTime.UtcNow + BrazilOffset;
        return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
    }

    public static DateTime UTCToBrazil(DateTime utcDateTime)
    {
        var utc = utcDateTime.Kind == DateTimeKind.Utc ? utcDateTime : utcDateTime.ToUniversalTime();
        var local = utc + BrazilOffset;
        return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
    }
}
