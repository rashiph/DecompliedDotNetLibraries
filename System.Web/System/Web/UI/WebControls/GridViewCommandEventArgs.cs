namespace System.Web.UI.WebControls
{
    using System;

    public class GridViewCommandEventArgs : CommandEventArgs
    {
        private object _commandSource;
        private GridViewRow _row;

        public GridViewCommandEventArgs(object commandSource, CommandEventArgs originalArgs) : base(originalArgs)
        {
            this._commandSource = commandSource;
        }

        public GridViewCommandEventArgs(GridViewRow row, object commandSource, CommandEventArgs originalArgs) : base(originalArgs)
        {
            this._row = row;
            this._commandSource = commandSource;
        }

        public object CommandSource
        {
            get
            {
                return this._commandSource;
            }
        }

        internal GridViewRow Row
        {
            get
            {
                return this._row;
            }
        }
    }
}

