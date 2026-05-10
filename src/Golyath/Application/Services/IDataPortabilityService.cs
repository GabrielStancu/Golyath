namespace Golyath.Application.Services;

public interface IDataPortabilityService
{
    /// <summary>Serializes all user data to a JSON string. The caller handles file I/O.</summary>
    Task<string> ExportToJsonAsync();

    /// <summary>
    /// Reads the stream directly and merges data into the local DB.
    /// Preferred over the string overload — avoids any encoding conversion by the caller.
    /// </summary>
    Task<ImportResult> ImportFromStreamAsync(Stream stream);

    /// <summary>Parses the JSON string and merges data into the local DB.</summary>
    Task<ImportResult> ImportFromJsonAsync(string json);
}

public record ImportResult(bool Success, string Message, int ItemsImported = 0);
