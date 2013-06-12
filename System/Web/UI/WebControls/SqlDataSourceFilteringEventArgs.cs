namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class SqlDataSourceFilteringEventArgs : CancelEventArgs
    {
        private IOrderedDictionary _parameterValues;

        public SqlDataSourceFilteringEventArgs(IOrderedDictionary parameterValues)
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

