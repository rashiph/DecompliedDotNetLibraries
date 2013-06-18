namespace System.Web.Services.Protocols
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    public class InvokeCompletedEventArgs : AsyncCompletedEventArgs
    {
        private object[] results;

        internal InvokeCompletedEventArgs(object[] results, Exception exception, bool cancelled, object userState) : base(exception, cancelled, userState)
        {
            this.results = results;
        }

        public object[] Results
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.results;
            }
        }
    }
}

