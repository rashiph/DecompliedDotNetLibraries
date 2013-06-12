namespace System.Web.UI.WebControls
{
    using System;

    public class FormViewCommandEventArgs : CommandEventArgs
    {
        private object _commandSource;

        public FormViewCommandEventArgs(object commandSource, CommandEventArgs originalArgs) : base(originalArgs)
        {
            this._commandSource = commandSource;
        }

        public object CommandSource
        {
            get
            {
                return this._commandSource;
            }
        }
    }
}

