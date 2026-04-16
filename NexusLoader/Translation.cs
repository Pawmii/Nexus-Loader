using System.Collections.Generic;

namespace NexusLoader;

public class Translation
{
    public string Authors { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;

    public Dictionary<string, string> TranslationDictionary { get; set; } = new Dictionary<string, string>();
}