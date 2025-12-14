namespace DeepCloneUtility.Cloning;

using global::Cloning.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides extension methods for deep cloning any object graph.
/// Supports cyclic references, arrays, and common collection types.
/// </summary>
public class DeepClone<T> //where T : class
{
    private T? sourceInstance;
    private Dictionary<Type, object[]> ctorParameters = [];

    private Dictionary<Type, Func<object?, object?>> customLogic = [];

    private DeepClone()
    {
    }

    [DebuggerStepThrough]
    public static DeepClone<T> Builder()
    {
        return new DeepClone<T>();
    }

    [DebuggerStepThrough]
    public DeepClone<T> WithSourceInstance(T source)
    {
        this.sourceInstance = source;
        return this;
    }

    [DebuggerStepThrough]
    public DeepClone<T> UseCustomLogic(Type type, Func<object?, object?> individualCloneLogic)
    {
        if (individualCloneLogic != null)
        {
            customLogic[type] = individualCloneLogic;
        }

        return this;
    }

    [DebuggerStepThrough]
    public DeepClone<T> UseCtorParameters(Type parameterType, object[] parameterDefaultValues)
    {
        this.ctorParameters[parameterType] = parameterDefaultValues;
        return this;
    }

    /// <summary>
    /// Creates a deep clone of the specified object.
    /// </summary>
    /// <typeparam name="T">Type of the object to clone.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <returns>A deep clone of <paramref name="source"/>.</returns>
    public T? CreateClone()
    {
        if (this.sourceInstance == null)
        {
            return default;
        }

        var alreadyCloned = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
        try
        {
            var clone = (T?)this.CloneInternal(this.sourceInstance, alreadyCloned);
            return clone;
        }
        catch (MissingMethodException)
        {
            throw;
        }
    }

    private object? CloneInternal(object? source, IDictionary<object, object> visited)
    {
        if (source is null)
        {
            return null;
        }

        var type = source.GetType();

        // use custom logic if provided
        if (TryUseCostomLogic(type, source, visited, out var custClone)) return custClone;

        // Immutable or primitive types
        if (TryClonePrimitiveTypes(type, source, out var primitiveClone))
        {
            return source;
        }

        // Skip already cloned objects to handle cyclic references
        if (visited.TryGetValue(source, out var existing))
        {
            return existing;
        }

        // Arrays
        if (TryCloneArrays(type, source, visited, out var arrayClone)) return arrayClone;

        // IList (e.g. List<T>)
        if (TryCloneList(type, source, visited, out var listClone)) return listClone;

        // ConcurrentBag<T> (e.g. ConcurrentBag<string>)
        if (TryCloneConcurrentBag(type, source, visited, out var bagClone)) return bagClone;

        // IDictionary (e.g. Dictionary<K,V>)
        if (TryCloneDictionary(type, source, visited, out var dictClone)) return dictClone;

        // Complex types
        if (TryCloneComplexObject(type, source, visited, out var objClone)) return objClone;
        
        throw new NotSupportedException($"Type '{type.FullName}' is not supported for deep cloning.");        
    }

    private bool TryUseCostomLogic(Type type, object? source, IDictionary<object, object> visited, out object? clone)
    {
        if (customLogic.TryGetValue(type, out var cloneLogic))
        {
            object? customClone = cloneLogic.Invoke(source);
            if (customClone != null && source != null)
            {
                visited[source] = customClone;
            }

            clone = customClone;
            return true;
        }

        clone = default(T);
        return false;
    }

    private bool TryClonePrimitiveTypes(Type type, object? source, out object? clone)
    {
        if (type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal))
        {
            clone = source;
            return true;
        }

        clone = null;
        return false;
    }

    private bool TryCloneArrays(Type type, object? source, IDictionary<object, object> visited, out object? clone)
    {
        if (type.IsArray && source != null)
        {
            var array = (Array)source;
            var elementType = type.GetElementType()!;
            var localClone = Array.CreateInstance(elementType, array.Length);
            visited[source] = localClone;

            for (int i = 0; i < array.Length; i++)
            {
                localClone.SetValue(CloneInternal(array.GetValue(i), visited), i);
            }

            clone = localClone;
            return true;
        }

        clone = null;
        return false;
    }

    private bool TryCloneList(Type type, object? source, IDictionary<object, object> visited, out object? clone)
    {
        if (source == null)
        {
            clone = null;
            return true; 
        }

        if (typeof(IList).IsAssignableFrom(type))
        {
            var listClone = (IList)Activator.CreateInstance(type)!;
            visited[source] = listClone;
            foreach (var item in (IList)source)
            {
                listClone.Add(CloneInternal(item, visited));
            }

            clone = listClone;
            return true;
        }

        clone = null;
        return false;
    }

    private bool TryCloneDictionary(Type type, object? source, IDictionary<object, object> visited, out object? clone)
    {
        if (source == null)
        {
            clone = null;
            return true;
        }

        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            var dictClone = (IDictionary)Activator.CreateInstance(type)!;
            visited[source] = dictClone;
            foreach (DictionaryEntry entry in (IDictionary)source)
            {
                var clonedKey = CloneInternal(entry.Key, visited);
                if (clonedKey != null)
                {
                    dictClone.Add(clonedKey, CloneInternal(entry.Value, visited));
                }
            }

            clone = dictClone;
            return true;
        }

        clone = null;
        return false;
    }

    private bool TryCloneConcurrentBag(Type type, object? source, IDictionary<object, object> visited, out object? clone)
    {
        if (source == null)
        {
            clone = null;
            return true;
        }

        if (type.IsGenericType &&
           type.GetGenericTypeDefinition() == typeof(ConcurrentBag<>))
        {
            var concurrentBagType = typeof(ConcurrentBag<>).MakeGenericType(type.GenericTypeArguments.First());
            var bagClone = (ICollection?)Activator.CreateInstance(concurrentBagType);

            // Add an item to the ConcurrentBag
            MethodInfo? addMethod = concurrentBagType.GetMethod("Add");

            if (bagClone != null)
            {
                visited[source] = bagClone;
            }

            if (source is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        addMethod?.Invoke(bagClone, [item]);
                    }
                }
            }

            clone = bagClone;
            return true;
        }

        clone = null;
        return false;
    }

    private bool TryCloneComplexObject(Type type, object? source, IDictionary<object, object> visited, out object? clone)
    {
        if (source == null)
        {
            clone = null;
            return true;
        }

        object? cloneObj;
        if (type.IsValueType || type.HasDefaultConstructor())
        {
            // Types with default (parameterless constructors)
            cloneObj = Activator.CreateInstance(type)!;
        }
        else
        {
            var paramTypes = type.GetConstructorParameterTypes();
            if (paramTypes.Count() == 0)
            {
                cloneObj = Activator.CreateInstance(type)!;
            }
            else
            {
                var defaultValues = GetDefaultValue(type);
                if (defaultValues == null || defaultValues.Length != paramTypes.Count())
                {
                    cloneObj = Activator.CreateInstance(type);
                }
                else
                {
                    object?[] initParamValues = new object[paramTypes.Length];
                    for (int i = 0; i < paramTypes.Length; i++)
                    {
                        var paramType = paramTypes[i].ParameterType;
                        initParamValues[i] = defaultValues[i];
                    }
                    cloneObj = Activator.CreateInstance(type, initParamValues)!;
                }
            }
        }

        if (cloneObj != null)
        {
            visited[source] = cloneObj;
        }
            
        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var fieldValue = field.GetValue(source);
            var clonedValue = CloneInternal(fieldValue, visited);
            field.SetValue(cloneObj, clonedValue);
        }

        clone = cloneObj;
        return true;
    }

    private object?[] GetDefaultValue(Type propertyType)
    {
        if (this.ctorParameters.TryGetValue(propertyType, out var defaultValues))
        {
            return defaultValues;
        }

        return [default];
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
