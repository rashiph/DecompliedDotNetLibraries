namespace System.Windows.Forms
{
    using System;

    public class BindingManagerDataErrorEventArgs : EventArgs
    {
        private System.Exception exception;

        public BindingManagerDataErrorEventArgs(System.Exception exception)
        {
            this.exception = exception;
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

