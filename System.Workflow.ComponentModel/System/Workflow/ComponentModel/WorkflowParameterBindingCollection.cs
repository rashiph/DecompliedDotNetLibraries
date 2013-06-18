namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.ObjectModel;

    [Serializable]
    public sealed class WorkflowParameterBindingCollection : KeyedCollection<string, WorkflowParameterBinding>
    {
        private Activity ownerActivity;

        public WorkflowParameterBindingCollection(Activity ownerActivity)
        {
            if (ownerActivity == null)
            {
                throw new ArgumentNullException("ownerActivity");
            }
            this.ownerActivity = ownerActivity;
        }

        protected override void ClearItems()
        {
            if (!this.ownerActivity.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            base.ClearItems();
        }

        public WorkflowParameterBinding GetItem(string key)
        {
            return base[key];
        }

        protected override string GetKeyForItem(WorkflowParameterBinding item)
        {
            return item.ParameterName;
        }

        protected override void InsertItem(int index, WorkflowParameterBinding item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (!this.ownerActivity.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            if (base.Contains(item.ParameterName))
            {
                WorkflowParameterBinding binding = base[item.ParameterName];
                index = base.IndexOf(binding);
                this.RemoveItem(index);
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (!this.ownerActivity.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, WorkflowParameterBinding item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (!this.ownerActivity.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            base.SetItem(index, item);
        }
    }
}

