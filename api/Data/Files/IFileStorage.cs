namespace Planera.Data.Files;

public interface IFileStorage
{
    public void CreateDirectory(string name);

    public Task<byte[]?> ReadAsync(string directory, string name, string? param = null);

    public void Delete(string path, string? param = null);

    public void DeleteDirectory(string name);

    public Task<string> WriteAsync(string directory, byte[] bytes, string? identifier = null);

    public Task<string> WriteManyAsync(string directory, params (byte[] bytes, string identifier)[] bytes);
}