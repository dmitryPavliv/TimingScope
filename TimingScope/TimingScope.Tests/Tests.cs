using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using static TimingScope.TimingScopeHelper;

namespace TimingScope.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public async Task WorksForDifferentThreadsAsync()
        {
            await Task.WhenAll(
                Task.Run(() => DoSomeWork("Abc")),
                Task.Run(() => DoSomeWork("Bcd")),
                Task.Run(() => DoSomeWork("Cde")),
                Task.Run(() => DoSomeWork("Def")),
                Task.Run(() => DoSomeWork("Efg"))
            );
        }

        [TestMethod]
        public async Task End2EndTestAsync()
        {
            using (var scope = TimingScope.Create())
            {
                scope.SetProperty("Prop1", "Val1")
                    .SetProperty("Prop2", "Val2");
                
                await WithTimingAsync("First", () => Task.Delay(100));

                await Task.WhenAll(Task.Run(OtherMethodAsync), Task.Run(OneMoreMethodAsync));

                var properties = scope.GetProperties();
                var logEntries = scope.GetLogEntries();

                Assert.AreEqual(3, properties.Count);
                Assert.AreEqual("Val1", properties["Prop1"]);
                Assert.AreEqual("Val2", properties["Prop2"]);
                Assert.AreEqual("Val3", properties["Prop3"]);

                Assert.AreEqual(4, logEntries.Count);
                Assert.AreEqual(1, logEntries.Count(x => x.Name == "First"));
                Assert.AreEqual(1, logEntries.Count(x => x.Name == "Second"));
                Assert.AreEqual(2, logEntries.Count(x => x.Name == "Third"));

                var logMessage = scope.ToString();
                Console.WriteLine(logMessage);
            }
        }

        private async Task OtherMethodAsync()
        {
            await WithTimingAsync("Second", () => Task.Delay(300));
            TimingScope.Current.SetProperty("Prop3", "Val3");
        }

        private async Task OneMoreMethodAsync()
        {
            await WithTimingAsync("Third", () => Task.Delay(100));
            await WithTimingAsync("Third", () => Task.Delay(300));
        }

        private void DoSomeWork(string key)
        {
            using (var scope = TimingScope.Create())
            {
                int n = 10000;

                Parallel.For(0, n, (i) =>
                {
                    WithTiming(key + "_" + i, () =>
                    {
                        //some work
                    });
                });

                var logEntries = scope.GetLogEntries().ToDictionary(x => x.Name);
                Assert.AreEqual(n, logEntries.Count);

                for (int i = 0; i < n; i++)
                {
                    Assert.IsTrue(logEntries.ContainsKey(key + "_" + i));
                }
            }
        }

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
    }
}
