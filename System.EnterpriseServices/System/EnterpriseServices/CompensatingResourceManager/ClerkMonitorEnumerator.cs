namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.Collections;
    using System.EnterpriseServices;

    internal class ClerkMonitorEnumerator : IEnumerator
    {
        private object _curElement;
        private int _curIndex = -1;
        private int _endIndex;
        private ClerkMonitor _monitor;
        private int _version;

        internal ClerkMonitorEnumerator(ClerkMonitor c)
        {
            this._monitor = c;
            this._version = c._version;
            this._endIndex = c.Count;
            this._curElement = null;
        }

        public virtual bool MoveNext()
        {
            if (this._version != this._monitor._version)
            {
                throw new InvalidOperationException(Resource.FormatString("InvalidOperation_EnumFailedVersion"));
            }
            if (this._curIndex < this._endIndex)
            {
                this._curIndex++;
            }
            if (this._curIndex < this._endIndex)
            {
                this._curElement = this._monitor[this._curIndex];
                return true;
            }
            this._curElement = null;
            return false;
        }

        public virtual void Reset()
        {
            if (this._version != this._monitor._version)
            {
                throw new InvalidOperationException(Resource.FormatString("InvalidOperation_EnumFailedVersion"));
            }
            this._curIndex = -1;
            this._curElement = null;
        }

        public virtual object Current
        {
            get
            {
                if (this._curIndex < 0)
                {
                    throw new InvalidOperationException(Resource.FormatString("InvalidOperation_EnumNotStarted"));
                }
                if (this._curIndex >= this._endIndex)
                {
                    throw new InvalidOperationException(Resource.FormatString("InvalidOperation_EnumEnded"));
                }
                return this._curElement;
            }
        }
    }
}

