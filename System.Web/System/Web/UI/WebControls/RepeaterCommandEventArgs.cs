namespace System.Web.UI.WebControls
{
    using System;

    public class RepeaterCommandEventArgs : CommandEventArgs
    {
        private object commandSource;
        private RepeaterItem item;

        public RepeaterCommandEventArgs(RepeaterItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs)
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

        public RepeaterItem Item
        {
            get
            {
                return this.item;
            }
        }
    }
}

