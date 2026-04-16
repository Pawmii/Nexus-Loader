using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NexusLoader;

public static class Yaml
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    
    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithQuotingNecessaryStrings()
        .Build();
    
    public static T? DeserializeObject<T>(string yaml)
    {
        try
        {
            return Deserializer.Deserialize<T>(yaml.Trim());
        }
        catch (Exception e)
        {
            Logger.LogError($"YAML Deserialization error: {e}");
            return default;
        }
    }
    
    public static string SerializeObject<T>(T obj)
    {
        try
        {
            return Serializer.Serialize(obj);
        }
        catch(Exception e)
        {
            Logger.LogDebug($"YAML Serialization error: {e}");
            return string.Empty;
        }
    }
}