namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IRemotingTypeInfo
    {
        [SecurityCritical]
        bool CanCastTo(Type fromType, object o);

        string TypeName { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

