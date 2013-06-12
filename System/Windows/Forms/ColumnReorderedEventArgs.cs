namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class ColumnReorderedEventArgs : CancelEventArgs
    {
        private ColumnHeader header;
        private int newDisplayIndex;
        private int oldDisplayIndex;

        public ColumnReorderedEventArgs(int oldDisplayIndex, int newDisplayIndex, ColumnHeader header)
        {
            this.oldDisplayIndex = oldDisplayIndex;
            this.newDisplayIndex = newDisplayIndex;
            this.header = header;
        }

        public ColumnHeader Header
        {
            get
            {
                return this.header;
            }
        }

        public int NewDisplayIndex
        {
            get
            {
                return this.newDisplayIndex;
            }
        }

        public int OldDisplayIndex
        {
            get
            {
                return this.oldDisplayIndex;
            }
        }
    }
}

