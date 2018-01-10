using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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

                var start = DateTime.Now;

                //some work
                await Task.Delay(100);

                TimingScope.Current.Log("First", start, DateTime.Now);

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
                Assert.AreEqual(1, logEntries.Count(x => x.Name == "Third" && x.Duration == 1));
                Assert.AreEqual(1, logEntries.Count(x => x.Name == "Third" && x.Duration == 2));

                var logMessage = scope.ToString();
                Console.WriteLine(logMessage);
            }
        }

        private async Task OtherMethodAsync()
        {
            var start = DateTime.Now;
            await Task.Delay(300);
            TimingScope.Current.Log("Second", start, DateTime.Now);
            TimingScope.Current.SetProperty("Prop3", "Val3");
        }

        private async Task OneMoreMethodAsync()
        {
            var start = DateTime.Now;
            await Task.Delay(100);
            TimingScope.Current.Log("Third", start, DateTime.Now, 1);

            start = DateTime.Now;
            await Task.Delay(300);
            TimingScope.Current.Log("Third", start, DateTime.Now, 2);
        }

        private void DoSomeWork(string key)
        {
            using (var scope = TimingScope.Create())
            {
                var start = DateTime.Now;
                int n = 10000;

                Parallel.For(0, n, (i) =>
                {
                    TimingScope.Current.Log(key + "_" + i, start, DateTime.Now);
                });

                var logEntries = scope.GetLogEntries().ToDictionary(x => x.Name);
                Assert.AreEqual(n, logEntries.Count);

                for (int i = 0; i < n; i++)
                {
                    Assert.IsTrue(logEntries.ContainsKey(key + "_" + i));
                }
            }
        }
    }
}
