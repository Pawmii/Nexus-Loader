using System.Reflection;

namespace NexusLoader;

public static class ReflectionHelper
{
    private const BindingFlags InstanceFlags = BindingFlags.Public |
                                       BindingFlags.NonPublic |
                                       BindingFlags.Instance;
    
    private const BindingFlags StaticFlags = BindingFlags.Public |
                                             BindingFlags.NonPublic |
                                             BindingFlags.Static;
    
    public static IEnumerable<T?> FindAll<T>(Assembly assembly) where T : class
    {
        Type[] types = assembly.GetTypes();
        
        Type targetType = typeof(T);
        
        return types.Where(t => t == targetType).Select(t => t as T);
    }
    
    public static IEnumerable<T?> FindAllChilds<T>(Assembly assembly) where T : class
    {
        Type[] types = assembly.GetTypes();
        
        Type baseType = typeof(T);
        
        return types.Where(t => !t.IsInterface && !t.IsInterface && baseType.IsAssignableFrom(t)).Select(t => t as T);
    }
    
    public static void SetFieldValue<TValue, TTarget>(string fieldName, TValue value, TTarget instance)
    {
        FieldInfo? info = typeof(TTarget).GetField(fieldName, InstanceFlags);
        
        if (info == null)
            throw new FieldAccessException($"Field {fieldName} (instance) does not exist in {typeof(TTarget)}");
        
        info.SetValue(instance, value);
    }
    
    public static void SetStaticFieldValue<TValue, TTarget>(string fieldName, TValue value)
    {
        FieldInfo? info = typeof(TTarget).GetField(fieldName, StaticFlags);
        
        if (info == null)
            throw new FieldAccessException($"Field {fieldName} (static) does not exist in {typeof(TTarget)}");
        
        info.SetValue(null, value);
    }
    
    public static TValue? GetFieldValue<TValue, TTarget>(string fieldName, TTarget instance) where TValue : class
    {
        FieldInfo? info = typeof(TTarget).GetField(fieldName, InstanceFlags);
        
        if (info == null)
            throw new FieldAccessException($"Field {fieldName} (instance) does not exist in {typeof(TTarget)}");
        
        return info.GetValue(instance) as TValue;
    }
    
    public static TValue? GetStaticFieldValue<TValue, TTarget>(string fieldName) where TValue : class
    {
        FieldInfo? info = typeof(TTarget).GetField(fieldName, StaticFlags);
        
        if (info == null)
            throw new FieldAccessException($"Field {fieldName} (static) does not exist in {typeof(TTarget)}");
        
        return info.GetValue(null) as TValue;
    }
}