using System.Text.Json;

namespace Neutrivox.LicenseServer;

public sealed class LicenseServerStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _path;

    public LicenseServerStore(IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration["NEUTRIVOX_DATA_PATH"] ?? Environment.GetEnvironmentVariable("NEUTRIVOX_DATA_PATH");
        _path = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(environment.ContentRootPath, "data", "license-server.json")
            : Path.GetFullPath(configured);
    }

    public async Task<T> ReadAsync<T>(Func<LicenseServerDatabase, T> action, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return action(await LoadAsync(cancellationToken)); }
        finally { _gate.Release(); }
    }

    public async Task<T> WriteAsync<T>(Func<LicenseServerDatabase, T> action, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var database = await LoadAsync(cancellationToken);
            var result = action(database);
            await SaveAsync(database, cancellationToken);
            return result;
        }
        finally { _gate.Release(); }
    }

    private async Task<LicenseServerDatabase> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path)) return new();
        await using var stream = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<LicenseServerDatabase>(stream, Options, cancellationToken) ?? new();
    }

    private async Task SaveAsync(LicenseServerDatabase database, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);
        var temporary = _path + ".tmp";
        await using (var stream = File.Create(temporary))
            await JsonSerializer.SerializeAsync(stream, database, Options, cancellationToken);
        File.Move(temporary, _path, true);
    }
}
