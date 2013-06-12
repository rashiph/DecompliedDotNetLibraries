namespace System.Windows.Forms
{
    using System;
    using System.Collections.Generic;

    internal class MergeHistory
    {
        private ToolStrip mergedToolStrip;
        private Stack<MergeHistoryItem> mergeHistoryItemsStack;

        public MergeHistory(ToolStrip mergedToolStrip)
        {
            this.mergedToolStrip = mergedToolStrip;
        }

        public ToolStrip MergedToolStrip
        {
            get
            {
                return this.mergedToolStrip;
            }
        }

        public Stack<MergeHistoryItem> MergeHistoryItemsStack
        {
            get
            {
                if (this.mergeHistoryItemsStack == null)
                {
                    this.mergeHistoryItemsStack = new Stack<MergeHistoryItem>();
                }
                return this.mergeHistoryItemsStack;
            }
        }
    }
}

