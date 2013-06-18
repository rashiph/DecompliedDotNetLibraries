namespace System.Web
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    internal sealed class HttpChannelBindingToken : ChannelBinding
    {
        private int _size;

        internal HttpChannelBindingToken(IntPtr token, int tokenSize)
        {
            base.SetHandle(token);
            this._size = tokenSize;
        }

        protected override bool ReleaseHandle()
        {
            base.SetHandle(IntPtr.Zero);
            this._size = 0;
            return true;
        }

        public override int Size
        {
            get
            {
                return this._size;
            }
        }
    }
}

