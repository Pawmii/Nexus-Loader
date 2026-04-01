using System.Reflection;
using Newtonsoft.Json;
using NexusLoader.Extensions;
using NexusLoader.Interfaces;

namespace NexusLoader.Abstract;

public abstract class Mod<TConfig> : IMod where TConfig : IModConfig, new()
{
    public abstract string Name { get; }
    public string Prefix => Name.ToSnakeCase();
    public abstract string Author { get; }
    public virtual string? Description { get; } = null;
    public abstract string Version { get; }
    public virtual string? RequiredNexusVersion { get; } = null;
    
    public bool IsEnabled { get; private set; }
    
    public Assembly ModAssembly { get; private set; } = null!;
    public TConfig Config { get; private set; } = default!;
    public Database Database { get; private set; } = null!;
    
    public void Enable()
    {
        if (IsEnabled)
            return;
        
        ModAssembly = Assembly.GetExecutingAssembly();
        
        string configPath = Paths.GetModConfigPath(Prefix);
        
        if (!File.Exists(configPath))
        {
            Logger.LogInfoT("mod.config_not_found");
            
            Config = new TConfig();
            
            File.WriteAllText(configPath, Yaml.SerializeObject(Config));
        }
        else
        {
            var cfg = Yaml.DeserializeObject<TConfig>(File.ReadAllText(configPath));
            
            if (cfg == null)
            {
                Logger.LogErrorT("msg.invalid_config");
                return;
            }
            
            Config = cfg;
        }
        
        if (!Config.IsEnabled)
        {
            Config = default!;
            return;
        }
        
        Database = new Database(Paths.GetModStoragePath(Prefix));
        
        Logger.IsDebugEnabled[ModAssembly] = Config.Debug;
        
        OnEnabling();
        IsEnabled = true;
        Logger.LogInfoT("mod.enabled", Prefix, Author);
    }
    
    public void Disable()
    {
        if (!IsEnabled)
            return;
        
        OnDisabling();
        
        Logger.IsDebugEnabled.Remove(ModAssembly);
        
        ModAssembly = null!;
        Config = default!;
        Database = null!;
        
        IsEnabled = false;
        Logger.LogInfoT("mod.disabled", Prefix, Author);
    }
    
    public virtual void OnEnabling() { }
    
    public virtual void OnDisabling() { }
}