namespace Microsoft.JScript
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    internal class TokenColorInfoList : ITokenEnumerator
    {
        private bool _atEnd = true;
        private TokenColorInfo _current = null;
        private TokenColorInfo _head = null;

        internal TokenColorInfoList()
        {
        }

        internal void Add(Context token)
        {
            TokenColorInfo info = null;
            if (this._head == null)
            {
                info = new TokenColorInfo(token);
                this._head = info;
            }
            else
            {
                info = this._head.Clone();
                this._head._next = info;
                this._head.UpdateToken(token);
                this._head = info;
            }
            this._current = this._head;
            this._atEnd = false;
        }

        public virtual ITokenColorInfo GetNext()
        {
            if (this._atEnd)
            {
                return null;
            }
            ITokenColorInfo info = this._current;
            this._current = this._current._next;
            this._atEnd = this._current == this._head;
            return info;
        }

        public virtual void Reset()
        {
            this._current = this._head;
            if (this._current != null)
            {
                this._atEnd = false;
            }
        }
    }
}

