namespace ChrisUsher.Core.Shared.Data;

public static class StaticData
{
    /// <summary>
    /// Ensures a DateTime has Kind=Utc, which is required by PostgreSQL with timestamp with time zone.
    /// If the DateTime has Kind=Unspecified, it treats it as UTC.
    /// </summary>
    public static DateTime SpecifyUtcKind(DateTime dateTime)
    {
        return dateTime.Kind is DateTimeKind.Utc
            ? dateTime
            : new DateTime(dateTime.Ticks, DateTimeKind.Utc);
    }
}
