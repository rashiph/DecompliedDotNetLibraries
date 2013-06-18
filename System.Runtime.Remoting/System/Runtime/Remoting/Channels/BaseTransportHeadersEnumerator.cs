namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;

    internal class BaseTransportHeadersEnumerator : IEnumerator
    {
        private bool _bStarted;
        private int _currentIndex;
        private BaseTransportHeaders _headers;
        private IEnumerator _otherHeadersEnumerator;

        public BaseTransportHeadersEnumerator(BaseTransportHeaders headers)
        {
            this._headers = headers;
            this.Reset();
        }

        public bool MoveNext()
        {
            if (this._currentIndex != -1)
            {
                if (this._bStarted)
                {
                    this._currentIndex++;
                }
                else
                {
                    this._bStarted = true;
                }
                while (this._currentIndex != -1)
                {
                    if (this._currentIndex >= 4)
                    {
                        this._otherHeadersEnumerator = this._headers.GetOtherHeadersEnumerator();
                        this._currentIndex = -1;
                    }
                    else
                    {
                        if (this._headers.GetValueFromHeaderIndex(this._currentIndex) != null)
                        {
                            return true;
                        }
                        this._currentIndex++;
                    }
                }
            }
            if (this._otherHeadersEnumerator == null)
            {
                return false;
            }
            if (!this._otherHeadersEnumerator.MoveNext())
            {
                this._otherHeadersEnumerator = null;
                return false;
            }
            return true;
        }

        public void Reset()
        {
            this._bStarted = false;
            this._currentIndex = 0;
            this._otherHeadersEnumerator = null;
        }

        public object Current
        {
            get
            {
                if (this._bStarted)
                {
                    if (this._currentIndex != -1)
                    {
                        return new DictionaryEntry(this._headers.MapHeaderIndexToName(this._currentIndex), this._headers.GetValueFromHeaderIndex(this._currentIndex));
                    }
                    if (this._otherHeadersEnumerator != null)
                    {
                        return this._otherHeadersEnumerator.Current;
                    }
                }
                return null;
            }
        }
    }
}

