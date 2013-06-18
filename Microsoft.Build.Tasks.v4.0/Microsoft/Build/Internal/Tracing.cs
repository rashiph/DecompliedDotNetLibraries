namespace Microsoft.Build.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal static class Tracing
    {
        private static Dictionary<string, int> counts;
        private static string currentAssemblyName;
        private static TimeSpan interval;
        private static DateTime last = DateTime.MinValue;
        private static string slot = string.Empty;

        [Conditional("DEBUG")]
        internal static void Dump()
        {
            if (counts.Count > 0)
            {
                Trace.WriteLine(currentAssemblyName);
                foreach (KeyValuePair<string, int> pair in counts)
                {
                    Trace.WriteLine(string.Concat(new object[] { "# ", pair.Key, "=", pair.Value }));
                }
            }
        }

        [Conditional("DEBUG")]
        internal static void List<T>(IEnumerable<T> items)
        {
            foreach (T local in items)
            {
                Trace.WriteLine(local.ToString());
            }
        }

        [Conditional("DEBUG")]
        internal static void Record(string counter)
        {
            lock (counts)
            {
                int num;
                counts.TryGetValue(counter, out num);
                int num2 = ++num;
                counts[counter] = num2;
                DateTime now = DateTime.Now;
                if (now > (last + interval))
                {
                    Trace.WriteLine("================================================");
                    Trace.WriteLine(slot);
                    slot = string.Empty;
                    Trace.WriteLine(Environment.StackTrace);
                    last = now;
                }
            }
        }

        [Conditional("DEBUG")]
        internal static void Slot(string tag, string value)
        {
            lock (counts)
            {
                slot = tag + ": " + value;
            }
        }

        [Conditional("DEBUG")]
        internal static void Slot<K, V>(string tag, KeyValuePair<K, V> value)
        {
        }
    }
}

