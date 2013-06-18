namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Workflow.ComponentModel;

    public sealed class RuleDefinitions : IWorkflowChangeDiff
    {
        private RuleConditionCollection conditions;
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty RuleDefinitionsProperty = DependencyProperty.RegisterAttached("RuleDefinitions", typeof(RuleDefinitions), typeof(RuleDefinitions), new PropertyMetadata(null, DependencyPropertyOptions.Metadata, new GetValueOverride(RuleDefinitions.OnGetRuleConditions), null, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        private RuleSetCollection ruleSets;
        private bool runtimeInitialized;
        [NonSerialized]
        private object syncLock = new object();

        internal RuleDefinitions Clone()
        {
            RuleDefinitions definitions = new RuleDefinitions();
            if (this.ruleSets != null)
            {
                definitions.ruleSets = new RuleSetCollection();
                foreach (RuleSet set in this.ruleSets)
                {
                    definitions.ruleSets.Add(set.Clone());
                }
            }
            if (this.conditions != null)
            {
                definitions.conditions = new RuleConditionCollection();
                foreach (RuleCondition condition in this.conditions)
                {
                    definitions.conditions.Add(condition.Clone());
                }
            }
            return definitions;
        }

        public IList<WorkflowChangeAction> Diff(object originalDefinition, object changedDefinition)
        {
            RuleDefinitions definitions = originalDefinition as RuleDefinitions;
            RuleDefinitions definitions2 = changedDefinition as RuleDefinitions;
            if ((definitions == null) || (definitions2 == null))
            {
                return new List<WorkflowChangeAction>();
            }
            IList<WorkflowChangeAction> list = this.Conditions.Diff(definitions.Conditions, definitions2.Conditions);
            IList<WorkflowChangeAction> list2 = this.RuleSets.Diff(definitions.RuleSets, definitions2.RuleSets);
            if (list.Count == 0)
            {
                return list2;
            }
            for (int i = 0; i < list2.Count; i++)
            {
                list.Add(list2[i]);
            }
            return list;
        }

        internal static object OnGetRuleConditions(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }
            RuleDefinitions valueBase = dependencyObject.GetValueBase(RuleDefinitionsProperty) as RuleDefinitions;
            if (valueBase == null)
            {
                Activity activity = dependencyObject as Activity;
                if (activity.Parent == null)
                {
                    valueBase = ConditionHelper.GetRuleDefinitionsFromManifest(activity.GetType());
                    if (valueBase != null)
                    {
                        dependencyObject.SetValue(RuleDefinitionsProperty, valueBase);
                    }
                }
            }
            return valueBase;
        }

        internal void OnRuntimeInitialized()
        {
            lock (this.syncLock)
            {
                if (!this.runtimeInitialized)
                {
                    this.Conditions.OnRuntimeInitialized();
                    this.RuleSets.OnRuntimeInitialized();
                    this.runtimeInitialized = true;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleConditionCollection Conditions
        {
            get
            {
                if (this.conditions == null)
                {
                    this.conditions = new RuleConditionCollection();
                }
                return this.conditions;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleSetCollection RuleSets
        {
            get
            {
                if (this.ruleSets == null)
                {
                    this.ruleSets = new RuleSetCollection();
                }
                return this.ruleSets;
            }
        }
    }
}

