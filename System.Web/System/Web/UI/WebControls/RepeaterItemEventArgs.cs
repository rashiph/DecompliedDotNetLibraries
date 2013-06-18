namespace System.Web.UI.WebControls
{
    using System;

    public class RepeaterItemEventArgs : EventArgs
    {
        private RepeaterItem item;

        public RepeaterItemEventArgs(RepeaterItem item)
        {
            this.item = item;
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

