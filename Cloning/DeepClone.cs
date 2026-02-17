namespace Cloning;

using Cloning.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides static methods for creating deep copies of objects.
/// </summary>
/// <remarks>The DeepClone class offers utility methods to perform deep cloning operations, allowing callers to
/// create independent copies of objects. All members are static and thread safe.</remarks>
public static class DeepClone
{
    /// <summary>
    /// Creates a deep clone of the specified object instance using a simplified cloning mechanism.
    /// Use this method when you need a quick and easy way to clone an object without requiring custom logic or constructor options.
    /// </summary>
    /// <remarks>This method provides a convenient way to create a deep copy of an object. The cloning process
    /// copies all fields and properties, resulting in a new instance that is independent of the original. The method
    /// may not support all types; ensure that the type parameter supports deep cloning as required.</remarks>
    /// <typeparam name="T">The type of the object to clone. Must be a reference type that supports deep cloning.</typeparam>
    /// <param name="source">The object instance to clone. Can be null.</param>
    /// <returns>A deep clone of the specified object, or null if the source is null.</returns>
    [DebuggerStepThrough]
    public static T? CreateEasyDeepClone<T>(T? source)
    {
        if (source == null)
        {
            return default(T)!;
        }

        T? clone = DeepClone<T>.Builder().UseSourceInstance(source).CreateClone().Result;
        return clone ?? default;
    }
}

/// <summary>
/// Provides extension methods for deep cloning any object graph.
/// Supports cyclic references, arrays, and common collection types.
/// </summary>
public class DeepClone<T>
{
    private T? sourceInstance;
    private readonly Dictionary<Type, object[]> ctorParameters = [];
    private readonly Dictionary<Type, Func<object?, object?>> customLogic = [];

    /// <summary>
    /// Private constructor to enforce the use of the Builder method.
    /// </summary>
    private DeepClone()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DeepClone{T}"/> builder for configuring and performing deep clone
    /// operations.
    /// </summary>
    /// <remarks>Use this method to begin a fluent configuration of deep cloning behavior for objects of type
    /// <typeparamref name="T"/>. The returned builder can be used to customize cloning options before performing the
    /// clone.</remarks>
    /// <returns>A new <see cref="DeepClone{T}"/> instance for the specified type parameter <typeparamref name="T"/>.</returns>
    [DebuggerStepThrough]
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Fluent code design")]
    public static DeepClone<T> Builder()
    {
        return new DeepClone<T>();
    }

    /// <summary>
    /// Gets the result value produced by the operation, or null if no result is available.
    /// </summary>
    public T? Result { get; private set; }

    /// <summary>
    /// Sets the source instance to be used for deep cloning operations and returns the current <see cref="DeepClone{T}"/> instance.  
    /// </summary>
    /// <param name="source">The object instance that will serve as the source for cloning. Can be null if cloning a default or empty
    /// instance is desired.</param>
    /// <returns>The current <see cref="DeepClone{T}"/> instance with the specified source instance set.</returns>
    [DebuggerStepThrough]
    public DeepClone<T> UseSourceInstance(T source)
    {
        this.sourceInstance = source;
        return this;
    }

    /// <summary>
    /// Registers a custom cloning logic for the specified type to be used during deep cloning operations.  
    /// </summary>
    /// <remarks>If custom logic is already registered for the specified type, it will be replaced by the new
    /// delegate. Custom logic allows overriding the default cloning behavior for specific types.</remarks>
    /// <param name="type">The type for which the custom cloning logic is to be applied. Cannot be null.</param>
    /// <param name="individualCloneLogic">A delegate that defines the custom logic for cloning instances of the specified type. If null, no custom logic
    /// is registered.</param>
    /// <returns>The current <see cref="DeepClone{T}"/> instance, enabling method chaining.</returns>
    [DebuggerStepThrough]
    public DeepClone<T> UseCustomLogic(Type type, Func<object?, object?> individualCloneLogic)
    {
        if (individualCloneLogic != null)
        {
            customLogic[type] = individualCloneLogic;
        }

        return this;
    }

    /// <summary>
    /// Specifies constructor parameter types and their default values to use when creating instances during deep
    /// cloning.    
    /// </summary>
    /// <remarks>Use this method to provide explicit constructor arguments when the type being cloned does not
    /// have a parameterless constructor or requires specific values for instantiation. This is useful for types that
    /// cannot be created with default values alone.</remarks>
    /// <param name="parameterType">The type of the constructor parameter for which to set default values. Cannot be null.</param>
    /// <param name="parameterDefaultValues">An array of default values to use for the specified constructor parameter type. The array elements correspond to
    /// the parameter values in order.</param>
    /// <returns>The current <see cref="DeepClone{T}"/> instance with the specified constructor parameters configured.</returns>
    [DebuggerStepThrough]
    public DeepClone<T> UseCtorParameters(Type parameterType, object[] parameterDefaultValues)
    {
        this.ctorParameters[parameterType] = parameterDefaultValues;
        return this;
    }

    /// <summary>
    /// Creates a deep clone of the current source instance, or returns the default value if the source instance is
    /// null.
    /// </summary>
    /// <remarks>The cloning operation preserves object references within the source instance, ensuring that
    /// shared references are maintained in the clone. If the type T does not have a parameterless constructor, a
    /// MissingMethodException is thrown.</remarks>
    /// <returns>A deep clone of the source instance if it is not null; otherwise, the default value for type T.</returns>
    public DeepClone<T> CreateClone()
    {
        if (this.sourceInstance == null)
        {
            this.Result = default;
        }

        var alreadyCloned = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
        try
        {
            var clone = (T?)this.CloneInternal(this.sourceInstance, alreadyCloned);
            this.Result = clone;
        }
        catch (MissingMethodException)
        {
            throw;
        }

        return this;
    }

    /// <summary>
    /// Creates a deep clone of the specified object, handling common collection types and preserving object reference
    /// integrity.  
    /// </summary>
    /// <remarks>This method supports deep cloning of arrays, lists, dictionaries, tuples, and several
    /// concurrent collection types. Immutable and primitive types are returned as-is. The <paramref name="visited"/>
    /// dictionary is used internally to prevent infinite recursion when cloning objects with cyclic
    /// references.</remarks>
    /// <param name="source">The object to clone. Can be null.</param>
    /// <param name="visited">A dictionary used to track already cloned objects and handle cyclic references during the cloning process. Must
    /// not be null.</param>
    /// <returns>A deep clone of the <paramref name="source"/> object, or null if <paramref name="source"/> is null.</returns>
    /// <exception cref="NotSupportedException">Thrown if the type of <paramref name="source"/> is not supported for deep cloning.</exception>
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
        if (TryClonePrimitiveTypes(type, source, out _))
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

        // ConcurrentStack<T> (e.g. ConcurrentStack<string>)
        if (TryCloneConcurrentStack(type, source, visited, out var concStackClone)) return concStackClone;

        // IDictionary (e.g. Dictionary<K,V>)
        if (TryCloneDictionary(type, source, visited, out var dictClone)) return dictClone;

        // ConcurrentDictionary<K,V> (e.g. ConcurrentDictionary<int,string>)
        if (TryCloneConcurrentDict(type, source, visited, out var concDictClone)) return concDictClone;

        // Tuples
        if (TryCloneTuple(type, source, visited, out var tupleClone)) return tupleClone;

        // Complex types
        if (TryCloneComplexObject(type, source, visited, out var objClone)) return objClone;
        
        throw new NotSupportedException($"Type '{type.FullName}' is not supported for deep cloning.");        
    }

    /// <summary>
    /// Attempts to create a clone of the specified object using custom logic associated with the given type.
    /// </summary>
    /// <remarks>If custom logic is available for the specified type, it is used to clone the object. The
    /// method also updates the visited dictionary to track the source and its clone, which is useful for handling
    /// object graphs with shared references or cycles.</remarks>
    /// <param name="type">The type for which to attempt custom cloning logic.</param>
    /// <param name="source">The object instance to be cloned. Can be null.</param>
    /// <param name="visited">A dictionary used to track already cloned objects to prevent duplicate cloning and handle object graphs with
    /// cycles.</param>
    /// <param name="clone">When this method returns, contains the cloned object if custom logic was applied; otherwise, null.</param>
    /// <returns>true if custom cloning logic was found and applied for the specified type; otherwise, false.</returns>
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

    /// <summary>
    /// Attempts to clone a value if it is a primitive type, enumeration, string, or decimal.   
    /// </summary>
    /// <remarks>This method does not perform a deep copy. For supported types, the clone is the same
    /// reference as the source. For unsupported types, the method returns false and clone is set to null.</remarks>
    /// <param name="type">The type of the value to clone. Must not be null.</param>
    /// <param name="source">The source object to clone. Can be null if the type supports null values.</param>
    /// <param name="clone">When this method returns, contains the cloned value if the operation succeeds; otherwise, null. This parameter
    /// is passed uninitialized.</param>
    /// <returns>true if the value was cloned as a primitive, enumeration, string, or decimal; otherwise, false.</returns>
    private static bool TryClonePrimitiveTypes(Type type, object? source, out object? clone)
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

    /// <summary>
    /// Attempts to create a deep clone of the specified array instance.
    /// </summary>
    /// <remarks>This method is intended for use in deep cloning scenarios where arrays may contain reference
    /// types or circular references. The method does not clone non-array objects.</remarks>
    /// <param name="type">The type of the object to clone. Must represent an array type for cloning to occur.</param>
    /// <param name="source">The source object to clone. Must be a non-null array instance of the specified type.</param>
    /// <param name="visited">A dictionary used to track already cloned objects to prevent circular references during cloning.</param>
    /// <param name="clone">When this method returns, contains the cloned array if cloning was successful; otherwise, null.</param>
    /// <returns>true if the source object is a non-null array and was successfully cloned; otherwise, false.</returns>
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

    /// <summary>
    /// Attempts to create a shallow clone of a tuple object of the specified type. 
    /// </summary>
    /// <remarks>This method only supports cloning objects that are tuples (types whose names start with
    /// "Tuple"). For other types, the method returns false and sets the out parameter to null.</remarks>
    /// <param name="type">The type of the tuple to clone. Must represent a System.Tuple or compatible tuple type.</param>
    /// <param name="source">The source object to clone. Should be an instance of the specified tuple type.</param>
    /// <param name="visited">A dictionary used to track already visited objects during the cloning process. This helps prevent circular
    /// references.</param>
    /// <param name="clone">When this method returns, contains the cloned tuple object if cloning was successful; otherwise, null. This
    /// parameter is passed uninitialized.</param>
    /// <returns>true if the source object is a tuple and was successfully cloned; otherwise, false.</returns>
    private static bool TryCloneTuple(Type type, object? source, IDictionary<object, object> visited, out object? clone)
    {
        if (type.Name.StartsWith("Tuple", StringComparison.InvariantCultureIgnoreCase))
        {
            if (source == null || source is not ITuple tupleInfo)
            {
                clone = null;
                return false;
            }

            object[] parameters = new object[tupleInfo.Length];
            for (var i = 0; i < tupleInfo.Length; i++)
            {
                parameters[i] = tupleInfo[i]!;
            }

            var tupleClone = Activator.CreateInstance(type, parameters);
            visited[source] = tupleClone!;

            clone = tupleClone;
            return true;
        }

        clone = null;
        return false;
    }

    /// <summary>
    /// Attempts to create a deep clone of a list object of the specified type. 
    /// </summary>
    /// <remarks>This method supports deep cloning of objects that implement <see cref="IList"/>. If the
    /// specified type does not represent a list, the method returns false and <paramref name="clone"/> is set to <see
    /// langword="null"/>.</remarks>
    /// <param name="type">The type of the list to clone. Must implement <see cref="IList"/>.</param>
    /// <param name="source">The source object to clone. If <see langword="null"/>, the method returns <see langword="true"/> and sets
    /// <paramref name="clone"/> to <see langword="null"/>.</param>
    /// <param name="visited">A dictionary used to track already cloned objects and handle reference cycles during the cloning process.</param>
    /// <param name="clone">When this method returns, contains the cloned list object if cloning was successful; otherwise, <see
    /// langword="null"/>.</param>
    /// <returns>true if the source object is a list and was successfully cloned; otherwise, false.</returns>
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

    /// <summary>
    /// Attempts to create a deep clone of the specified dictionary object. 
    /// </summary>
    /// <remarks>This method supports deep cloning of objects that implement <see cref="IDictionary"/>. If the
    /// specified type does not implement <see cref="IDictionary"/>, the method returns false and does not perform
    /// cloning.</remarks>
    /// <param name="type">The type of the dictionary to clone. Must implement <see cref="IDictionary"/>.</param>
    /// <param name="source">The source object to clone. If <see langword="null"/>, the method returns <see langword="true"/> and sets
    /// <paramref name="clone"/> to <see langword="null"/>.</param>
    /// <param name="visited">A dictionary used to track already cloned objects to handle reference cycles during cloning.</param>
    /// <param name="clone">When this method returns, contains the cloned dictionary if cloning was successful; otherwise, <see
    /// langword="null"/>.</param>
    /// <returns>true if the dictionary was successfully cloned or the source is null; otherwise, false.</returns>
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

    /// <summary>
    /// Attempts to create a shallow clone of a ConcurrentBag collection if the specified type represents a
    /// ConcurrentBag.   
    /// </summary>
    /// <remarks>This method performs a shallow clone of the elements in the ConcurrentBag. Only collections
    /// of generic type ConcurrentBag are supported; for other types, the method returns false and does not perform cloning.
    /// The method is intended for use in custom object cloning scenarios where thread-safe collections need to be
    /// duplicated.</remarks>
    /// <param name="type">The type of the object to clone. Must be a generic type definition of ConcurrentBag to perform cloning.</param>
    /// <param name="source">The source object to clone. If null, the method returns true and sets <paramref name="clone"/> to null.</param>
    /// <param name="visited">A dictionary used to track already-cloned objects to prevent circular references during the cloning process.</param>
    /// <param name="clone">When this method returns, contains the cloned ConcurrentBag instance if cloning was successful; otherwise, null.</param>
    /// <returns>true if the source was a ConcurrentBag and was successfully cloned or was null; otherwise, false.</returns>
    private static bool TryCloneConcurrentBag(Type type, object? source, IDictionary<object, object> visited, out object? clone)
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

    /// <summary>
    /// Attempts to create a shallow clone of a ConcurrentStack instance of the specified type.
    /// </summary>
    /// <remarks>This method performs a shallow clone of the ConcurrentStack, copying references to the
    /// contained items. It does not perform a deep copy of the stack's elements. The method is intended for use in
    /// custom cloning scenarios where thread-safe collections need to be duplicated.</remarks>
    /// <param name="type">The type of the source object. Must represent a generic ConcurrentStack type to be cloned.</param>
    /// <param name="source">The source object to clone. If null, the method sets <paramref name="clone"/> to null and returns <see
    /// langword="true"/>.</param>
    /// <param name="visited">A dictionary used to track already visited objects during the cloning process to prevent cycles.</param>
    /// <param name="clone">When this method returns, contains the cloned ConcurrentStack instance if cloning was successful; otherwise,
    /// null.</param>
    /// <returns>true if the source object is a ConcurrentStack and was successfully cloned or is null; otherwise, false.</returns>
    private static bool TryCloneConcurrentStack(Type type, object? source, IDictionary<object, object> visited, out object? clone)
    {
        if (source == null)
        {
            clone = null;
            return true;
        }

        if (type.IsGenericType &&
           type.GetGenericTypeDefinition() == typeof(ConcurrentStack<>))
        {
            var concurrentStackType = typeof(ConcurrentStack<>).MakeGenericType(type.GenericTypeArguments.First());
            var stackClone = (ICollection?)Activator.CreateInstance(concurrentStackType);

            // Push an item to the ConcurrentStack
            MethodInfo? addMethod = concurrentStackType.GetMethod("Push");

            if (stackClone != null)
            {
                visited[source] = stackClone;
            }

            if (source is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        addMethod?.Invoke(stackClone, [item]);
                    }
                }
            }

            clone = stackClone;
            return true;
        }

        clone = null;
        return false;
    }

    /// <summary>
    /// Attempts to create a shallow clone of a ConcurrentDictionary instance, if the specified type represents a
    /// ConcurrentDictionary.
    /// </summary>
    /// <remarks>This method performs a shallow clone of the ConcurrentDictionary. Only types that are generic
    /// ConcurrentDictionary are supported. If the type does not represent a ConcurrentDictionary, the
    /// method returns false and <paramref name="clone"/> is set to null.</remarks>
    /// <param name="type">The type of the object to clone. Must represent a ConcurrentDictionary generic type to be cloned by this method.</param>
    /// <param name="source">The source object to clone. If null, the method returns true and sets <paramref name="clone"/> to null.</param>
    /// <param name="visited">A dictionary used to track already-cloned objects to prevent cycles during cloning.</param>
    /// <param name="clone">When this method returns, contains the cloned ConcurrentDictionary if cloning was successful; otherwise, null.</param>
    /// <returns>true if the source was null or a ConcurrentDictionary and was successfully cloned; otherwise, false.</returns>
    private static bool TryCloneConcurrentDict(Type type, object? source, IDictionary<object, object> visited, out object? clone)
    {
        if (source == null)
        {
            clone = null;
            return true;
        }

        if (type.IsGenericType &&
           type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>))
        {
            var concurrentKeyType = typeof(ConcurrentDictionary<,>).MakeGenericType(type.GenericTypeArguments.First());
            var concurrentValueType = typeof(ConcurrentDictionary<,>).MakeGenericType(type.GenericTypeArguments.Last());

            var dictClone = Convert.ChangeType(Activator.CreateInstance(concurrentKeyType, concurrentValueType), type, System.Globalization.CultureInfo.InvariantCulture);

            // Add an item to the ConcurrentBag
            MethodInfo? addMethod = type.GetMethod("GetOrAdd");

            if (dictClone != null)
            {
                visited[source] = dictClone;
            }

            if (source is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        KeyValuePair<object, object> kvp = (KeyValuePair<object, object>)item;
                        addMethod?.Invoke(dictClone, [kvp.Key, kvp.Value]);
                    }
                }
            }

            clone = dictClone;
            return true;
        }

        clone = null;
        return false;
    }

    /// <summary>
    /// Attempts to create a deep clone of the specified complex object, returning a value that indicates whether the
    /// operation was successful.
    /// </summary>
    /// <remarks>This method performs a deep clone of the object, including all fields, and supports objects
    /// with reference cycles by using the <paramref name="visited"/> dictionary. The method does not throw exceptions
    /// for unsupported types; instead, it returns false if cloning is not possible.</remarks>
    /// <param name="type">The type of the object to clone. Must not be null.</param>
    /// <param name="source">The source object instance to clone. If null, the method returns true and sets <paramref name="clone"/> to null.</param>
    /// <param name="visited">A dictionary used to track already-cloned objects and handle reference cycles during the cloning process. Must
    /// not be null.</param>
    /// <param name="clone">When this method returns, contains the cloned object if the operation was successful; otherwise, null.</param>
    /// <returns>true if the object was successfully cloned or the source is null; otherwise, false.</returns>
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
            if (paramTypes.Length == 0)
            {
                cloneObj = Activator.CreateInstance(type)!;
            }
            else
            {
                var defaultValues = GetDefaultValue(type);
                if (defaultValues == null || defaultValues.Length != paramTypes.Length)
                {
                    cloneObj = Activator.CreateInstance(type);
                }
                else
                {
                    object?[] initParamValues = new object[paramTypes.Length];
                    for (int i = 0; i < paramTypes.Length; i++)
                    {
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

    /// <summary>
    /// Retrieves the default value array associated with the specified property type.
    /// </summary>
    /// <param name="propertyType">The type of the property for which to obtain the default value array. Cannot be null.</param>
    /// <returns>An array of objects containing the default values for the specified property type. If no default values are
    /// defined, returns an array containing a single null element.</returns>
    private object?[] GetDefaultValue(Type propertyType)
    {
        if (this.ctorParameters.TryGetValue(propertyType, out var defaultValues))
        {
            return defaultValues;
        }

        return [default];
    }

    /// <summary>
    /// Provides an equality comparer that determines equality by object reference rather than by value.
    /// </summary>
    /// <remarks>Use this comparer when object identity, rather than value equality, is required for
    /// comparisons or as a key in collections such as dictionaries or hash sets. Two objects are considered equal only
    /// if they refer to the same instance.</remarks>
    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}