namespace System.Net
{
    using System;

    internal class HttpListenerTimeoutManager
    {
        private const int defaulServerTimeout = 120;
        private const uint defaultMinSendRate = 150;
        private HttpListener listener;
        private uint minSendRate;
        private int[] timeouts;

        internal HttpListenerTimeoutManager(HttpListener context)
        {
            this.listener = context;
            this.timeouts = new int[5];
        }

        private TimeSpan GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE type)
        {
            return new TimeSpan(0, 0, this.timeouts[(int) type]);
        }

        private void SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE type, TimeSpan value)
        {
            long num = Convert.ToInt64(value.TotalSeconds);
            if ((num < 0L) || (num > 0xffffL))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            int[] timeouts = this.timeouts;
            timeouts[(int) type] = (int) num;
            this.listener.SetServerTimeout(timeouts, this.minSendRate);
            this.timeouts[(int) type] = (int) num;
        }

        public TimeSpan DrainEntityBody
        {
            get
            {
                return this.GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.DrainEntityBody);
            }
            set
            {
                this.SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.DrainEntityBody, value);
            }
        }

        public TimeSpan EntityBody
        {
            get
            {
                return this.GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.EntityBody);
            }
            set
            {
                this.SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.EntityBody, value);
            }
        }

        public TimeSpan HeaderWait
        {
            get
            {
                return this.GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.HeaderWait);
            }
            set
            {
                this.SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.HeaderWait, value);
            }
        }

        public TimeSpan IdleConnection
        {
            get
            {
                return this.GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.IdleConnection);
            }
            set
            {
                this.SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.IdleConnection, value);
            }
        }

        public long MinSendRate
        {
            get
            {
                return (long) this.minSendRate;
            }
            set
            {
                if ((value < 0L) || (value > 0xffffffffL))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.listener.SetServerTimeout(this.timeouts, (uint) value);
                this.minSendRate = (uint) value;
            }
        }

        public TimeSpan RequestQueue
        {
            get
            {
                return this.GetTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.RequestQueue);
            }
            set
            {
                this.SetTimespanTimeout(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.RequestQueue, value);
            }
        }
    }
}

