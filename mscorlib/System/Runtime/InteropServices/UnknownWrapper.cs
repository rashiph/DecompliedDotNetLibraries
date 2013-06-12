namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, ComVisible(true)]
    public sealed class UnknownWrapper
    {
        private object m_WrappedObject;

        public UnknownWrapper(object obj)
        {
            this.m_WrappedObject = obj;
        }

        public object WrappedObject
        {
            get
            {
                return this.m_WrappedObject;
            }
        }
    }
}

