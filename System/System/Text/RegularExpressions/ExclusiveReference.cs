namespace System.Text.RegularExpressions
{
    using System;
    using System.Threading;

    internal sealed class ExclusiveReference
    {
        private int _locked;
        private object _obj;
        private RegexRunner _ref;

        internal object Get()
        {
            if (Interlocked.Exchange(ref this._locked, 1) != 0)
            {
                return null;
            }
            object obj2 = this._ref;
            if (obj2 == null)
            {
                this._locked = 0;
                return null;
            }
            this._obj = obj2;
            return obj2;
        }

        internal void Release(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (this._obj == obj)
            {
                this._obj = null;
                this._locked = 0;
            }
            else if ((this._obj == null) && (Interlocked.Exchange(ref this._locked, 1) == 0))
            {
                if (this._ref == null)
                {
                    this._ref = (RegexRunner) obj;
                }
                this._locked = 0;
            }
        }
    }
}

