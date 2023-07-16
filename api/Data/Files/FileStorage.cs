namespace Planera.Data.Files;

class FileStorage : IFileStorage
{
    private readonly string _baseDirectory;

    public FileStorage(IConfiguration configuration)
    {
        _baseDirectory = configuration["FileStorage:Path"]!;
    }

    public void CreateDirectory(string name)
    {
        Directory.CreateDirectory(Path.Combine(_baseDirectory, name));
    }

    public async Task<byte[]?> ReadAsync(string directory, string name, string? param = null)
    {
        if (param != null)
            name = $"{name}:{param}";

        var path = Path.Combine(_baseDirectory, directory, name);
        if (!File.Exists(path))
            return null;

        return await File.ReadAllBytesAsync(path);
    }

    public void Delete(string path, string? param = null)
    {
        if (param != null)
            path += $":{param}";

        File.Delete(Path.Combine(_baseDirectory, path));
    }

    public void DeleteDirectory(string name)
    {
        Directory.Delete(Path.Combine(_baseDirectory, name));
    }

    public async Task<string> WriteAsync(
        string directory,
        byte[] bytes,
        string? param = null)
    {
        string name;
        string path;
        do
        {
            name = Guid.NewGuid().ToString();
            path = Path.Combine(
                _baseDirectory,
                directory,
                param == null ? name : $"{name}:{param}"
            );
        }
        while (File.Exists(path));

        await File.WriteAllBytesAsync(path, bytes);

        return Path.Combine(directory, name);
    }

    public async Task<string> WriteManyAsync(
        string directory,
        params (byte[] bytes, string identifier)[] values)
    {
        var firstValue = values.First();
        var name = await WriteAsync(directory, firstValue.bytes, firstValue.identifier);
        foreach (var (bytes, identifier) in values.Skip(1))
        {
            var path = Path.Combine(_baseDirectory, name);
            await File.WriteAllBytesAsync($"{path}:{identifier}", bytes);
        }

        return name;
    }
}