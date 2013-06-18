namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Web.UI;

    internal sealed class ReadOnlyDataSourceView : DataSourceView
    {
        private IEnumerable _dataSource;

        public ReadOnlyDataSourceView(ReadOnlyDataSource owner, string name, IEnumerable dataSource) : base(owner, name)
        {
            this._dataSource = dataSource;
        }

        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            arguments.RaiseUnsupportedCapabilitiesError(this);
            return this._dataSource;
        }
    }
}

