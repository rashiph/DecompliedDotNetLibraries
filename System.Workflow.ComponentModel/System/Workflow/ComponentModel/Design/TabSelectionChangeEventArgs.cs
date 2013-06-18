namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;

    internal sealed class TabSelectionChangeEventArgs : SelectionChangeEventArgs
    {
        private Rectangle selectedTabBounds;

        public TabSelectionChangeEventArgs(System.Workflow.ComponentModel.Design.ItemInfo previousItem, System.Workflow.ComponentModel.Design.ItemInfo currentItem, Rectangle selectedTabBounds) : base(previousItem, currentItem)
        {
            this.selectedTabBounds = Rectangle.Empty;
            this.selectedTabBounds = selectedTabBounds;
        }

        public Rectangle SelectedTabBounds
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.selectedTabBounds;
            }
        }
    }
}

