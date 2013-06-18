namespace System.Runtime.Collections
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal class ValidatingCollection<T> : Collection<T>
    {
        protected override void ClearItems()
        {
            this.OnMutate();
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            this.OnAdd(item);
            base.InsertItem(index, item);
        }

        private void OnAdd(T item)
        {
            if (this.OnAddValidationCallback != null)
            {
                this.OnAddValidationCallback(item);
            }
        }

        private void OnMutate()
        {
            if (this.OnMutateValidationCallback != null)
            {
                this.OnMutateValidationCallback();
            }
        }

        protected override void RemoveItem(int index)
        {
            this.OnMutate();
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            this.OnAdd(item);
            this.OnMutate();
            base.SetItem(index, item);
        }

        public Action<T> OnAddValidationCallback
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<OnAddValidationCallback>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<OnAddValidationCallback>k__BackingField = value;
            }
        }

        public Action OnMutateValidationCallback
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<OnMutateValidationCallback>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<OnMutateValidationCallback>k__BackingField = value;
            }
        }
    }
}

