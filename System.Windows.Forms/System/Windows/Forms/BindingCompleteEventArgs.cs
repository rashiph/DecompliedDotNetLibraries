namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class BindingCompleteEventArgs : CancelEventArgs
    {
        private System.Windows.Forms.Binding binding;
        private System.Windows.Forms.BindingCompleteContext context;
        private string errorText;
        private System.Exception exception;
        private System.Windows.Forms.BindingCompleteState state;

        public BindingCompleteEventArgs(System.Windows.Forms.Binding binding, System.Windows.Forms.BindingCompleteState state, System.Windows.Forms.BindingCompleteContext context) : this(binding, state, context, string.Empty, null, false)
        {
        }

        public BindingCompleteEventArgs(System.Windows.Forms.Binding binding, System.Windows.Forms.BindingCompleteState state, System.Windows.Forms.BindingCompleteContext context, string errorText) : this(binding, state, context, errorText, null, true)
        {
        }

        public BindingCompleteEventArgs(System.Windows.Forms.Binding binding, System.Windows.Forms.BindingCompleteState state, System.Windows.Forms.BindingCompleteContext context, string errorText, System.Exception exception) : this(binding, state, context, errorText, exception, true)
        {
        }

        public BindingCompleteEventArgs(System.Windows.Forms.Binding binding, System.Windows.Forms.BindingCompleteState state, System.Windows.Forms.BindingCompleteContext context, string errorText, System.Exception exception, bool cancel) : base(cancel)
        {
            this.binding = binding;
            this.state = state;
            this.context = context;
            this.errorText = (errorText == null) ? string.Empty : errorText;
            this.exception = exception;
        }

        public System.Windows.Forms.Binding Binding
        {
            get
            {
                return this.binding;
            }
        }

        public System.Windows.Forms.BindingCompleteContext BindingCompleteContext
        {
            get
            {
                return this.context;
            }
        }

        public System.Windows.Forms.BindingCompleteState BindingCompleteState
        {
            get
            {
                return this.state;
            }
        }

        public string ErrorText
        {
            get
            {
                return this.errorText;
            }
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

