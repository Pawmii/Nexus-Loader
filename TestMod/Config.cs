using NexusLoader.Interfaces;

namespace TestMod;

public class Config : IModConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;
    public bool TestSetting1 { get; set; } = true;
    public string TestSetting1String { get; set; } = "Hello World!";
    public int IterationsNum { get; set; } = 1000;
}