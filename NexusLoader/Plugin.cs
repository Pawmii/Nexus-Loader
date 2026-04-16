using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;

namespace NexusLoader;

[BepInPlugin(PluginInfo.PluginGuid, PluginInfo.PluginName, PluginInfo.PluginVersionString)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; } = null!;
    public static ManualLogSource PluginLogger { get; private set; } = null!;
    public static NexusConfig NexusConfig { get; private set; } = null!;
    public static Translation Translation { get; private set; } = null!;

    private async void Awake()
    {
        Instance = this;
        
        PluginLogger = Logger;
        
        CheckDirs();
        
        if (!File.Exists(Paths.NexusConfigPath))
        {
            Logger.LogInfo("Nexus configuration file not found, looks like this is your first time using Nexus");
            
            NexusConfig = new NexusConfig();
            
            await File.WriteAllTextAsync(Paths.NexusConfigPath, Yaml.SerializeObject(NexusConfig));
            
            Translation = new Translation();
            
            await File.WriteAllTextAsync(Path.Combine(Paths.TranslationsPath, "en.yml"), Yaml.SerializeObject(Translation));
            
            Logger.LogInfo(
                "The configuration file was created automatically. You can customize it (path Processor Tycoon/Mods/config.json). You can specify the language and other useful settings there. The default language is English.");
            Logger.LogInfo("Thank you for using Nexus.");
        }
        else
        {
            var cfg = Yaml.DeserializeObject<NexusConfig>(File.ReadAllText(Paths.NexusConfigPath));
            
            if (cfg == null)
            {
                Logger.LogError("Invalid Nexus configuration file");
                return;
            }
            
            NexusConfig = cfg;
            
            string translationFile = Path.Combine(Paths.TranslationsPath, NexusConfig.Locale + ".yml");
            
            if (!File.Exists(translationFile))
                Logger.LogError($"Translation file {NexusConfig.Locale}.yml not found");
            else
            {
                var translation = Yaml.DeserializeObject<Translation>(File.ReadAllText(translationFile));

                if (translation == null)
                    Logger.LogError($"Invalid translation file ({NexusConfig.Locale}.yml)");
                else
                    Translation = translation;
            }
        }
        
        NexusLoader.Logger.IsDebugEnabled[Assembly.GetExecutingAssembly()] = NexusConfig.Debug;
        
        NexusLoader.Logger.LogInfoT("translation.welcome");
        NexusLoader.Logger.LogInfoT("translation.welcome_authors", Translation.Authors);
        NexusLoader.Logger.LogInfoT("translation.welcome_license", Translation.License);
        
        NexusLoader.Logger.LogInfoT("mod.loading");
        
        if (NexusConfig.AutoUpdate)
        {
            try
            {
                string? updateFileName = null;
            
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    updateFileName = "NexusUpdater.exe";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    updateFileName = "NexusUpdater";
                else
                {
                    NexusLoader.Logger.LogErrorT("general.invalid_os");
                }
            
                if (updateFileName != null)
                {
                    var latestVersion = await GetLatestVersionAsync("Pawmii", "Nexus-Loader");
                
                    if (latestVersion != null && latestVersion > PluginInfo.PluginVersion)
                    {
                        NexusLoader.Logger.LogInfoT("updater.found_new_version");
                    
                        Process.Start(Path.Combine(Paths.GamePath, updateFileName));
                    
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception e)
            {
                NexusLoader.Logger.LogErrorT("loader.cant_update", e);
            }
        }
        
        try
        {
            ModLoader.LoadAllMods();
        }
        catch (Exception e)
        {
            NexusLoader.Logger.LogErrorT("mod.load_error", e);
        }
    }
    
    private static async Task<Version?> GetLatestVersionAsync(string owner, string repo)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", $"NexusLoader/{PluginInfo.PluginVersionString}");

        var url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
        var response = await http.GetStringAsync(url);

        using var doc = JsonDocument.Parse(response);
        var tagName = doc.RootElement.GetProperty("tag_name").GetString();
    
        if (tagName != null && tagName.StartsWith("v"))
            return Version.Parse(tagName[1..]);

        return null;
    }

    private void OnDestroy()
    {
        NexusLoader.Logger.LogInfoT("mod.unloading");

        try
        {
            ModLoader.UnloadAllMods();
        }
        catch (Exception e)
        {
            NexusLoader.Logger.LogErrorT("mod.unload_error", e);
        }
    }

    private static void CheckDirs()
    {
        if (!Directory.Exists(Paths.NexusPath))
            Directory.CreateDirectory(Paths.NexusPath);
        
        if (!Directory.Exists(Paths.ModsPath))
            Directory.CreateDirectory(Paths.ModsPath);
        
        if (!Directory.Exists(Paths.ConfigsPath))
            Directory.CreateDirectory(Paths.ConfigsPath);
        
        if (!Directory.Exists(Paths.StoragesPath))
            Directory.CreateDirectory(Paths.StoragesPath);
        
        if (!Directory.Exists(Paths.TranslationsPath))
            Directory.CreateDirectory(Paths.TranslationsPath);
    }
}