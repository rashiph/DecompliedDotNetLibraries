namespace System.Runtime.Remoting.Messaging
{
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IMessage
    {
        IDictionary Properties { [SecurityCritical] get; }
    }
}

