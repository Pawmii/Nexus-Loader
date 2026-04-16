using System.IO;

namespace NexusLoader;

public static class Paths
{
    static Paths()
    {
        GamePath = BepInEx.Paths.GameRootPath;
        NexusPath = Path.Combine(BepInEx.Paths.GameRootPath, "Nexus");
        ModsPath = Path.Combine(NexusPath, "Mods");
        ConfigsPath = Path.Combine(NexusPath, "Configs");
        StoragesPath = Path.Combine(NexusPath, "Storages");
        TranslationsPath = Path.Combine(NexusPath, "Translations");
        NexusConfigPath = Path.Combine(NexusPath, "config.yml");
    }
    
    public static string GamePath { get; }
    public static string NexusPath { get; }
    public static string ModsPath { get; }
    public static string ConfigsPath { get; }
    public static string StoragesPath { get; }
    public static string TranslationsPath { get; }
    public static string NexusConfigPath { get; }
    
    
    public static string GetModConfigPath(string prefix) => Path.Combine(ConfigsPath, prefix, "config.yml");
    
    public static string GetModStoragePath(string prefix) => Path.Combine(StoragesPath, prefix, "storage.dat");
}