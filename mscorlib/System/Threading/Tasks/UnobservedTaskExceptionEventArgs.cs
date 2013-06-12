namespace System.Threading.Tasks
{
    using System;

    public class UnobservedTaskExceptionEventArgs : EventArgs
    {
        private AggregateException m_exception;
        internal bool m_observed;

        public UnobservedTaskExceptionEventArgs(AggregateException exception)
        {
            this.m_exception = exception;
        }

        public void SetObserved()
        {
            this.m_observed = true;
        }

        public AggregateException Exception
        {
            get
            {
                return this.m_exception;
            }
        }

        public bool Observed
        {
            get
            {
                return this.m_observed;
            }
        }
    }
}

