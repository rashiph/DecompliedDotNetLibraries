namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface ISurrogateSelector
    {
        [SecurityCritical]
        void ChainSelector(ISurrogateSelector selector);
        [SecurityCritical]
        ISurrogateSelector GetNextSelector();
        [SecurityCritical]
        ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector);
    }
}

