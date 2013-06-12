namespace System.Windows.Forms
{
    using System;

    public class TabControlEventArgs : EventArgs
    {
        private TabControlAction action;
        private System.Windows.Forms.TabPage tabPage;
        private int tabPageIndex;

        public TabControlEventArgs(System.Windows.Forms.TabPage tabPage, int tabPageIndex, TabControlAction action)
        {
            this.tabPage = tabPage;
            this.tabPageIndex = tabPageIndex;
            this.action = action;
        }

        public TabControlAction Action
        {
            get
            {
                return this.action;
            }
        }

        public System.Windows.Forms.TabPage TabPage
        {
            get
            {
                return this.tabPage;
            }
        }

        public int TabPageIndex
        {
            get
            {
                return this.tabPageIndex;
            }
        }
    }
}

