using System.IO.Compression;
using System.Reflection;
using Newtonsoft.Json;
using NexusLoader.Crypt;
using NexusLoader.Extensions;
using NexusLoader.Interfaces;
using NexusLoader.Models;

namespace NexusLoader;

public class ModData
{
    public ModData(Assembly asm, ModManifest manifest, IMod mod)
    {
        Assembly = asm;
        Manifest = manifest;
        Mod = mod;
    }
    
    public Assembly Assembly { get; set; }
    public ModManifest Manifest { get; set; }
    public IMod Mod { get; set; }
}

public static class ModLoader
{
    public static List<ModData> Mods { get; } = new();
    
    public static List<string> GetAllModFiles()
    {
        return Directory.EnumerateFiles(Paths.ModsPath, "*.nxmod").ToList();
    }
    
    public static void LoadAllMods()
    {
        List<string> modFiles = GetAllModFiles();
        
        Dictionary<string, byte[]> deps = new Dictionary<string, byte[]>();
        
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var name = new AssemblyName(args.Name).Name;
            var path = name + ".dll";
            
            if (deps.TryGetValue(path, out var bytes))
                return Assembly.Load(bytes);
            
            string managedPath = Path.Combine(BepInEx.Paths.ManagedPath, path);
            
            if (File.Exists(managedPath))
                return Assembly.Load(managedPath);
            
            string bepAssemblyPath = Path.Combine(BepInEx.Paths.BepInExAssemblyDirectory, path);
            
            if (File.Exists(bepAssemblyPath))
                return Assembly.Load(bepAssemblyPath);
            
            string bepPluginsPath = Path.Combine(BepInEx.Paths.PluginPath, path);
            
            if (File.Exists(bepPluginsPath))
                return Assembly.Load(bepPluginsPath);
            
            return null;
        };
        
        foreach (var modFile in modFiles)
        {
            try
            {
                using var archive = ZipFile.Open(modFile, ZipArchiveMode.Read);
                
                var manifestEntry = archive.GetEntry("manifest.json");
                
                if (manifestEntry == null)
                {
                    Logger.LogErrorT("mod.no_manifest", modFile);
                    continue;
                }
                
                string manifestData = manifestEntry.ReadAllText();
                
                var manifest = JsonConvert.DeserializeObject<ModManifest>(manifestData);
                
                if (manifest == null)
                {
                    Logger.LogErrorT("mod.invalid_metadata", modFile);
                    continue;
                }
                
                var dllEntry = archive.GetEntry(manifest.DllName);
                
                if (dllEntry == null)
                {
                    Logger.LogErrorT("mod.no_dll", modFile);
                    continue;
                }
                
                var dllData = dllEntry.ReadAllBytes();
                
                var realHash = SHA256.Hash(dllData);
                
                if (realHash != manifest.DllHash.Trim())
                {
                    Logger.LogErrorT("mod.invalid_main_hash", modFile, realHash, manifest.DllHash);
                    continue;
                }
                
                foreach (var dep in manifest.Dependencies)
                {
                    var depEntry = archive.GetEntry(Path.Combine("Dependencies", dep));
                    
                    if (depEntry == null)
                    {
                        Logger.LogErrorT("mod.no_dep", modFile, dep);
                        continue;
                    }
                    
                    var depHashEntry = archive.GetEntry(
                        Path.Combine("Dependencies", Path.ChangeExtension(dep, ".txt")));
                    
                    if (depHashEntry == null)
                    {
                        Logger.LogErrorT("mod.no_dep_hash", modFile, dep);
                        continue;
                    }
                    
                    var depBytes = depEntry.ReadAllBytes();
                    
                    var depHash = depHashEntry.ReadAllText().Trim();
                    var depRealHash = SHA256.Hash(depBytes);
                    
                    if (depRealHash != depHash)
                    {
                        Logger.LogErrorT("mod.invalid_dep_hash", modFile, dep, depRealHash, depHash);
                        continue;
                    }
                    
                    deps[dep] = depBytes;
                }
                
                var assembly = Assembly.Load(dllData);
                
                var mods = ReflectionHelper.FindAllChilds<IMod>(assembly).ToArray();
                
                foreach (var mod in mods)
                {
                    if (mod == null)
                    {
                        Logger.LogWarnT("mod.nested_mod_null");
                        continue;
                    }
                    
                    mod.Enable();
                    Mods.Add(new ModData(assembly, manifest, mod));
                }
            }
            catch (Exception e)
            {
                Logger.LogErrorT("mod.loading_error", modFile, e);
            }
        }
    }

    public static void UnloadAllMods()
    {
        foreach (var mod in Mods)
        {
            try
            {
                mod.Mod.Disable();
            }
            catch (Exception e)
            {
                Logger.LogErrorT("mod.unloading_error", e);
            }
        }
    }
}