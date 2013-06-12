namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class ItemDragEventArgs : EventArgs
    {
        private readonly MouseButtons button;
        private readonly object item;

        public ItemDragEventArgs(MouseButtons button)
        {
            this.button = button;
            this.item = null;
        }

        public ItemDragEventArgs(MouseButtons button, object item)
        {
            this.button = button;
            this.item = item;
        }

        public MouseButtons Button
        {
            get
            {
                return this.button;
            }
        }

        public object Item
        {
            get
            {
                return this.item;
            }
        }
    }
}

