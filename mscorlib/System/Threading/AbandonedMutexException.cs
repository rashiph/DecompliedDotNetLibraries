namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(false)]
    public class AbandonedMutexException : SystemException
    {
        private System.Threading.Mutex m_Mutex;
        private int m_MutexIndex;

        public AbandonedMutexException() : base(Environment.GetResourceString("Threading.AbandonedMutexException"))
        {
            this.m_MutexIndex = -1;
            base.SetErrorCode(-2146233043);
        }

        public AbandonedMutexException(string message) : base(message)
        {
            this.m_MutexIndex = -1;
            base.SetErrorCode(-2146233043);
        }

        public AbandonedMutexException(int location, WaitHandle handle) : base(Environment.GetResourceString("Threading.AbandonedMutexException"))
        {
            this.m_MutexIndex = -1;
            base.SetErrorCode(-2146233043);
            this.SetupException(location, handle);
        }

        [SecuritySafeCritical]
        protected AbandonedMutexException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.m_MutexIndex = -1;
        }

        public AbandonedMutexException(string message, Exception inner) : base(message, inner)
        {
            this.m_MutexIndex = -1;
            base.SetErrorCode(-2146233043);
        }

        public AbandonedMutexException(string message, int location, WaitHandle handle) : base(message)
        {
            this.m_MutexIndex = -1;
            base.SetErrorCode(-2146233043);
            this.SetupException(location, handle);
        }

        public AbandonedMutexException(string message, Exception inner, int location, WaitHandle handle) : base(message, inner)
        {
            this.m_MutexIndex = -1;
            base.SetErrorCode(-2146233043);
            this.SetupException(location, handle);
        }

        private void SetupException(int location, WaitHandle handle)
        {
            this.m_MutexIndex = location;
            if (handle != null)
            {
                this.m_Mutex = handle as System.Threading.Mutex;
            }
        }

        public System.Threading.Mutex Mutex
        {
            get
            {
                return this.m_Mutex;
            }
        }

        public int MutexIndex
        {
            get
            {
                return this.m_MutexIndex;
            }
        }
    }
}

