namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class InvokeCompletedEventArgs : AsyncCompletedEventArgs
    {
        internal InvokeCompletedEventArgs(Exception error, bool cancelled, AsyncInvokeContext context) : base(error, cancelled, context.UserState)
        {
            this.Outputs = context.Outputs;
        }

        public IDictionary<string, object> Outputs { get; private set; }
    }
}

