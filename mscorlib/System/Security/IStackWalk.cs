namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IStackWalk
    {
        void Assert();
        void Demand();
        void Deny();
        void PermitOnly();
    }
}

