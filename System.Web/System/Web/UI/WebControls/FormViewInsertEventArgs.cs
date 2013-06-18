namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class FormViewInsertEventArgs : CancelEventArgs
    {
        private object _commandArgument;
        private OrderedDictionary _values;

        public FormViewInsertEventArgs(object commandArgument) : base(false)
        {
            this._commandArgument = commandArgument;
        }

        public object CommandArgument
        {
            get
            {
                return this._commandArgument;
            }
        }

        public IOrderedDictionary Values
        {
            get
            {
                if (this._values == null)
                {
                    this._values = new OrderedDictionary();
                }
                return this._values;
            }
        }
    }
}

