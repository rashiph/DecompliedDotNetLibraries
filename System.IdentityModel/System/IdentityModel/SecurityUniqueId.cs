namespace System.IdentityModel
{
    using System;
    using System.Globalization;
    using System.Threading;

    internal class SecurityUniqueId
    {
        private static string commonPrefix = ("uuid-" + Guid.NewGuid().ToString() + "-");
        private long id;
        private static long nextId = 0L;
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
    }
}

