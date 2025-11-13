namespace DeepCloneUtility.Cloning;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

/// <summary>
/// Provides extension methods for deep cloning any object graph.
/// Supports cyclic references, arrays, and common collection types.
/// </summary>
public static class CopyCreator
{
    /// <summary>
    /// Deep clone an object using Newtonsoft.Json, including read-only properties.
    /// </summary>
    public static T? DeepCloneViaNsJson<T>(this T? obj)
    {
        if (obj == null)
            return default!;

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new IncludeReadOnlyResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        var json = JsonConvert.SerializeObject(obj, settings);
        return JsonConvert.DeserializeObject<T>(json, settings)!;
    }

    /// <summary>
    /// Custom resolver that includes read-only properties and private fields.
    /// </summary>
    private class IncludeReadOnlyResolver : DefaultContractResolver
    {
        protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(
            System.Reflection.MemberInfo member, 
            MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            // Include read-only properties
            if (!prop.Writable)
            {
                var property = member as System.Reflection.PropertyInfo;
                if (property?.GetSetMethod(true) != null)
                    prop.Writable = true;
            }

            // Include private fields if needed
            if (!prop.Readable)
            {
                var field = member as System.Reflection.FieldInfo;
                if (field != null)
                    prop.Readable = true;
            }

            return prop;
        }
    }

    public static T? DeepCloneViaMsJson<T>(this T obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj, typeof(T), new JsonSerializerOptions
        {
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            IncludeFields = true,
            MaxDepth = 64,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict
        });
        return System.Text.Json.JsonSerializer.Deserialize<T>(json)!;
    }

    /// <summary>
    /// Creates a deep clone of the specified object.
    /// </summary>
    /// <typeparam name="T">Type of the object to clone.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <returns>A deep clone of <paramref name="source"/>.</returns>
    public static T? DeepClone<T>(this T? source)
    {
        if (source == null)
        {
            return default;
        }

        var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
        return (T?)CloneInternal(source!, visited);
    }

    private static object? CloneInternal(object? source, IDictionary<object, object> visited)
    {
        if (source is null)
        {
            return null;
        }

        var type = source.GetType();

        // Immutable or primitive types
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
        {
            return source;
        }

        if (visited.TryGetValue(source, out var existing))
        {
            return existing;
        }

        // Arrays
        if (type.IsArray)
        {
            var array = (Array)source;
            var elementType = type.GetElementType()!;
            var clone = Array.CreateInstance(elementType, array.Length);
            visited[source] = clone;

            for (int i = 0; i < array.Length; i++)
            {
                clone.SetValue(CloneInternal(array.GetValue(i), visited), i);
            }

            return clone;
        }

        // IList (e.g. List<T>)
        if (typeof(IList).IsAssignableFrom(type))
        {
            var listClone = (IList)Activator.CreateInstance(type)!;
            visited[source] = listClone;
            foreach (var item in (IList)source)
            {
                listClone.Add(CloneInternal(item, visited));
            }

            return listClone;
        }

        // IDictionary (e.g. Dictionary<K,V>)
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            var dictClone = (IDictionary)Activator.CreateInstance(type)!;
            visited[source] = dictClone;
            foreach (DictionaryEntry entry in (IDictionary)source)
            {
                dictClone.Add(CloneInternal(entry.Key, visited), CloneInternal(entry.Value, visited));
            }
            
            return dictClone;
        }

        // Complex types
        var cloneObj = Activator.CreateInstance(type)!;
        visited[source] = cloneObj;

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var value = field.GetValue(source);
            var clonedValue = CloneInternal(value, visited);
            field.SetValue(cloneObj, clonedValue);
        }

        return cloneObj;
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
