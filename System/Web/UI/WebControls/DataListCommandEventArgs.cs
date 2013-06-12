namespace System.Web.UI.WebControls
{
    using System;

    public class DataListCommandEventArgs : CommandEventArgs
    {
        private object commandSource;
        private DataListItem item;

        public DataListCommandEventArgs(DataListItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs)
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

        public DataListItem Item
        {
            get
            {
                return this.item;
            }
        }
    }
}

