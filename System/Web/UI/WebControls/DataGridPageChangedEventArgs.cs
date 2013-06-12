namespace System.Web.UI.WebControls
{
    using System;

    public class DataGridPageChangedEventArgs : EventArgs
    {
        private object commandSource;
        private int newPageIndex;

        public DataGridPageChangedEventArgs(object commandSource, int newPageIndex)
        {
            this.commandSource = commandSource;
            this.newPageIndex = newPageIndex;
        }

        public object CommandSource
        {
            get
            {
                return this.commandSource;
            }
        }

        public int NewPageIndex
        {
            get
            {
                return this.newPageIndex;
            }
        }
    }
}

