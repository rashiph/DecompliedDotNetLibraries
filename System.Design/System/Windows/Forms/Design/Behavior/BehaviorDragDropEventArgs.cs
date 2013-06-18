namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;

    public class BehaviorDragDropEventArgs : EventArgs
    {
        private ICollection dragComponents;

        public BehaviorDragDropEventArgs(ICollection dragComponents)
        {
            this.dragComponents = dragComponents;
        }

        public ICollection DragComponents
        {
            get
            {
                return this.dragComponents;
            }
        }
    }
}

