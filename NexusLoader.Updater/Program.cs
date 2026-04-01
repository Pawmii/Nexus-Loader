using System.IO.Compression;
using System.Runtime.InteropServices;

namespace NexusLoader.Updater;

public class Program
{
    public const string BaseUrl = "https://github.com/Pawmii/Nexus-Loader";
    
    public static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Provide target version as first argument");
            Console.ReadKey();
            return;
        }
        
        string version = args[0];
        
        if (!version.StartsWith('v'))
            version = $"v{version}";
        
        Console.WriteLine("Running NexusLoader Updater v1.0.0");
        Console.WriteLine($"Downloading version: {version}");
        
        string platformString;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            platformString = "win";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            platformString = "linux";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            platformString = "mac";
        else
        {
            Console.WriteLine("You are using an unsupported OS, try building the release yourself");
            Console.ReadKey();
            return;
        }
        
        var cpuArch = RuntimeInformation.ProcessArchitecture;
        
        bool unsupportedArch = false;
        
        if (cpuArch is Architecture.Wasm or Architecture.S390x)
            unsupportedArch = true;
        else if (platformString == "win" && cpuArch == Architecture.Arm)
            unsupportedArch = true;
        else if (platformString == "linux" && cpuArch == Architecture.X86)
            unsupportedArch = true;
        else if (platformString == "mac" && cpuArch is Architecture.Arm or Architecture.X86)
            unsupportedArch = true;
        
        if (unsupportedArch)
        {
            Console.WriteLine("You are using an unsupported architecture, try building the release yourself");
            Console.ReadKey();
            return;
        }
        
        string fileName = $"NexusLoader-{platformString}-{cpuArch.ToString().ToLower()}";
        
        string targetUrl = $"{BaseUrl}/releases/download/{version}/{fileName}";
        
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "NexusUpdater/1.0.0");
        
        var request = new HttpRequestMessage(HttpMethod.Head, targetUrl);
        var response = await http.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"File not found: {response.StatusCode}");
            return;
        }
        
        string tempPath = Path.Combine(Path.GetTempPath(), "NexusLoader-tmp.zip");
        
        var data = await http.GetByteArrayAsync(targetUrl);
        await File.WriteAllBytesAsync(tempPath, data);
        
        Console.WriteLine($"Downloaded tmp ZIP path: {tempPath}");
        
        Console.WriteLine("Unpacking ZIP...");
        
        ZipFile.ExtractToDirectory(tempPath, Directory.GetCurrentDirectory(), true);
        
        Console.WriteLine("ZIP Unpacked, removing tmp ZIP...");
        
        File.Delete(tempPath);
        
        Console.WriteLine("Updated successfully!");
    }
}