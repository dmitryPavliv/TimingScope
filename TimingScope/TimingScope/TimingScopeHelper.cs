using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TimingScope
{
    public static class TimingScopeHelper
    {
        public static async Task WithTimingAsync(string activityName, Func<Task> taskFunc)
        {
            var start = DateTime.Now;
            var sw = Stopwatch.StartNew();

            try
            {
                await taskFunc();
            }
            finally
            {
                sw.Stop();
                TimingScope.Current?.Log(activityName, start, DateTime.Now, sw.ElapsedMilliseconds);
            }
        }

        public static async Task<T> WithTimingAsync<T>(string activityName, Func<Task<T>> taskFunc)
        {
            var start = DateTime.Now;
            var sw = Stopwatch.StartNew();

            try
            {
                return await taskFunc();
            }
            finally
            {
                sw.Stop();
                TimingScope.Current?.Log(activityName, start, DateTime.Now, sw.ElapsedMilliseconds);
            }
        }

        public static void WithTiming(string activityName, Action action)
        {
            var start = DateTime.Now;
            var sw = Stopwatch.StartNew();

            try
            {
                action();
            }
            finally
            {
                sw.Stop();
                TimingScope.Current?.Log(activityName, start, DateTime.Now, sw.ElapsedMilliseconds);
            }
        }

        public static T WithTiming<T>(string activityName, Func<T> func)
        {
            var start = DateTime.Now;
            var sw = Stopwatch.StartNew();

            try
            {
                return func();
            }
            finally
            {
                sw.Stop();
                TimingScope.Current?.Log(activityName, start, DateTime.Now, sw.ElapsedMilliseconds);
            }
        }
    }
}