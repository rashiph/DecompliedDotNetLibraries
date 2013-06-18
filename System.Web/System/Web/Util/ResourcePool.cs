namespace System.Web.Util
{
    using System;
    using System.Collections;
    using System.Threading;

    internal class ResourcePool : IDisposable
    {
        private TimerCallback _callback;
        private bool _disposed;
        private int _iDisposable;
        private TimeSpan _interval;
        private int _max;
        private ArrayList _resources;
        private Timer _timer;

        internal ResourcePool(TimeSpan interval, int max)
        {
            this._interval = interval;
            this._resources = new ArrayList(4);
            this._max = max;
            this._callback = new TimerCallback(this.TimerProc);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    if (!this._disposed)
                    {
                        if (this._resources != null)
                        {
                            foreach (IDisposable disposable in this._resources)
                            {
                                disposable.Dispose();
                            }
                            this._resources.Clear();
                        }
                        if (this._timer != null)
                        {
                            this._timer.Dispose();
                        }
                        this._disposed = true;
                    }
                }
            }
        }

        internal object RetrieveResource()
        {
            object obj2 = null;
            if (this._resources.Count != 0)
            {
                lock (this)
                {
                    if (this._disposed)
                    {
                        return obj2;
                    }
                    if (this._resources.Count == 0)
                    {
                        return null;
                    }
                    obj2 = this._resources[this._resources.Count - 1];
                    this._resources.RemoveAt(this._resources.Count - 1);
                    if (this._resources.Count < this._iDisposable)
                    {
                        this._iDisposable = this._resources.Count;
                    }
                }
            }
            return obj2;
        }

        internal void StoreResource(IDisposable o)
        {
            lock (this)
            {
                if (!this._disposed && (this._resources.Count < this._max))
                {
                    this._resources.Add(o);
                    o = null;
                    if (this._timer == null)
                    {
                        this._timer = new Timer(this._callback, null, this._interval, this._interval);
                    }
                }
            }
            if (o != null)
            {
                o.Dispose();
            }
        }

        private void TimerProc(object userData)
        {
            IDisposable[] array = null;
            lock (this)
            {
                if (!this._disposed)
                {
                    if (this._resources.Count == 0)
                    {
                        if (this._timer != null)
                        {
                            this._timer.Dispose();
                            this._timer = null;
                        }
                        return;
                    }
                    array = new IDisposable[this._iDisposable];
                    this._resources.CopyTo(0, array, 0, this._iDisposable);
                    this._resources.RemoveRange(0, this._iDisposable);
                    this._iDisposable = this._resources.Count;
                }
            }
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    try
                    {
                        array[i].Dispose();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}

