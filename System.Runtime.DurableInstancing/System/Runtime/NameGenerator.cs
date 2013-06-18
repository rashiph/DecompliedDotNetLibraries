namespace System.Runtime
{
    using System;
    using System.Globalization;
    using System.Threading;

    internal class NameGenerator
    {
        private long id;
        private static NameGenerator nameGenerator = new NameGenerator();
        private string prefix = ("_" + Guid.NewGuid().ToString().Replace('-', '_') + "_");

        private NameGenerator()
        {
        }

        public static string Next()
        {
            long num = Interlocked.Increment(ref nameGenerator.id);
            return (nameGenerator.prefix + num.ToString(CultureInfo.InvariantCulture));
        }
    }
}

