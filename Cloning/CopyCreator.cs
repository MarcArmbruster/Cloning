namespace DeepCloneUtility.Cloning;

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
    public static T? DeepCloneViaJson<T>(this T obj)
    {
        var json = JsonSerializer.Serialize(obj, typeof(T), new JsonSerializerOptions
        {
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            IncludeFields = true,
            MaxDepth = 64,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict
        });
        return JsonSerializer.Deserialize<T>(json)!;
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
            return default;

        var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
        return (T)CloneInternal(source!, visited);
    }

    private static object? CloneInternal(object? source, IDictionary<object, object> visited)
    {
        if (source is null)
            return null;

        var type = source.GetType();

        // Immutable or primitive types
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
            return source;

        if (visited.TryGetValue(source, out var existing))
            return existing;

        // Arrays
        if (type.IsArray)
        {
            var array = (Array)source;
            var elementType = type.GetElementType()!;
            var clone = Array.CreateInstance(elementType, array.Length);
            visited[source] = clone;

            for (int i = 0; i < array.Length; i++)
                clone.SetValue(CloneInternal(array.GetValue(i), visited), i);

            return clone;
        }

        // IList (e.g. List<T>)
        if (typeof(IList).IsAssignableFrom(type))
        {
            var listClone = (IList)Activator.CreateInstance(type)!;
            visited[source] = listClone;
            foreach (var item in (IList)source)
                listClone.Add(CloneInternal(item, visited));
            return listClone;
        }

        // IDictionary (e.g. Dictionary<K,V>)
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            var dictClone = (IDictionary)Activator.CreateInstance(type)!;
            visited[source] = dictClone;
            foreach (DictionaryEntry entry in (IDictionary)source)
                dictClone.Add(CloneInternal(entry.Key, visited), CloneInternal(entry.Value, visited));
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
