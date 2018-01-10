# TimingScope

Library to help measure different time activities across multiple thread. 


```c#
[TestMethod]
public void TestFromMultipleMethods()
{
    Parallel.Invoke
    (
        () => RootMethod(Guid.NewGuid(), "Call_1"),
        () => RootMethod(Guid.NewGuid(), "Call_2"),
        () => RootMethod(Guid.NewGuid(), "Call_3")
    );
}
```    
Wrap code of your root method in a `TimingScope`. Inside this `using` statement you can log your activities in each method of your calling chain. Library will distinguish different call trees with help of [`ObjectCallContext`](https://github.com/dmitryPavliv/ObjectCallContext)
 
```c#
private void RootMethod(Guid id, string anotherParam)
{
    //Arrange
    using (var scope = TimingScope.Create())
    {
        //Set some context properties. May be helpful for future logging
        scope.SetProperty("ObjectId", id.ToString())
            .SetProperty("AnotherParam", anotherParam);

        var startTime = DateTimeOffset.Now;

        int n = 100;

        //Act
        Parallel.For(0, n, (i) =>
        {
            InnerMethod(i);
        });

        TimingScope.Current.Log("RootMethod", startTime, DateTimeOffset.Now);

        //Assert
        var logEntries = TimingScope.Current.GetLogEntries();
        var properties = TimingScope.Current.GetProperties();

        for (int i = 0; i < n; i++)
        {
            Assert.AreEqual(1, logEntries.Count(x => x.Name == $"InnerMethod({i})"));
        }
        Assert.AreEqual(1, logEntries.Count(x => x.Name == "RootMethod"));

        Assert.AreEqual(2, properties.Count);
        Assert.AreEqual(id.ToString(), properties["ObjectId"]);
        Assert.AreEqual(anotherParam, properties["AnotherParam"]);
    }
}

private void InnerMethod(int i)
{
    var startTime = DateTimeOffset.Now;

    //Some Work
    Thread.Sleep(100);

    TimingScope.Current.Log($"InnerMethod({i})", startTime, DateTimeOffset.Now);
}
```
Available at nuget

    PM> Install-Package TimingScope
