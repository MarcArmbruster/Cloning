namespace Cloning.Tests;

using DeepCloneUtility.Cloning;

[TestClass]
public sealed class CopyCreatorTests
{
    [TestMethod]
    public void PropertyBasedCloneTest()
    {
        // Arrange
        Parcel parcel = CreateTestObject();
        
        // Act
        Parcel? clone = DeepClone<Parcel>
                        .Builder()
                        .WithSourceInstance(parcel)
                        .UseCtorParameters(typeof(NoDefCtor), [1,"test"])
                        .CreateClone();

        // Assert
        CheckClone(parcel, clone);
    }

    private static Parcel CreateTestObject()
    {
        Parcel parcel = new Parcel("master")
        {
            Id = Guid.NewGuid(),
            Value= 99.99m,
            Weight = 10
        };
        parcel.Metadata.Add("Key1", "Value1");
        parcel.Metadata.Add("Key2", "Value2");

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

        parcel.Notes.Add("Note1");
        parcel.Notes.Add("Note2");
        parcel.Notes.Add("Note3");

        parcel.NoDefCtorProp = new NoDefCtor(4711, "FancyTestString");

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

        return parcel;
    }

    private static void CheckClone(Parcel parcel, Parcel? clone)
    {
        Assert.IsNotNull(clone);
        Assert.AreEqual(parcel.Id, clone.Id);
        Assert.AreEqual(parcel.Name, clone.Name);
        Assert.AreEqual(parcel.Weight, clone.Weight);
        Assert.AreEqual(99.99m, clone.Value);
        Assert.HasCount(parcel.Children.Count, clone.Children);
        Assert.AreEqual("Value1", parcel.Metadata["Key1"]);
        Assert.AreEqual("Value2", parcel.Metadata["Key2"]);

        Assert.AreEqual(parcel.Children[0].Id, clone.Children[0].Id);
        Assert.AreEqual(parcel.Children[0].Name, clone.Children[0].Name);
        Assert.AreEqual(parcel.Children[0].Weight, clone.Children[0].Weight);

        Assert.AreEqual(parcel.Children[1].Id, clone.Children[1].Id);
        Assert.AreEqual(parcel.Children[1].Name, clone.Children[1].Name);
        Assert.AreEqual(parcel.Children[1].Weight, clone.Children[1].Weight);

        Assert.HasCount(3, parcel.Notes);
        CollectionAssert.AreEquivalent(parcel.Notes, clone.Notes);

        Assert.AreEqual(parcel.NoDefCtorProp?.Count, clone.NoDefCtorProp?.Count);
        Assert.AreEqual(parcel.NoDefCtorProp?.Text, clone.NoDefCtorProp?.Text);

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
    }
}