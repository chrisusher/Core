namespace ChrisUsher.Core.Shared.UI;

public sealed class ToastMessage
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public ToastLevel Level { get; set; }

    public int TimeoutMs { get; set; }

    public DateTime Timestamp { get; set; }
}
