namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IMethodMessage : IMessage
    {
        [SecurityCritical]
        object GetArg(int argNum);
        [SecurityCritical]
        string GetArgName(int index);

        int ArgCount { [SecurityCritical] get; }

        object[] Args { [SecurityCritical] get; }

        bool HasVarArgs { [SecurityCritical] get; }

        System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext { [SecurityCritical] get; }

        System.Reflection.MethodBase MethodBase { [SecurityCritical] get; }

        string MethodName { [SecurityCritical] get; }

        object MethodSignature { [SecurityCritical] get; }

        string TypeName { [SecurityCritical] get; }

        string Uri { [SecurityCritical] get; }
    }
}

