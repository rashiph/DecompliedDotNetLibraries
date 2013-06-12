namespace System.Drawing.Printing
{
    using System;
    using System.ComponentModel;

    public class PrintEventArgs : CancelEventArgs
    {
        private System.Drawing.Printing.PrintAction printAction;

        public PrintEventArgs()
        {
        }

        internal PrintEventArgs(System.Drawing.Printing.PrintAction action)
        {
            this.printAction = action;
        }

        public System.Drawing.Printing.PrintAction PrintAction
        {
            get
            {
                return this.printAction;
            }
        }
    }
}

