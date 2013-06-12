namespace System.Net.Cache
{
    using System;
    using System.Text;

    internal class ResponseCacheControl
    {
        internal int MaxAge;
        internal bool MustRevalidate;
        internal bool NoCache;
        internal string[] NoCacheHeaders;
        internal bool NoStore;
        internal bool Private;
        internal string[] PrivateHeaders;
        internal bool ProxyRevalidate;
        internal bool Public;
        internal int SMaxAge;

        internal ResponseCacheControl()
        {
            this.MaxAge = this.SMaxAge = -1;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (this.Public)
            {
                builder.Append(" public");
            }
            if (this.Private)
            {
                builder.Append(" private");
                if (this.PrivateHeaders != null)
                {
                    builder.Append('=');
                    for (int i = 0; i < (this.PrivateHeaders.Length - 1); i++)
                    {
                        builder.Append(this.PrivateHeaders[i]).Append(',');
                    }
                    builder.Append(this.PrivateHeaders[this.PrivateHeaders.Length - 1]);
                }
            }
            if (this.NoCache)
            {
                builder.Append(" no-cache");
                if (this.NoCacheHeaders != null)
                {
                    builder.Append('=');
                    for (int j = 0; j < (this.NoCacheHeaders.Length - 1); j++)
                    {
                        builder.Append(this.NoCacheHeaders[j]).Append(',');
                    }
                    builder.Append(this.NoCacheHeaders[this.NoCacheHeaders.Length - 1]);
                }
            }
            if (this.NoStore)
            {
                builder.Append(" no-store");
            }
            if (this.MustRevalidate)
            {
                builder.Append(" must-revalidate");
            }
            if (this.ProxyRevalidate)
            {
                builder.Append(" proxy-revalidate");
            }
            if (this.MaxAge != -1)
            {
                builder.Append(" max-age=").Append(this.MaxAge);
            }
            if (this.SMaxAge != -1)
            {
                builder.Append(" s-maxage=").Append(this.SMaxAge);
            }
            return builder.ToString();
        }

        internal bool IsNotEmpty
        {
            get
            {
                if (((!this.Public && !this.Private) && (!this.NoCache && !this.NoStore)) && ((!this.MustRevalidate && !this.ProxyRevalidate) && (this.MaxAge == -1)))
                {
                    return (this.SMaxAge != -1);
                }
                return true;
            }
        }
    }
}

