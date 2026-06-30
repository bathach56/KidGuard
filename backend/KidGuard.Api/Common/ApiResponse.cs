using System.Text.Json.Serialization;

namespace KidGuard.Api.Common;

public record ApiResponse<T>(
    bool Success,
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] T? Data,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Errors = null)
{
    public static ApiResponse<T> Ok(string message, T data)
    {
        return new ApiResponse<T>(true, message, data);
    }

    public static ApiResponse<T> Fail(string message, IReadOnlyList<string>? errors = null)
    {
        return new ApiResponse<T>(false, message, default, errors ?? Array.Empty<string>());
    }
}
