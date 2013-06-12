namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IMethodCallMessage : IMethodMessage, IMessage
    {
        [SecurityCritical]
        object GetInArg(int argNum);
        [SecurityCritical]
        string GetInArgName(int index);

        int InArgCount { [SecurityCritical] get; }

        object[] InArgs { [SecurityCritical] get; }
    }
}

