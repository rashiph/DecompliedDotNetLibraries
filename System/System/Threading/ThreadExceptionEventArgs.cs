namespace System.Threading
{
    using System;

    public class ThreadExceptionEventArgs : EventArgs
    {
        private System.Exception exception;

        public ThreadExceptionEventArgs(System.Exception t)
        {
            this.exception = t;
        }

        public System.Exception Exception
        {
            get
            {
                return this.exception;
            }
        }
    }
}

