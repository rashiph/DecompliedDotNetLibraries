namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;

    internal class GroupEnumerator : IEnumerator
    {
        internal int _curindex = -1;
        internal GroupCollection _rgc;

        internal GroupEnumerator(GroupCollection rgc)
        {
            this._rgc = rgc;
        }

        public bool MoveNext()
        {
            int count = this._rgc.Count;
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
                if ((this._curindex < 0) || (this._curindex >= this._rgc.Count))
                {
                    throw new InvalidOperationException(SR.GetString("EnumNotStarted"));
                }
                return this._rgc[this._curindex];
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

