namespace Cloning.Tests;

using DeepCloneUtility.Cloning;

[TestClass]
public sealed class CopyCreatorTests
{
    [TestMethod]
    public void CloneViaMsJsonTest()
    {
        Parcel parcel = CreateTestObject();

        var clone = parcel.DeepCloneViaMsJson();

        CheckClone(parcel, clone);
    }

    [TestMethod]
    public void CloneViaNsJsonTest()
    {
        Parcel parcel = CreateTestObject();

        var clone = parcel.DeepCloneViaNsJson();

        CheckClone(parcel, clone);
    }

    [TestMethod]
    public void PropertyBasedCloneTest()
    {
        Parcel parcel = CreateTestObject();

        var clone = parcel.DeepClone();

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

        return parcel;
    }

    private static void CheckClone(Parcel parcel, Parcel? clone)
    {
        Assert.AreEqual(parcel.Id, clone.Id);
        Assert.AreEqual(parcel.Name, clone.Name);
        Assert.AreEqual(parcel.Weight, clone.Weight);
        Assert.AreEqual(99.99m, clone.Value);
        Assert.AreEqual(parcel.Children.Count, clone.Children.Count);
        Assert.AreEqual("Value1", parcel.Metadata["Key1"]);
        Assert.AreEqual("Value2", parcel.Metadata["Key2"]);

        Assert.AreEqual(parcel.Children[0].Id, clone.Children[0].Id);
        Assert.AreEqual(parcel.Children[0].Name, clone.Children[0].Name);
        Assert.AreEqual(parcel.Children[0].Weight, clone.Children[0].Weight);

        Assert.AreEqual(parcel.Children[1].Id, clone.Children[1].Id);
        Assert.AreEqual(parcel.Children[1].Name, clone.Children[1].Name);
        Assert.AreEqual(parcel.Children[1].Weight, clone.Children[1].Weight);
    }
}
