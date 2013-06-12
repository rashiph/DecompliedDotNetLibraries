namespace System
{
    using System.Runtime;
    using System.Security;

    [ForceTokenStabilization]
    internal class RuntimeMethodInfoStub : IRuntimeMethodInfo
    {
        private object m_a;
        private object m_b;
        private object m_c;
        private object m_d;
        private object m_e;
        private object m_f;
        private object m_g;
        private object m_h;
        private object m_keepalive;
        [ForceTokenStabilization]
        public RuntimeMethodHandleInternal m_value;

        [SecurityCritical]
        public RuntimeMethodInfoStub(IntPtr methodHandleValue, object keepalive)
        {
            this.m_keepalive = keepalive;
            this.m_value = new RuntimeMethodHandleInternal(methodHandleValue);
        }

        public RuntimeMethodInfoStub(RuntimeMethodHandleInternal methodHandleValue, object keepalive)
        {
            this.m_keepalive = keepalive;
            this.m_value = methodHandleValue;
        }

        RuntimeMethodHandleInternal IRuntimeMethodInfo.Value
        {
            get
            {
                return this.m_value;
            }
        }
    }
}

