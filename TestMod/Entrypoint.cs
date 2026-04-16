using System;
using NexusLoader;
using NexusLoader.Abstract;

namespace TestMod;

public class Entrypoint : Mod<Config>
{
    public override string Name => "TestMod";
    public override string Author => "NexusLoader, Pawmi";
    public override string Version => "1.0.0";

    public override void OnEnabling()
    {
        Logger.LogInfo($"Hello from {Name} by {Author} v{Version}");
        
        if (Config.TestSetting1)
        {
            Logger.LogInfo("TestSetting1 is enabled");
            Logger.LogInfo(Config.TestSetting1String);
        }
        
        long sum = 0;
        
        for (int i = 1; i < Config.IterationsNum + 1; i++)
            sum += DateTime.UtcNow.Ticks / i;
        
        Logger.LogInfo($"Sum: {sum}");
    }

    public override void OnDisabling()
    {
        Logger.LogInfo($"Bye from {Name} by {Author} v{Version}");
        
        if (Config.TestSetting1)
        {
            Logger.LogInfo("TestSetting1 is enabled");
            Logger.LogInfo(Config.TestSetting1String);
        }
        
        long sum = 0;
        
        for (int i = 1; i < Config.IterationsNum + 1; i++)
            sum += DateTime.UtcNow.Ticks / i;
        
        Logger.LogInfo($"Sum 2: {sum}");
    }
}