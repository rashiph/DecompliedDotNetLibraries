namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class ObjectDataSourceFilteringEventArgs : CancelEventArgs
    {
        private IOrderedDictionary _parameterValues;

        public ObjectDataSourceFilteringEventArgs(IOrderedDictionary parameterValues)
        {
            this._parameterValues = parameterValues;
        }

        public IOrderedDictionary ParameterValues
        {
            get
            {
                return this._parameterValues;
            }
        }
    }
}

