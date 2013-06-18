namespace System.Web.UI.WebControls
{
    using System;

    public class DataGridCommandEventArgs : CommandEventArgs
    {
        private object commandSource;
        private DataGridItem item;

        public DataGridCommandEventArgs(DataGridItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs)
        {
            this.item = item;
            this.commandSource = commandSource;
        }

        public object CommandSource
        {
            get
            {
                return this.commandSource;
            }
        }

        public DataGridItem Item
        {
            get
            {
                return this.item;
            }
        }
    }
}

