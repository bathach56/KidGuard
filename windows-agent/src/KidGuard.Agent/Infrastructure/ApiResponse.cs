namespace KidGuard.Agent.Infrastructure;

public sealed class ApiResponse<TData>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public TData? Data { get; set; }

    public List<string> Errors { get; set; } = [];
}
