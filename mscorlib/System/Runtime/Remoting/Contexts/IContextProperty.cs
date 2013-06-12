namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IContextProperty
    {
        [SecurityCritical]
        void Freeze(Context newContext);
        [SecurityCritical]
        bool IsNewContextOK(Context newCtx);

        string Name { [SecurityCritical] get; }
    }
}

