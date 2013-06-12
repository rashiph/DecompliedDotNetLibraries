namespace System.Security.Util
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TokenBasedSetEnumerator
    {
        public object Current;
        public int Index;
        private TokenBasedSet _tb;
        public bool MoveNext()
        {
            if (this._tb == null)
            {
                return false;
            }
            return this._tb.MoveNext(ref this);
        }

        public void Reset()
        {
            this.Index = -1;
            this.Current = null;
        }

        public TokenBasedSetEnumerator(TokenBasedSet tb)
        {
            this.Index = -1;
            this.Current = null;
            this._tb = tb;
        }
    }
}

