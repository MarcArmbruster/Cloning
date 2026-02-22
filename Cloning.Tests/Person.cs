namespace Cloning.Tests;

internal class Person
{
    public Person()
    {
    }

    public Person(string? name, int? age)
    {
        Name = name;
        Age = age;
    }

    internal string? Name { get; init; }

    internal int? Age { get; init; }
}
