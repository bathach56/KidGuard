using System.Text.Json.Serialization;

namespace KidGuard.Agent.Infrastructure;

public sealed record CreatePairCodeRequest(
    string DeviceName,
    string ComputerName,
    string AgentVersion);

public sealed record PairCodeData(
    string PairCode,
    int ExpiresIn);

public sealed record HeartbeatRequest(
    string Status,
    string AgentVersion);

public sealed record HeartbeatData(
    int NextHeartbeat);

public sealed record ModeData(
    string Mode,
    DateTimeOffset UpdatedAt);

public sealed record UploadLogRequest(
    string ProcessName,
    string Action,
    string Mode,
    string Message);

[JsonSerializable(typeof(ApiResponse<PairCodeData>))]
[JsonSerializable(typeof(ApiResponse<HeartbeatData>))]
[JsonSerializable(typeof(ApiResponse<ModeData>))]
[JsonSerializable(typeof(ApiResponse<object>))]
[JsonSerializable(typeof(CreatePairCodeRequest))]
[JsonSerializable(typeof(HeartbeatRequest))]
[JsonSerializable(typeof(UploadLogRequest))]
internal sealed partial class BackendApiJsonContext : JsonSerializerContext;
