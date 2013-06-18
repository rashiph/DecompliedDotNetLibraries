namespace System.Management
{
    using System;
    using System.Runtime;

    public class ProgressEventArgs : ManagementEventArgs
    {
        private int current;
        private string message;
        private int upperBound;

        internal ProgressEventArgs(object context, int upperBound, int current, string message) : base(context)
        {
            this.upperBound = upperBound;
            this.current = current;
            this.message = message;
        }

        public int Current
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.current;
            }
        }

        public string Message
        {
            get
            {
                if (this.message == null)
                {
                    return string.Empty;
                }
                return this.message;
            }
        }

        public int UpperBound
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.upperBound;
            }
        }
    }
}

