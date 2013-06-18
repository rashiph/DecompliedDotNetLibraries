namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Runtime;

    internal class SelectionChangeEventArgs : EventArgs
    {
        private System.Workflow.ComponentModel.Design.ItemInfo currentItem;
        private System.Workflow.ComponentModel.Design.ItemInfo previousItem;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SelectionChangeEventArgs(System.Workflow.ComponentModel.Design.ItemInfo previousItem, System.Workflow.ComponentModel.Design.ItemInfo currentItem)
        {
            this.previousItem = previousItem;
            this.currentItem = currentItem;
        }

        public System.Workflow.ComponentModel.Design.ItemInfo CurrentItem
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.currentItem;
            }
        }
    }
}

