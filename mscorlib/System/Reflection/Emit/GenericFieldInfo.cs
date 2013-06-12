namespace System.Reflection.Emit
{
    using System;

    internal class GenericFieldInfo
    {
        internal RuntimeTypeHandle m_context;
        internal RuntimeFieldHandle m_fieldHandle;

        internal GenericFieldInfo(RuntimeFieldHandle fieldHandle, RuntimeTypeHandle context)
        {
            this.m_fieldHandle = fieldHandle;
            this.m_context = context;
        }
    }
}

