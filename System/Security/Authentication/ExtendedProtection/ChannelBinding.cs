namespace System.Security.Authentication.ExtendedProtection
{
    using Microsoft.Win32.SafeHandles;
    using System;

    public abstract class ChannelBinding : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected ChannelBinding() : base(true)
        {
        }

        protected ChannelBinding(bool ownsHandle) : base(ownsHandle)
        {
        }

        public abstract int Size { get; }
    }
}

