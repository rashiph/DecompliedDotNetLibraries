namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.Activities.Rules;

    internal static class DesignerHelpers
    {
        internal static void DisplayError(string message, string messageBoxTitle, IServiceProvider serviceProvider)
        {
            IUIService service = null;
            if (serviceProvider != null)
            {
                service = (IUIService) serviceProvider.GetService(typeof(IUIService));
            }
            if (service != null)
            {
                service.ShowError(message);
            }
            else
            {
                MessageBox.Show(message, messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0);
            }
        }

        internal static string GetRulePreview(Rule rule)
        {
            StringBuilder builder = new StringBuilder();
            if (rule != null)
            {
                builder.Append("IF ");
                if (rule.Condition != null)
                {
                    builder.Append(rule.Condition.ToString() + " ");
                }
                builder.Append("THEN ");
                foreach (RuleAction action in rule.ThenActions)
                {
                    builder.Append(action.ToString());
                    builder.Append(' ');
                }
                if (rule.ElseActions.Count > 0)
                {
                    builder.Append("ELSE ");
                    foreach (RuleAction action2 in rule.ElseActions)
                    {
                        builder.Append(action2.ToString());
                        builder.Append(' ');
                    }
                }
            }
            return builder.ToString();
        }

        internal static string GetRuleSetPreview(RuleSet ruleSet)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            if (ruleSet != null)
            {
                foreach (Rule rule in ruleSet.Rules)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append("\n");
                    }
                    builder.Append(rule.Name);
                    builder.Append(": ");
                    builder.Append(GetRulePreview(rule));
                }
            }
            return builder.ToString();
        }
    }
}

