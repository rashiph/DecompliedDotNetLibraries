namespace System.Net.NetworkInformation
{
    using System;

    public class PingOptions
    {
        private bool dontFragment;
        private const int DontFragmentFlag = 2;
        private int ttl;

        public PingOptions()
        {
            this.ttl = 0x80;
        }

        internal PingOptions(IPOptions options)
        {
            this.ttl = 0x80;
            this.ttl = options.ttl;
            this.dontFragment = (options.flags & 2) > 0;
        }

        public PingOptions(int ttl, bool dontFragment)
        {
            this.ttl = 0x80;
            if (ttl <= 0)
            {
                throw new ArgumentOutOfRangeException("ttl");
            }
            this.ttl = ttl;
            this.dontFragment = dontFragment;
        }

        public bool DontFragment
        {
            get
            {
                return this.dontFragment;
            }
            set
            {
                this.dontFragment = value;
            }
        }

        public int Ttl
        {
            get
            {
                return this.ttl;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ttl = value;
            }
        }
    }
}

