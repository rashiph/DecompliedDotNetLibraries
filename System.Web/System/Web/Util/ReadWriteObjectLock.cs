namespace System.Web.Util
{
    using System;
    using System.Threading;

    internal class ReadWriteObjectLock
    {
        private int _lock;

        internal ReadWriteObjectLock()
        {
        }

        internal virtual void AcquireRead()
        {
            lock (this)
            {
                while (this._lock == -1)
                {
                    try
                    {
                        Monitor.Wait(this);
                        continue;
                    }
                    catch (ThreadInterruptedException)
                    {
                        continue;
                    }
                }
                this._lock++;
            }
        }

        internal virtual void AcquireWrite()
        {
            lock (this)
            {
                while (this._lock != 0)
                {
                    try
                    {
                        Monitor.Wait(this);
                        continue;
                    }
                    catch (ThreadInterruptedException)
                    {
                        continue;
                    }
                }
                this._lock = -1;
            }
        }

        internal virtual void ReleaseRead()
        {
            lock (this)
            {
                this._lock--;
                if (this._lock == 0)
                {
                    Monitor.PulseAll(this);
                }
            }
        }

        internal virtual void ReleaseWrite()
        {
            lock (this)
            {
                this._lock = 0;
                Monitor.PulseAll(this);
            }
        }
    }
}

