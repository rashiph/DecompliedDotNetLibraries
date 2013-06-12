namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Delegate, AllowMultiple=false, Inherited=false)]
    public sealed class UnmanagedFunctionPointerAttribute : Attribute
    {
        public bool BestFitMapping;
        public System.Runtime.InteropServices.CharSet CharSet;
        private System.Runtime.InteropServices.CallingConvention m_callingConvention;
        public bool SetLastError;
        public bool ThrowOnUnmappableChar;

        public UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention callingConvention)
        {
            this.m_callingConvention = callingConvention;
        }

        public System.Runtime.InteropServices.CallingConvention CallingConvention
        {
            get
            {
                return this.m_callingConvention;
            }
        }
    }
}

