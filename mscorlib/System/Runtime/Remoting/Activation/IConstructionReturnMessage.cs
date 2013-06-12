namespace System.Runtime.Remoting.Activation
{
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;

    [ComVisible(true)]
    public interface IConstructionReturnMessage : IMethodReturnMessage, IMethodMessage, IMessage
    {
    }
}

