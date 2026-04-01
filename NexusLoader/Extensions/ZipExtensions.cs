using System.IO.Compression;

namespace NexusLoader.Extensions;

public static class ZipExtensions
{
    public static string ReadAllText(this ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        
        string text = reader.ReadToEnd();
        
        return text;
    }

    public static byte[] ReadAllBytes(this ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        
        byte[] data = new byte[entry.Length];
        _ = stream.Read(data, 0, data.Length);
        
        return data;
    }
}