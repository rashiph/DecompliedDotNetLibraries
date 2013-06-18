namespace System.DirectoryServices
{
    using System;
    using System.ComponentModel;

    public class DirectorySynchronization
    {
        private byte[] cookie;
        private DirectorySynchronizationOptions flag;

        public DirectorySynchronization()
        {
            this.cookie = new byte[0];
        }

        public DirectorySynchronization(DirectorySynchronization sync)
        {
            this.cookie = new byte[0];
            if (sync != null)
            {
                this.Option = sync.Option;
                this.ResetDirectorySynchronizationCookie(sync.GetDirectorySynchronizationCookie());
            }
        }

        public DirectorySynchronization(DirectorySynchronizationOptions option)
        {
            this.cookie = new byte[0];
            this.Option = option;
        }

        public DirectorySynchronization(byte[] cookie)
        {
            this.cookie = new byte[0];
            this.ResetDirectorySynchronizationCookie(cookie);
        }

        public DirectorySynchronization(DirectorySynchronizationOptions option, byte[] cookie)
        {
            this.cookie = new byte[0];
            this.Option = option;
            this.ResetDirectorySynchronizationCookie(cookie);
        }

        public DirectorySynchronization Copy()
        {
            return new DirectorySynchronization(this.flag, this.cookie);
        }

        public byte[] GetDirectorySynchronizationCookie()
        {
            byte[] buffer = new byte[this.cookie.Length];
            for (int i = 0; i < this.cookie.Length; i++)
            {
                buffer[i] = this.cookie[i];
            }
            return buffer;
        }

        public void ResetDirectorySynchronizationCookie()
        {
            this.cookie = new byte[0];
        }

        public void ResetDirectorySynchronizationCookie(byte[] cookie)
        {
            if (cookie == null)
            {
                this.cookie = new byte[0];
            }
            else
            {
                this.cookie = new byte[cookie.Length];
                for (int i = 0; i < cookie.Length; i++)
                {
                    this.cookie[i] = cookie[i];
                }
            }
        }

        [DefaultValue(0L), DSDescription("DSDirectorySynchronizationFlag")]
        public DirectorySynchronizationOptions Option
        {
            get
            {
                return this.flag;
            }
            set
            {
                long num = (long) (value & ~(DirectorySynchronizationOptions.IncrementalValues | DirectorySynchronizationOptions.ObjectSecurity | DirectorySynchronizationOptions.ParentsFirst | DirectorySynchronizationOptions.PublicDataOnly));
                if (num != 0L)
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DirectorySynchronizationOptions));
                }
                this.flag = value;
            }
        }
    }
}

