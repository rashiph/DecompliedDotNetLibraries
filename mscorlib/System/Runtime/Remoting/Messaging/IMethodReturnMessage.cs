namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IMethodReturnMessage : IMethodMessage, IMessage
    {
        [SecurityCritical]
        object GetOutArg(int argNum);
        [SecurityCritical]
        string GetOutArgName(int index);

        System.Exception Exception { [SecurityCritical] get; }

        int OutArgCount { [SecurityCritical] get; }

        object[] OutArgs { [SecurityCritical] get; }

        object ReturnValue { [SecurityCritical] get; }
    }
}

