namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class ObjectDataSourceMethodEventArgs : CancelEventArgs
    {
        private IOrderedDictionary _inputParameters;

        public ObjectDataSourceMethodEventArgs(IOrderedDictionary inputParameters)
        {
            this._inputParameters = inputParameters;
        }

        public IOrderedDictionary InputParameters
        {
            get
            {
                return this._inputParameters;
            }
        }
    }
}

