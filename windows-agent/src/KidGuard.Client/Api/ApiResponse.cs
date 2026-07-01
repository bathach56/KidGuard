using System.Text.Json.Serialization;

namespace KidGuard.Client.Api;

public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] T? Data,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Errors);
