namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms.Design;

    [ComVisible(true)]
    public class PropertyTabChangedEventArgs : EventArgs
    {
        private PropertyTab newTab;
        private PropertyTab oldTab;

        public PropertyTabChangedEventArgs(PropertyTab oldTab, PropertyTab newTab)
        {
            this.oldTab = oldTab;
            this.newTab = newTab;
        }

        public PropertyTab NewTab
        {
            get
            {
                return this.newTab;
            }
        }

        public PropertyTab OldTab
        {
            get
            {
                return this.oldTab;
            }
        }
    }
}

