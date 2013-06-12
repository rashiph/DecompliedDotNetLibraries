namespace System
{
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public class UnhandledExceptionEventArgs : EventArgs
    {
        private object _Exception;
        private bool _IsTerminating;

        public UnhandledExceptionEventArgs(object exception, bool isTerminating)
        {
            this._Exception = exception;
            this._IsTerminating = isTerminating;
        }

        public object ExceptionObject
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._Exception;
            }
        }

        public bool IsTerminating
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._IsTerminating;
            }
        }
    }
}

