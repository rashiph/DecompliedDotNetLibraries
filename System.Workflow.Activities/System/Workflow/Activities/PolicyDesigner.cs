namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.Activities.Rules.Design;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(PolicyDesignerTheme))]
    internal sealed class PolicyDesigner : ActivityDesigner, IServiceProvider
    {
        protected override void DoDefaultAction()
        {
            base.DoDefaultAction();
            WorkflowDesignerLoader service = this.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if ((service != null) && service.InDebugMode)
            {
                throw new InvalidOperationException(Messages.DebugModeEditsDisallowed);
            }
            PolicyActivity activity = (PolicyActivity) base.Activity;
            if (!System.Workflow.Activities.Common.Helpers.IsActivityLocked(activity))
            {
                RuleDefinitions definitions = ConditionHelper.Load_Rules_DT(this, System.Workflow.Activities.Common.Helpers.GetRootActivity(activity));
                if (definitions != null)
                {
                    RuleSetCollection ruleSets = definitions.RuleSets;
                    RuleSetReference ruleSetReference = activity.RuleSetReference;
                    RuleSet ruleSet = null;
                    string key = null;
                    if ((ruleSetReference != null) && !string.IsNullOrEmpty(ruleSetReference.RuleSetName))
                    {
                        key = ruleSetReference.RuleSetName;
                        if (ruleSets.Contains(key))
                        {
                            ruleSet = ruleSets[key];
                        }
                    }
                    else
                    {
                        key = ruleSets.GenerateRuleSetName();
                    }
                    using (RuleSetDialog dialog = new RuleSetDialog(activity, ruleSet))
                    {
                        if (DialogResult.OK == dialog.ShowDialog())
                        {
                            if (ruleSet != null)
                            {
                                ruleSets.Remove(key);
                            }
                            else
                            {
                                dialog.RuleSet.Name = key;
                                activity.RuleSetReference = new RuleSetReference(key);
                            }
                            ruleSets.Add(dialog.RuleSet);
                            ConditionHelper.Flush_Rules_DT(this, System.Workflow.Activities.Common.Helpers.GetRootActivity(activity));
                        }
                    }
                }
                TypeDescriptor.GetProperties(activity)["RuleSetReference"].SetValue(activity, activity.RuleSetReference);
            }
        }

        public object GetService(System.Type type)
        {
            return base.GetService(type);
        }
    }
}

