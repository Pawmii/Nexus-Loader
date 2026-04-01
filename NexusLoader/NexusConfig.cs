namespace NexusLoader;

public class NexusConfig
{
    public string Locale { get; set; } = "en";
    public bool Debug { get; set; } = false;
    public bool AutoUpdate { get; set; } = true;
    public bool LoadOutdatedMods { get; set; } = false;
    public bool LoadUpToDateMods { get; set; } = false;
}