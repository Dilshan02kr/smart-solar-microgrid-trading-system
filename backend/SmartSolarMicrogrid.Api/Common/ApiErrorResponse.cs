// Defines the structured error payload returned consistently by API endpoints.
namespace SmartSolarMicrogrid.Api.Common;

public sealed record ApiErrorResponse(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Errors = null);
