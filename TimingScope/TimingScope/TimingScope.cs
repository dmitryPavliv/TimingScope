using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimingScope
{
    public class TimingScope : IDisposable
    {
        private static readonly string Key = Guid.NewGuid().ToString();

        private readonly ConcurrentDictionary<string, string> _properties;
        private readonly ConcurrentBag<TimingLogEntry> _logEntries;
        private readonly DateTimeOffset _createdAt;

        private bool _disposed;

        private TimingScope()
        {
            _createdAt = DateTimeOffset.Now;

            _properties = new ConcurrentDictionary<string, string>();
            _logEntries = new ConcurrentBag<TimingLogEntry>();

            ObjectCallContext.ObjectCallContext.TrySetData(Key, this);
        }

        public static TimingScope Create()
        {
            return new TimingScope();
        }

        public static TimingScope Current
        {
            get
            {
                TimingScope current;
                ObjectCallContext.ObjectCallContext.TryGetData(Key, out current);
                return current;
            }
        }

        public TimingScope SetProperty(string name, string value)
        {
            ThrowIfAlreadyDisposed();
            _properties[name] = value;

            return this;
        }
        
        public DateTimeOffset CreatedAt { get { return _createdAt; } }

        public IDictionary<string, string> GetProperties()
        {
            ThrowIfAlreadyDisposed();
            return _properties.ToDictionary(x => x.Key, x => x.Value);
        }

        public void Log(string activityName, DateTimeOffset startedAt, DateTimeOffset finishedAt, long? duration = null)
        {
            ThrowIfAlreadyDisposed();
            if (!duration.HasValue)
            {
                duration = (long)(finishedAt - startedAt).TotalMilliseconds;
            }

            _logEntries.Add(new TimingLogEntry
            {
                Name = activityName,
                StartedAt = startedAt,
                FinishedAt = finishedAt,
                Duration = duration.Value
            });
        }

        public IList<TimingLogEntry> GetLogEntries()
        {
            ThrowIfAlreadyDisposed();
            return _logEntries.ToList();
        }

        public void Dispose()
        {
            TimingScope scope;
            ObjectCallContext.ObjectCallContext.TryRemove(Key, out scope);
            _disposed = true;
            if (this != scope)
            {
                throw new Exception("Timing scopes do not match");
            }
        }

        public override string ToString()
        {
            ThrowIfAlreadyDisposed();

            var sb = new StringBuilder();

            var properties = GetProperties();
            var logEntries = GetLogEntries();

            if (properties.Count > 0)
            {
                sb.AppendLine("-----------Context properties-----------").AppendLine();

                var maxLength = properties.Max(x => x.Key.Length);

                sb.AppendLine($"{"Property".PadRight(maxLength + 5)}\tValue").AppendLine();
                foreach (var property in properties)
                {
                    sb.AppendLine($"{property.Key.PadRight(maxLength + 5)}\t{property.Value}");
                }

                sb.AppendLine();
            }

            if (logEntries.Count > 0)
            {
                sb.AppendLine("-----------Time Log-----------").AppendLine();

                var groupedLogEntries = logEntries.GroupBy(k => k.Name, g => g, (k, g) => new
                {
                    Name = k,
                    TotalDuration = g.Sum(x => x.Duration),
                    Count = g.Count()
                }).OrderByDescending(x=>x.TotalDuration);

                var maxLength = groupedLogEntries.Max(x => x.Name.Length);

                sb.AppendLine($"{"Activity".PadRight(maxLength + 5)}\tAggregated Duration\tCount").AppendLine();

                foreach (var groupedLogEntry in groupedLogEntries)
                {
                    sb.AppendLine($"{groupedLogEntry.Name.PadRight(maxLength + 5)}\t" +
                                  $"{TimeSpan.FromMilliseconds(groupedLogEntry.TotalDuration):c}\t" +
                                  $"{groupedLogEntry.Count}");
                }

                sb.AppendLine();

                var totalDuration = logEntries.Sum(x => x.Duration);
                var totalTime = DateTimeOffset.Now - _createdAt;
                
                sb.AppendLine($"Aggregated Duration\t{TimeSpan.FromMilliseconds(totalDuration):c}");
                sb.AppendLine($"Total Duration     \t{totalTime:c}");
            }

            return sb.ToString();
        }
        
        private void ThrowIfAlreadyDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("TimingScope already disposed");
        }
        
    }
}
