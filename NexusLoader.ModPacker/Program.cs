using System;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using NexusLoader.Models;
using NexusLoader.ModPacker.Crypt;

namespace NexusLoader.ModPacker;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Provide mod .dll path as first argument");
            return;
        }

        string modDllPath = args[0];

        if (!File.Exists(modDllPath))
        {
            Console.WriteLine("Mod .dll not found");
            return;
        }
        
        if (!Directory.Exists("Dependencies"))
            Directory.CreateDirectory("Dependencies");
        
        if (!Directory.Exists("Assets"))
            Directory.CreateDirectory("Assets");
        
        byte[] modDllBytes = File.ReadAllBytes(modDllPath);
        
        string modDllHash = SHA256.Hash(modDllBytes);
        
        ModManifest manifest = new ModManifest();
        
        manifest.DllName = Path.GetFileName(modDllPath);
        manifest.DllHash = modDllHash;
        
        List<string> dependencies = Directory.EnumerateFiles("Dependencies", "*.dll").Select(Path.GetFileName).ToList()!;
        List<string> assets = Directory.EnumerateFiles("Assets").Select(Path.GetFileName).ToList()!;
        
        manifest.Dependencies = dependencies;
        manifest.Assets = assets;
        
        using var zipStream = new FileStream(Path.ChangeExtension(modDllPath, ".nxmod"), FileMode.Create);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
        
        byte[] manifestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(manifest));
        var manifestEntry = archive.CreateEntry("manifest.json");
        
        using (var manifestEntryStream = manifestEntry.Open())
        {
            manifestEntryStream.Write(manifestData, 0, manifestData.Length);
        }
        
        foreach (var dependency in dependencies)
        {
            byte[] dependencyBytes = File.ReadAllBytes(dependency);
            string dependencyHash = SHA256.Hash(dependencyBytes);
            
            byte[] dependencyHashData = Encoding.UTF8.GetBytes(dependencyHash);
            
            string dependencyName = Path.GetFileName(dependency);
            
            archive.CreateEntryFromFile(dependency, Path.Combine("Dependencies", dependencyName));
            
            var dependencyHashEntry = archive.CreateEntry(Path.Combine("Dependencies",
                Path.ChangeExtension(dependencyName, ".txt")));
            
            using var dependencyHashEntryStream = dependencyHashEntry.Open();
            
            dependencyHashEntryStream.Write(dependencyHashData, 0, dependencyHashData.Length);
        }
        
        foreach (var asset in assets)
        {
            string assetName = Path.GetFileName(asset);
            
            archive.CreateEntryFromFile(asset, Path.Combine("Assets", assetName));
        }
    }
}