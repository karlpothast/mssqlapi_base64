using System.ComponentModel;

namespace mssqlapi.Models;

public record SqlRequest(
    [property: DefaultValue("")]
    string SqlBase64,

    [property: DefaultValue("html")]
    string Format,

    [property: DefaultValue("")]
    string? Database
);

public record SqlResponse(
    string DataBase64,
    string ContentType
);

public class ApiError
{
    public string? Message { get; set; }
    public string? Code { get; set; }
    public string? Details { get; set; }
}

// ✅ ADD THIS
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ApiError? Error { get; set; }
}