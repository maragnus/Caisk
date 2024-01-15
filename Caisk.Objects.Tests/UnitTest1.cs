namespace Caisk.Objects.Tests;

public class TestProfile : ObjectProfile
{
    public string? RequiredValue { get; set; }

    public void MethodRequiringValue()
    {
        PropertyRequired(RequiredValue);
    }
}

[TestClass]
public class ObjectProfileTests
{
    [TestMethod]
    public void PropertyRequiredWithValue_Works()
    {
        var profile = new TestProfile
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test-object",
            RequiredValue = "has value"
        };
        profile.MethodRequiringValue();
    }
    
    [TestMethod]
    [ExpectedException(typeof(ProfilePropertyRequiredException))]
    public void PropertyRequiredWithNull_Works()
    {
        var profile = new TestProfile
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test-object",
            RequiredValue = null
        };
        profile.MethodRequiringValue();
    }
}