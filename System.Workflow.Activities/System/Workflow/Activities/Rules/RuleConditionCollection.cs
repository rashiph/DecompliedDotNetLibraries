namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    public sealed class RuleConditionCollection : KeyedCollection<string, RuleCondition>, IWorkflowChangeDiff
    {
        [NonSerialized]
        private object _runtimeInitializationLock = new object();
        private bool _runtimeInitialized;

        public void Add(RuleCondition item)
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
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.InvalidConditionName, new object[] { "item.Name" }));
            }
            base.Add(item);
        }

        public IList<WorkflowChangeAction> Diff(object originalDefinition, object changedDefinition)
        {
            List<WorkflowChangeAction> list = new List<WorkflowChangeAction>();
            RuleConditionCollection conditions = (RuleConditionCollection) originalDefinition;
            RuleConditionCollection conditions2 = (RuleConditionCollection) changedDefinition;
            if (conditions2 != null)
            {
                foreach (RuleCondition condition in conditions2)
                {
                    if (conditions != null)
                    {
                        if (conditions.Contains(condition.Name))
                        {
                            RuleCondition conditionDefinition = conditions[condition.Name];
                            if (!conditionDefinition.Equals(condition))
                            {
                                list.Add(new UpdatedConditionAction(conditionDefinition, condition));
                            }
                        }
                        else
                        {
                            list.Add(new AddedConditionAction(condition));
                        }
                    }
                    else
                    {
                        list.Add(new AddedConditionAction(condition));
                    }
                }
            }
            if (conditions != null)
            {
                foreach (RuleCondition condition3 in conditions)
                {
                    if (conditions2 != null)
                    {
                        if (!conditions2.Contains(condition3.Name))
                        {
                            list.Add(new RemovedConditionAction(condition3));
                        }
                    }
                    else
                    {
                        list.Add(new RemovedConditionAction(condition3));
                    }
                }
            }
            return list;
        }

        protected override string GetKeyForItem(RuleCondition item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, RuleCondition item)
        {
            if (this._runtimeInitialized)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            if (((item.Name != null) && (item.Name.Length >= 0)) && base.Contains(item.Name))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.ConditionExists, new object[] { item.Name }));
            }
            base.InsertItem(index, item);
        }

        internal void OnRuntimeInitialized()
        {
            lock (this._runtimeInitializationLock)
            {
                if (!this._runtimeInitialized)
                {
                    foreach (RuleCondition condition in this)
                    {
                        condition.OnRuntimeInitialized();
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

        protected override void SetItem(int index, RuleCondition item)
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

