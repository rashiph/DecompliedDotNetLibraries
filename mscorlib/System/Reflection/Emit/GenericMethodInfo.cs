namespace System.Reflection.Emit
{
    using System;

    internal class GenericMethodInfo
    {
        internal RuntimeTypeHandle m_context;
        internal RuntimeMethodHandle m_methodHandle;

        internal GenericMethodInfo(RuntimeMethodHandle methodHandle, RuntimeTypeHandle context)
        {
            this.m_methodHandle = methodHandle;
            this.m_context = context;
        }
    }
}

