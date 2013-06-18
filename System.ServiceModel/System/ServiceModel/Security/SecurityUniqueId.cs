namespace System.ServiceModel.Security
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityUniqueId
    {
        private static long nextId;
        private static string commonPrefix;
        private long id;
        private string prefix;
        private string val;
        private SecurityUniqueId(string prefix, long id)
        {
            this.id = id;
            this.prefix = prefix;
            this.val = null;
        }

        public static SecurityUniqueId Create()
        {
            return Create(commonPrefix);
        }

        public static SecurityUniqueId Create(string prefix)
        {
            return new SecurityUniqueId(prefix, Interlocked.Increment(ref nextId));
        }

        public string Value
        {
            get
            {
                if (this.val == null)
                {
                    this.val = this.prefix + this.id.ToString(CultureInfo.InvariantCulture);
                }
                return this.val;
            }
        }
        static SecurityUniqueId()
        {
            nextId = 0L;
            commonPrefix = "uuid-" + Guid.NewGuid().ToString() + "-";
        }
    }
}

