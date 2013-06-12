namespace System.Runtime.InteropServices
{
    using System;

    [Serializable]
    public sealed class VariantWrapper
    {
        private object m_WrappedObject;

        public VariantWrapper(object obj)
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

