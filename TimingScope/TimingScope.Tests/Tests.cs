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
        public async Task End2EndTestAsync()
        {
            using (var scope = TimingScope.Create())
            {
                scope.SetProperty("Prop1", "Val1")
                    .SetProperty("Prop2", "Val2");

                var start = DateTime.Now;

                //some work
                await Task.Delay(100);

                TimingScope.GetCurrent().Log("First", start, DateTime.Now);

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
            TimingScope.GetCurrent().Log("Second", start, DateTime.Now);
            TimingScope.GetCurrent().SetProperty("Prop3", "Val3");
        }

        private async Task OneMoreMethodAsync()
        {
            var start = DateTime.Now;
            await Task.Delay(100);
            TimingScope.GetCurrent().Log("Third", start, DateTime.Now, 1);

            start = DateTime.Now;
            await Task.Delay(300);
            TimingScope.GetCurrent().Log("Third", start, DateTime.Now, 2);
        }
    }
}
