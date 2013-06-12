namespace System
{
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public abstract class ContextBoundObject : MarshalByRefObject
    {
        protected ContextBoundObject()
        {
        }
    }
}

