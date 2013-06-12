namespace System.Runtime.ExceptionServices
{
    using System;
    using System.Runtime.ConstrainedExecution;

    public class FirstChanceExceptionEventArgs : EventArgs
    {
        private System.Exception m_Exception;

        public FirstChanceExceptionEventArgs(System.Exception exception)
        {
            this.m_Exception = exception;
        }

        public System.Exception Exception
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this.m_Exception;
            }
        }
    }
}

