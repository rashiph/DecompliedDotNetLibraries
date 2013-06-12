namespace System.Diagnostics
{
    using System;

    internal class ProcessThreadTimes
    {
        internal long create;
        internal long exit;
        internal long kernel;
        internal long user;

        public DateTime ExitTime
        {
            get
            {
                return DateTime.FromFileTime(this.exit);
            }
        }

        public TimeSpan PrivilegedProcessorTime
        {
            get
            {
                return new TimeSpan(this.kernel);
            }
        }

        public DateTime StartTime
        {
            get
            {
                return DateTime.FromFileTime(this.create);
            }
        }

        public TimeSpan TotalProcessorTime
        {
            get
            {
                return new TimeSpan(this.user + this.kernel);
            }
        }

        public TimeSpan UserProcessorTime
        {
            get
            {
                return new TimeSpan(this.user);
            }
        }
    }
}

