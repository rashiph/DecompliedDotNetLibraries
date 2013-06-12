namespace System.Web.UI.WebControls
{
    using System;

    public class DataGridSortCommandEventArgs : EventArgs
    {
        private object commandSource;
        private string sortExpression;

        public DataGridSortCommandEventArgs(object commandSource, DataGridCommandEventArgs dce)
        {
            this.commandSource = commandSource;
            this.sortExpression = (string) dce.CommandArgument;
        }

        public object CommandSource
        {
            get
            {
                return this.commandSource;
            }
        }

        public string SortExpression
        {
            get
            {
                return this.sortExpression;
            }
        }
    }
}

