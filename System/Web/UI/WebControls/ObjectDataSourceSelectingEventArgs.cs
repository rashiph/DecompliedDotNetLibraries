namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Web.UI;

    public class ObjectDataSourceSelectingEventArgs : ObjectDataSourceMethodEventArgs
    {
        private DataSourceSelectArguments _arguments;
        private bool _executingSelectCount;

        public ObjectDataSourceSelectingEventArgs(IOrderedDictionary inputParameters, DataSourceSelectArguments arguments, bool executingSelectCount) : base(inputParameters)
        {
            this._arguments = arguments;
            this._executingSelectCount = executingSelectCount;
        }

        public DataSourceSelectArguments Arguments
        {
            get
            {
                return this._arguments;
            }
        }

        public bool ExecutingSelectCount
        {
            get
            {
                return this._executingSelectCount;
            }
        }
    }
}

