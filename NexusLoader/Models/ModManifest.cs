using System.Collections.Generic;

namespace NexusLoader.Models;

public class ModManifest
{
    public string DllName { get; set; }
    public string DllHash { get; set; }
    public List<string> Assets { get; set; }
    public List<string> Dependencies { get; set; }
}