# Deep Cloning Of Complex Types

Based on several approaches in multiple projects over many years, the deep cloning of complex types is still a big challange.
This library is an attempt to provide a high-performance solution for deep cloning in C# using all the learnings over the year.
It is focused on functionality and performance.
In addition it shall be based on .NET basic functionalities, only. Avoiding references of any external libraries.

Development started in 2025 and is still ongoing!
Therefore you should not use it in productive systems unless a stable version is published ;-)

Marc Armbruster, 17-Feb-2026

## Version
|Version|Date (code base) |Remarks|
|---|---|---|
|0.1.0| 15-Dec-2025 |initial version (based on .NET10) |
|0.2.0| 17-Feb-2026 |DeepClone.CreateEasyDeepClone(...) added |

## Licence
--> to be defined

## Overview
This library provides functionality for deep cloning of complex types in C#. It supports a variety of data types including primitive types, arrays, lists, dictionaries, and custom objects. The deep cloning process ensures that all nested objects are also cloned, preventing any references to the original objects.

The library does not work with serialization/deserialization like working with JSON.
The entire logic is based on reflection and expression trees to achieve high performance and supports internal structures, also.

## Easy Usage

### A) Easy Cloning Situations
For non-complex types, the cloning process is straightforward:

Just call the static method CreateEasyDeepClone with the source instance as parameter:
```C#

Person origPerson = new Person("Marc", 99)
Person clone = DeepClone.CreateEasyDeepClone(origPerson);

```

### B) Complex Cloning Situations
For more complex types, the builder pattern can be used to provide additional information for the cloning process, e.g. constructor parameters for types without default constructor or custom logic for special types.
```C#

// Example: cloning an object of a custom 'Parcel' type 
// with a complex substructure
        var clone = DeepClone<Parcel>
                        .Builder()
                        .UseSourceInstance(parcel)
                        .UseCtorParameters(typeof(NoDefCtor), [1,"test"])
                        .UseCustomLogic(
                            typeof(BoringCustomType), 
                            new Func<object?, object?>((source) => new BoringCustomType
                            {
                                ID = Guid.NewGuid(),
                                Name = ((BoringCustomType?)source)?.Name ?? string.Empty
                            }))
                        .CreateClone()
                        .Result;

```

The Methods:
- .Builder() --> provides a new builder instance [required]
- .UseSourceInstance(...) --> sets the source instance to be cloned [required]
- .UseCtorParameters(...) --> provides parameters for types without default constructor [optional]
- .UseCustomLogic(...) --> provides custom logic for special types [optional]
- .CreateClone() --> performs the deep clone logic [required]
- .Result --> Result property conating the cloned instance [required]
- 
## Challenges/Features
To create a deep clone of complex types, several challenges need to be addressed:
- Avoid circular references [included]
- Handle setter only properties [included]
- Handle init only properties [included]
- Collection types [partially included]
- Concurrent types [partially included]
- Readonly fields [included]
- Types without default constructors [included by parameter registration]
- Individual/custom logic for special types [included]
- ...

## Limitations
- Does actually not support all concurrent collections.
- Still missing some test scenarios

## Handling types without default constructor
Creation of clone instances need to call a constructor.
Classes without a parameterless constructor need to provide parameters for a specific constructor.
This can be done by calling the method .UseCtorParameters(...) on the builder instance.
Provide any constructor parameters that are required for the specific type.
The values will be overwritten during the cloning process, anyway.

## Custom Logic
If you like to use special/custom handling during the cloning process for defined types use the method
.UseCustomLogic(...) on the builder instance.
Here you can provide a Func<object?, object?> delegate that will be called during the cloning process for the defined type.

## Supported Types (Feb-2026)
|Type|supported|
|---|---|
|Custom Objects (classes)| YES (depending on sub types - see below)|
|bool|yes|
|byte|yes|
|sbyte|yes|
|char|yes|
|decimal|yes|
|double|yes|
|float|yes|
|int|yes|
|uint|yes|
|long|yes|
|ulong|yes|
|short|yes|
|ushort|yes|
|string|yes|
|DateTime|yes|
|DateTimeOffset|yes|
|TimeSpan|yes|
|Guid|yes|
|Enum|yes|
|||
|Array|yes|
|List<>|yes|
|Dictionary<,>|yes|
|HashSet<>|yes|
|Queue<>|NOT planned: queues are dynamic data elements|
|Stack<>|yes|
|LinkedList<>|yes|
|Structs|yes|
|Nullable<>|yes|
|Tuple<>|yes|
|ValueTuple<>|yes|
|ConcurrentBag<>|yes|
|ConcurrentDictionary<,>|yes|
|ConcurrentQueue<>|NOT planned: queues are dynamic data elements|
|ConcurrentStack<>|yes|
|Immutable Collections|NO|

<b>
IMPORTANT: other types maybe supported but not tested yet!!
</b>

## Author
Marc Armbruster
- [on nuget](https://www.nuget.org/profiles/marcarmbruster)
- [on github](https://github.com/MarcArmbruster)
