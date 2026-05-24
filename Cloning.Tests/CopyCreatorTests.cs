namespace Cloning.Tests;

using Cloning;

[TestClass]
public sealed class CopyCreatorTests
{
    [TestMethod]
    public void EasyCloneTest()
    {
        Person person = new("John", 30);
        Person clone = DeepClone.CreateEasyDeepClone(person)!;

        Assert.IsFalse(object.ReferenceEquals(person, clone));
        Assert.AreEqual(person.Name, clone.Name);
        Assert.AreEqual(person.Age, clone.Age);
    }

    [TestMethod]
    public void EasyCloneNullPropertiesTest()
    {
        Person person = new(null, null);
        Person clone = DeepClone.CreateEasyDeepClone(person)!;

        Assert.IsFalse(object.ReferenceEquals(person, clone));
        Assert.AreEqual(person.Name, clone.Name);
        Assert.AreEqual(person.Age, clone.Age);
    }

    [TestMethod]
    public void PropertyBasedCloneTest()
    {
        // Arrange
        Parcel parcel = CreateTestObject();
        
        // Act
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

        // Assert
        CheckClone(parcel, clone);
    }

    private static Parcel CreateTestObject()
    {
        Parcel parcel = new("master")
        {
            Id = Guid.NewGuid(),
            Value= 99.99m,
            Weight = 10
        };
        parcel.DictProp.Add("Key1", "Value1");
        parcel.DictProp.Add("Key2", "Value2");

        parcel.Children.Add(new Parcel("child1")
        {
            Id = Guid.NewGuid(),
            Weight = 1
        });

        parcel.Children.Add(new Parcel("child2")
        {
            Id = Guid.NewGuid(),
            Weight = 2
        });

        parcel.ConcBagProp.Add("Note1");
        parcel.ConcBagProp.Add("Note2");
        parcel.ConcBagProp.Add("Note3");

        parcel.ConcDictProp.TryAdd(1, "One");
        parcel.ConcDictProp.TryAdd(2, "Two");
        parcel.ConcDictProp.TryAdd(3, "Three");

        parcel.NoDefCtorProp = new NoDefCtor(4711, "FancyTestString");

        parcel.Boring = new BoringCustomType
        {
            ID = Guid.NewGuid(),
            Name = "BoringName"
        };

        parcel.PrimitveDetails = new PrimitveDetails
        {
            BoolValue = true,
            ByteValue = 0x12,
            SByteValue = -12,
            ShortValue = -1234,
            UShortValue = 1234,
            CharValue = 'X',
            DecimalValue = 123.45m,
            DoubleValue = 12345.6789,
            FloatValue = 1234.56f,
            IntValue = 12345,
            UIntValue = 777,
            LongValue = 123456789,
            ULongValue = 987654321,
            StringValue = "TestString",
            DateTimeValue = new DateTime(2020, 1, 1, 12, 0, 0),
            DateTimeOffsetValue = new DateTimeOffset(2020, 1, 1, 12, 0, 0, TimeSpan.Zero),
            GuidValue = Guid.NewGuid(),
            TimeSpanValue = new TimeSpan(1, 2, 3, 4, 5),
            Supported = Supported.Yes
        };

        parcel.HashSetProp.Add(1);
        parcel.HashSetProp.Add(11);
        parcel.HashSetProp.Add(111);
        parcel.HashSetProp.Add(1111);

        parcel.StackProp.Push("First");
        parcel.StackProp.Push("Second");
        parcel.StackProp.Push("Third");

        parcel.LinkedListProp.AddFirst(10.1m);
        parcel.LinkedListProp.AddLast(20.2m);

        parcel.ConcStackProp.Push("CFirst");
        parcel.ConcStackProp.Push("CSecond");
        parcel.ConcStackProp.Push("CThird");

        parcel.ConcDictProp[1] = "A";
        parcel.ConcDictProp[2] = "B";
        parcel.ConcDictProp[3] = "C";

        parcel.TupleProp = new Tuple<int, string, decimal, object?>(7, "TupleString", 1234.56m, "X");
        parcel.ValueTupleProp = new ValueTuple<int, string, decimal, object?>(7, "TupleString", 1234.56m, "X");
        
        parcel.FakeImageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // Simulated JPEG header bytes

        return parcel;
    }

    private static void CheckClone(Parcel parcel, Parcel? clone)
    {
        // Root Level
        Assert.IsNotNull(clone);
        Assert.AreEqual(parcel.Id, clone.Id);
        Assert.AreEqual(parcel.Name, clone.Name);
        Assert.AreEqual(parcel.Weight, clone.Weight);
        Assert.AreEqual(99.99m, clone.Value);
        Assert.HasCount(parcel.Children.Count, clone.Children);
        Assert.AreEqual("Value1", parcel.DictProp["Key1"]);
        Assert.AreEqual("Value2", parcel.DictProp["Key2"]);

        // List (Hierarchal Children)
        Assert.AreEqual(parcel.Children[0].Id, clone.Children[0].Id);
        Assert.AreEqual(parcel.Children[0].Name, clone.Children[0].Name);
        Assert.AreEqual(parcel.Children[0].Weight, clone.Children[0].Weight);
        Assert.AreEqual(parcel.Children[1].Id, clone.Children[1].Id);
        Assert.AreEqual(parcel.Children[1].Name, clone.Children[1].Name);
        Assert.AreEqual(parcel.Children[1].Weight, clone.Children[1].Weight);

        // ConcurrentBag
        Assert.HasCount(3, parcel.ConcBagProp);
        CollectionAssert.AreEquivalent(parcel.ConcBagProp, clone.ConcBagProp);

        // ConcurrentDictionary
        Assert.HasCount(3, parcel.ConcDictProp);
        CollectionAssert.AreEquivalent(parcel.ConcDictProp, clone.ConcDictProp);

        // NoDefCtor
        Assert.AreEqual(parcel.NoDefCtorProp?.Count, clone.NoDefCtorProp?.Count);
        Assert.AreEqual(parcel.NoDefCtorProp?.Text, clone.NoDefCtorProp?.Text);

        // BoringCustomType
        Assert.IsFalse(object.ReferenceEquals(parcel.Boring, clone.Boring));
        Assert.AreNotEqual(parcel.Boring.ID, clone.Boring.ID); // ID is regenerated in custom clone logic
        Assert.AreEqual(parcel.Boring.Name, clone.Boring.Name);

        // PrimitveDetails
        Assert.IsFalse(object.ReferenceEquals(parcel.PrimitveDetails, clone.PrimitveDetails));
        Assert.AreEqual(parcel.PrimitveDetails.BoolValue, clone.PrimitveDetails.BoolValue);
        Assert.AreEqual(parcel.PrimitveDetails.ByteValue, clone.PrimitveDetails.ByteValue);
        Assert.AreEqual(parcel.PrimitveDetails.SByteValue, clone.PrimitveDetails.SByteValue);
        Assert.AreEqual(parcel.PrimitveDetails.ShortValue, clone.PrimitveDetails.ShortValue);
        Assert.AreEqual(parcel.PrimitveDetails.UShortValue, clone.PrimitveDetails.UShortValue);
        Assert.AreEqual(parcel.PrimitveDetails.CharValue, clone.PrimitveDetails.CharValue);
        Assert.AreEqual(parcel.PrimitveDetails.DecimalValue, clone.PrimitveDetails.DecimalValue);
        Assert.AreEqual(parcel.PrimitveDetails.DoubleValue, clone.PrimitveDetails.DoubleValue);
        Assert.AreEqual(parcel.PrimitveDetails.FloatValue, clone.PrimitveDetails.FloatValue);
        Assert.AreEqual(parcel.PrimitveDetails.IntValue, clone.PrimitveDetails.IntValue);
        Assert.AreEqual(parcel.PrimitveDetails.UIntValue, clone.PrimitveDetails.UIntValue);
        Assert.AreEqual(parcel.PrimitveDetails.LongValue, clone.PrimitveDetails.LongValue);
        Assert.AreEqual(parcel.PrimitveDetails.ULongValue, clone.PrimitveDetails.ULongValue);
        Assert.AreEqual(parcel.PrimitveDetails.StringValue, clone.PrimitveDetails.StringValue);
        Assert.AreEqual(parcel.PrimitveDetails.DateTimeValue, clone.PrimitveDetails.DateTimeValue);
        Assert.AreEqual(parcel.PrimitveDetails.DateTimeOffsetValue, clone.PrimitveDetails.DateTimeOffsetValue);
        Assert.AreEqual(parcel.PrimitveDetails.GuidValue, clone.PrimitveDetails.GuidValue);
        Assert.AreEqual(parcel.PrimitveDetails.TimeSpanValue, clone.PrimitveDetails.TimeSpanValue);
        Assert.AreEqual(parcel.PrimitveDetails.Supported, clone.PrimitveDetails.Supported);

        // HashSet
        Assert.IsFalse(object.ReferenceEquals(parcel.HashSetProp, clone.HashSetProp));
        Assert.HasCount(parcel.HashSetProp.Count, clone.HashSetProp);
        CollectionAssert.AreEquivalent(parcel.HashSetProp.ToArray(), clone.HashSetProp.ToArray());

        // Stack
        Assert.IsFalse(object.ReferenceEquals(parcel.StackProp, clone.StackProp));
        Assert.HasCount(parcel.StackProp.Count, clone.StackProp);
        CollectionAssert.AreEquivalent(parcel.StackProp.ToArray(), clone.StackProp.ToArray());

        // LinkedList
        Assert.IsFalse(object.ReferenceEquals(parcel.LinkedListProp, clone.LinkedListProp));
        Assert.HasCount(parcel.LinkedListProp.Count, clone.LinkedListProp);
        CollectionAssert.AreEquivalent(parcel.LinkedListProp.ToArray(), clone.LinkedListProp.ToArray());

        // ConcurrentStack
        Assert.IsFalse(object.ReferenceEquals(parcel.ConcStackProp, clone.ConcStackProp));
        Assert.HasCount(parcel.ConcStackProp.Count, clone.ConcStackProp);
        CollectionAssert.AreEquivalent(parcel.ConcStackProp.ToArray(), clone.ConcStackProp.ToArray());

        // ConcurrentDictionary
        Assert.IsFalse(object.ReferenceEquals(parcel.ConcDictProp, clone.ConcDictProp));
        Assert.HasCount(parcel.ConcDictProp.Count, clone.ConcDictProp);
        CollectionAssert.AreEquivalent(parcel.ConcDictProp.Values.ToArray(), clone.ConcDictProp.Values.ToArray());

        // Tuple
        Assert.IsFalse(object.ReferenceEquals(parcel.TupleProp, clone.TupleProp));
        Assert.AreEqual(parcel.TupleProp.Item1, clone.TupleProp.Item1);
        Assert.AreEqual(parcel.TupleProp.Item2, clone.TupleProp.Item2);
        Assert.AreEqual(parcel.TupleProp.Item3, clone.TupleProp.Item3);
        Assert.AreEqual(parcel.TupleProp.Item4, clone.TupleProp.Item4);

        // ValueTuple
        Assert.AreEqual(parcel.ValueTupleProp.Item1, clone.ValueTupleProp.Item1);
        Assert.AreEqual(parcel.ValueTupleProp.Item2, clone.ValueTupleProp.Item2);
        Assert.AreEqual(parcel.ValueTupleProp.Item3, clone.ValueTupleProp.Item3);
        Assert.AreEqual(parcel.ValueTupleProp.Item4, clone.ValueTupleProp.Item4);

        // Byte-Array
        Assert.IsNotNull(parcel.FakeImageData);
        Assert.IsNotNull(clone.FakeImageData);
        Assert.AreNotSame(parcel.FakeImageData, clone.FakeImageData);
        Assert.HasCount(4, parcel.FakeImageData);
        Assert.HasCount(parcel.FakeImageData.Length, clone.FakeImageData);
        Assert.AreEqual(parcel.FakeImageData[0], clone.FakeImageData[0]);
        Assert.AreEqual(parcel.FakeImageData[1], clone.FakeImageData[1]);
        Assert.AreEqual(parcel.FakeImageData[2], clone.FakeImageData[2]);
        Assert.AreEqual(parcel.FakeImageData[3], clone.FakeImageData[3]);
    }
}