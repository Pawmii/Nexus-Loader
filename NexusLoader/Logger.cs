using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;

namespace NexusLoader;

public static class Logger
{
    public static Dictionary<Assembly, bool> IsDebugEnabled { get; } = new();

    public static void Log(LogLevel level, object message)
        => Plugin.PluginLogger.Log(level, message);

    public static void LogFormated(LogLevel level, string message)
        => Log(level, message);
    
    public static void LogFormatedT(LogLevel level, string key, params object[] format)
    {
        string message = key;

        if (Plugin.Translation.TranslationDictionary.TryGetValue(key, out var translation))
            message = string.Format(translation, format);

        Log(level, message);
    }
    
    public static void LogMessage(string message, params object[] format)
        => LogFormated(LogLevel.Message, string.Format(message, format));
    
    public static void LogMessageT(string message, params object[] format)
        => LogFormatedT(LogLevel.Message, message, format);

    public static void LogInfo(string message, params object[] format)
        => LogFormated(LogLevel.Info, string.Format(message, format));
    
    public static void LogInfoT(string message, params object[] format)
        => LogFormatedT(LogLevel.Info, message, format);
    
    public static void LogDebug(string message, params object[] format)
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        if (!IsDebugEnabled[asm])
            return;
        
        LogFormated(LogLevel.Debug, string.Format(message, format));
    }
    public static void LogDebugT(string message, params object[] format)
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        if (!IsDebugEnabled[asm])
            return;
        
        LogFormatedT(LogLevel.Debug, message, format);
    }
    
    public static void LogWarn(string message, params object[] format)
        => LogFormated(LogLevel.Warning, string.Format(message, format));
    
    public static void LogWarnT(string message, params object[] format)
        => LogFormatedT(LogLevel.Warning, message, format);
    
    public static void LogError(string message, params object[] format)
        => LogFormated(LogLevel.Error, string.Format(message, format));
    
    public static void LogErrorT(string message, params object[] format)
        => LogFormatedT(LogLevel.Error, message, format);
    
    public static void LogFatal(string message, params object[] format)
        => LogFormated(LogLevel.Fatal, string.Format(message, format));
    
    public static void LogFatalT(string message, params object[] format)
        => LogFormatedT(LogLevel.Fatal, message, format);
}