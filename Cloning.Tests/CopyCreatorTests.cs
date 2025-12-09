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

        parcel.NoDefCtorProp = new NoDefCtor(4711, "FancyTestString");

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

        Assert.AreEqual(parcel.NoDefCtorProp?.Count, clone.NoDefCtorProp?.Count);
        Assert.AreEqual(parcel.NoDefCtorProp?.Text, clone.NoDefCtorProp?.Text);
    }
}
