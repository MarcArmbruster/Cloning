namespace Cloning.Tests;

using DeepCloneUtility.Cloning;

[TestClass]
public sealed class CopyCreatorTests
{
    [TestMethod]
    public void CloneViaJsonTest()
    {
        Parcel parcel = new Parcel("master")
        {
            Id = Guid.NewGuid(),
            Weight = 10
        };
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

        var clone = parcel.DeepCloneViaJson();

        Assert.AreEqual(parcel.Id, clone.Id);
        Assert.AreEqual(parcel.Name, clone.Name);
        Assert.AreEqual(parcel.Weight, clone.Weight);
        Assert.AreEqual(parcel.Children.Count, clone.Children.Count);

        Assert.AreEqual(parcel.Children[0].Id, clone.Children[0].Id);
        Assert.AreEqual(parcel.Children[0].Name, clone.Children[0].Name);
        Assert.AreEqual(parcel.Children[0].Weight, clone.Children[0].Weight);

        Assert.AreEqual(parcel.Children[1].Id, clone.Children[1].Id);
        Assert.AreEqual(parcel.Children[1].Name, clone.Children[1].Name);
        Assert.AreEqual(parcel.Children[1].Weight, clone.Children[1].Weight);
    }

    [TestMethod]
    public void CloneTest()
    {
        Parcel parcel = new Parcel("master")
        {
            Id = Guid.NewGuid(),
            Weight = 10
        };
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

        var clone = parcel.DeepClone();

        Assert.AreEqual(parcel.Id, clone.Id);
        Assert.AreEqual(parcel.Name, clone.Name);
        Assert.AreEqual(parcel.Weight, clone.Weight);
        Assert.AreEqual(parcel.Children.Count, clone.Children.Count);

        Assert.AreEqual(parcel.Children[0].Id, clone.Children[0].Id);
        Assert.AreEqual(parcel.Children[0].Name, clone.Children[0].Name);
        Assert.AreEqual(parcel.Children[0].Weight, clone.Children[0].Weight);

        Assert.AreEqual(parcel.Children[1].Id, clone.Children[1].Id);
        Assert.AreEqual(parcel.Children[1].Name, clone.Children[1].Name);
        Assert.AreEqual(parcel.Children[1].Weight, clone.Children[1].Weight);
    }
}
