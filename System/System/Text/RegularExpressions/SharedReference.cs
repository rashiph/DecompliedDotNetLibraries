namespace System.Text.RegularExpressions
{
    using System;
    using System.Threading;

    internal sealed class SharedReference
    {
        private int _locked;
        private WeakReference _ref = new WeakReference(null);

        internal void Cache(object obj)
        {
            if (Interlocked.Exchange(ref this._locked, 1) == 0)
            {
                this._ref.Target = obj;
                this._locked = 0;
            }
        }

        internal object Get()
        {
            if (Interlocked.Exchange(ref this._locked, 1) == 0)
            {
                object target = this._ref.Target;
                this._locked = 0;
                return target;
            }
            return null;
        }
    }
}

