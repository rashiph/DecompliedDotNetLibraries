namespace System.Windows.Forms.PropertyGridInternal
{
    using System;

    internal class GridEntryRecreateChildrenEventArgs : EventArgs
    {
        public readonly int NewChildCount;
        public readonly int OldChildCount;

        public GridEntryRecreateChildrenEventArgs(int oldCount, int newCount)
        {
            this.OldChildCount = oldCount;
            this.NewChildCount = newCount;
        }
    }
}

