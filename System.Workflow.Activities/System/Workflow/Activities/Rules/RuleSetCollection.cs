namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class RuleSetCollection : KeyedCollection<string, RuleSet>, IWorkflowChangeDiff
    {
        private bool _runtimeInitialized;
        [NonSerialized]
        private object syncLock = new object();

        public void Add(RuleSet item)
        {
            if (this._runtimeInitialized)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (item.Name == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.InvalidRuleSetName, new object[] { "item.Name" }));
            }
            base.Add(item);
        }

        public IList<WorkflowChangeAction> Diff(object originalDefinition, object changedDefinition)
        {
            List<WorkflowChangeAction> list = new List<WorkflowChangeAction>();
            RuleSetCollection sets = (RuleSetCollection) originalDefinition;
            RuleSetCollection sets2 = (RuleSetCollection) changedDefinition;
            if (sets2 != null)
            {
                foreach (RuleSet set in sets2)
                {
                    if ((sets != null) && sets.Contains(set.Name))
                    {
                        RuleSet originalRuleSetDefinition = sets[set.Name];
                        if (!originalRuleSetDefinition.Equals(set))
                        {
                            list.Add(new UpdatedRuleSetAction(originalRuleSetDefinition, set));
                        }
                    }
                    else
                    {
                        list.Add(new AddedRuleSetAction(set));
                    }
                }
            }
            if (sets != null)
            {
                foreach (RuleSet set3 in sets)
                {
                    if ((sets2 == null) || !sets2.Contains(set3.Name))
                    {
                        list.Add(new RemovedRuleSetAction(set3));
                    }
                }
            }
            return list;
        }

        internal string GenerateRuleSetName()
        {
            string str2;
            string newRuleSetName = Messages.NewRuleSetName;
            int num = 1;
            do
            {
                str2 = newRuleSetName + num.ToString(CultureInfo.InvariantCulture);
                num++;
            }
            while (base.Contains(str2));
            return str2;
        }

        protected override string GetKeyForItem(RuleSet item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, RuleSet item)
        {
            if (this._runtimeInitialized)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            if (((item.Name != null) && (item.Name.Length >= 0)) && base.Contains(item.Name))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.RuleSetExists, new object[] { item.Name }));
            }
            base.InsertItem(index, item);
        }

        internal void OnRuntimeInitialized()
        {
            lock (this.syncLock)
            {
                if (!this._runtimeInitialized)
                {
                    foreach (RuleSet set in this)
                    {
                        set.OnRuntimeInitialized();
                    }
                    this._runtimeInitialized = true;
                }
            }
        }

        protected override void RemoveItem(int index)
        {
            if (this._runtimeInitialized)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, RuleSet item)
        {
            if (this._runtimeInitialized)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            base.SetItem(index, item);
        }

        internal bool RuntimeMode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._runtimeInitialized;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._runtimeInitialized = value;
            }
        }
    }
}

