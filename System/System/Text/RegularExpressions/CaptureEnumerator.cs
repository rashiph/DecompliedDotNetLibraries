namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;

    [Serializable]
    internal class CaptureEnumerator : IEnumerator
    {
        internal int _curindex = -1;
        internal CaptureCollection _rcc;

        internal CaptureEnumerator(CaptureCollection rcc)
        {
            this._rcc = rcc;
        }

        public bool MoveNext()
        {
            int count = this._rcc.Count;
            if (this._curindex >= count)
            {
                return false;
            }
            this._curindex++;
            return (this._curindex < count);
        }

        public void Reset()
        {
            this._curindex = -1;
        }

        public System.Text.RegularExpressions.Capture Capture
        {
            get
            {
                if ((this._curindex < 0) || (this._curindex >= this._rcc.Count))
                {
                    throw new InvalidOperationException(SR.GetString("EnumNotStarted"));
                }
                return this._rcc[this._curindex];
            }
        }

        public object Current
        {
            get
            {
                return this.Capture;
            }
        }
    }
}

