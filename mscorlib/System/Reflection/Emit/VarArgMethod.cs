namespace System.Reflection.Emit
{
    using System;
    using System.Reflection;

    internal class VarArgMethod
    {
        internal DynamicMethod m_dynamicMethod;
        internal RuntimeMethodInfo m_method;
        internal SignatureHelper m_signature;

        internal VarArgMethod(RuntimeMethodInfo method, DynamicMethod dm, SignatureHelper signature)
        {
            this.m_method = method;
            this.m_dynamicMethod = dm;
            this.m_signature = signature;
        }
    }
}

