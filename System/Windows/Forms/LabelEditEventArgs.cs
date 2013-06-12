namespace System.Windows.Forms
{
    using System;

    public class LabelEditEventArgs : EventArgs
    {
        private bool cancelEdit;
        private readonly int item;
        private readonly string label;

        public LabelEditEventArgs(int item)
        {
            this.item = item;
            this.label = null;
        }

        public LabelEditEventArgs(int item, string label)
        {
            this.item = item;
            this.label = label;
        }

        public bool CancelEdit
        {
            get
            {
                return this.cancelEdit;
            }
            set
            {
                this.cancelEdit = value;
            }
        }

        public int Item
        {
            get
            {
                return this.item;
            }
        }

        public string Label
        {
            get
            {
                return this.label;
            }
        }
    }
}

