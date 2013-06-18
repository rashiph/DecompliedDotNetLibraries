namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    internal sealed class KeyedPriorityQueueHeadChangedEventArgs<T> : EventArgs where T: class
    {
        private T newFirstElement;
        private T oldFirstElement;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public KeyedPriorityQueueHeadChangedEventArgs(T oldFirstElement, T newFirstElement)
        {
            this.oldFirstElement = oldFirstElement;
            this.newFirstElement = newFirstElement;
        }

        public T NewFirstElement
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.newFirstElement;
            }
        }

        public T OldFirstElement
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.oldFirstElement;
            }
        }
    }
}

