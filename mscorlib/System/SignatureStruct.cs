namespace System
{
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SignatureStruct
    {
        internal RuntimeType[] m_arguments;
        internal RuntimeType m_declaringType;
        internal RuntimeType m_returnTypeORfieldType;
        internal object m_keepalive;
        internal unsafe void* m_sig;
        internal unsafe void* m_pCallTarget;
        internal CallingConventions m_managedCallingConvention;
        internal int m_csig;
        internal int m_numVirtualFixedArgs;
        internal int m_64bitpad;
        internal RuntimeMethodHandleInternal m_pMethod;
        [SecuritySafeCritical]
        public unsafe SignatureStruct(RuntimeMethodHandleInternal method, RuntimeType[] arguments, RuntimeType returnType, CallingConventions callingConvention)
        {
            this.m_pMethod = method;
            this.m_arguments = arguments;
            this.m_returnTypeORfieldType = returnType;
            this.m_managedCallingConvention = callingConvention;
            this.m_sig = null;
            this.m_pCallTarget = null;
            this.m_csig = 0;
            this.m_numVirtualFixedArgs = 0;
            this.m_64bitpad = 0;
            this.m_declaringType = null;
            this.m_keepalive = null;
        }
    }
}

