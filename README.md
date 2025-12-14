# Deep Cloning Of Complexe Types

Based on several approaches in multiple projects over many years, the deep cloning of complex types is still a big challange.
This library is an attempt to provide a high-performance solution for deep cloning in C# using all the learnings over the year.
It is focused on functionality and performance.
In addition it shall be based on .NET basic functionalities, only. Avoiding references of any external libraries.

Development started in 2025 and is still ongoing!
Therefore you should not use it in productive systems unless a stable version is published ;-)

Marc Armbruster, 14-Dec-2025

## Licence
--> to be defined

## Overview
This library provides functionality for deep cloning of complex types in C#. It supports a variety of data types including primitive types, arrays, lists, dictionaries, and custom objects. The deep cloning process ensures that all nested objects are also cloned, preventing any references to the original objects.

The library does not work with serialization/deserialization like working with JSON.
The entire logic is based on reflection and expression trees to achieve high performance and supports internal structures, also.

## Easy Usage

```C#

// Example: cloning an object of a custom 'Parcel' type 
// with a complex substructure
var clone = DeepClone<Parcel>
                .Builder()
                .WithSourceInstance(parcel)
                .UseCtorParameters(typeof(NoDefCtor), [1,"test"])
                .UseCustomLogic(
                    typeof(BoringCustomType), 
                    new Func<object?, object?>((source) => new BoringCustomType
                    {
                        ID = Guid.NewGuid(),
                        Name = ((BoringCustomType?)source)?.Name ?? string.Empty
                    }))
                .CreateClone();

```

## Challenges/Features
To create a deep clone of complex types, several challenges need to be addressed:
- Avoid circular references [included]
- Setter only properties [included]
- Init only properties [included]
- Collection types [partially included]
- Concurrent types [partially included]
- Readonly fields [included]
- Types without default constructors [included by parameter registration]
- Individual/custom logic for special types [included]
- ...

## Limitations
- Does not support all concurrent collections, yet.
- Still missing some test scenarios

## Handling types without default constructor
Creation of clone instances need to call a constructor.


## Supported Types
|Type|supported|
|---|---|
|Custom Objects (classes)|yes (depending on sub types - see below)|
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
|HashSet<>|work in process|
|Queue<>|planned|
|Stack<>|work in process|
|LinkedList<>|planned|
|Structs|yes|
|Nullable<>|yes|
|Tuple<>|work in process|
|ValueTuple<>|planned|
|ConcurrentBag<>|partially|
|ConcurrentDictionary<,>|partially|
|ConcurrentQueue<>|partially|
|ConcurrentStack<>|partially|
|Immutable Collections|no|

<b>
IMPORTANT: other types maybe supported but not tested yet!!
</b>

## Author
Marc Armbruster
- [on nuget](https://www.nuget.org/profiles/marcarmbruster)
- [on github](https://github.com/MarcArmbruster)
