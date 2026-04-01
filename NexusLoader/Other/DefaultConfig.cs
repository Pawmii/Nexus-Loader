using NexusLoader.Interfaces;

namespace NexusLoader.Other;

public class DefaultConfig : IModConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;
}