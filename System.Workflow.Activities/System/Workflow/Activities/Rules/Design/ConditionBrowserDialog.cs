namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class ConditionBrowserDialog : BasicBrowserDialog
    {
        private RuleConditionCollection declarativeConditionCollection;

        public ConditionBrowserDialog(Activity activity, string name) : base(activity, name)
        {
            RuleDefinitions definitions = ConditionHelper.Load_Rules_DT(activity.Site, Helpers.GetRootActivity(activity));
            if (definitions != null)
            {
                this.declarativeConditionCollection = definitions.Conditions;
            }
            base.InitializeListView(this.declarativeConditionCollection, name);
        }

        private string CreateNewName()
        {
            string newConditionName = Messages.NewConditionName;
            int num = 1;
            while (true)
            {
                string key = newConditionName + num.ToString(CultureInfo.InvariantCulture);
                if (!this.declarativeConditionCollection.Contains(key))
                {
                    return key;
                }
                num++;
            }
        }

        protected override string GetObjectName(object ruleObject)
        {
            RuleExpressionCondition condition = ruleObject as RuleExpressionCondition;
            return condition.Name;
        }

        internal override bool IsUniqueName(string ruleName)
        {
            return !this.declarativeConditionCollection.Contains(ruleName);
        }

        protected override void OnDeleteInternal(object ruleObject)
        {
            RuleExpressionCondition condition = ruleObject as RuleExpressionCondition;
            this.declarativeConditionCollection.Remove(condition.Name);
        }

        protected override bool OnEditInternal(object currentRuleObject, out object updatedRuleObject)
        {
            RuleExpressionCondition condition = currentRuleObject as RuleExpressionCondition;
            updatedRuleObject = null;
            using (RuleConditionDialog dialog = new RuleConditionDialog(base.Activity, condition.Expression))
            {
                if (DialogResult.OK == dialog.ShowDialog(this))
                {
                    updatedRuleObject = new RuleExpressionCondition(condition.Name, dialog.Expression);
                    this.declarativeConditionCollection.Remove(condition.Name);
                    this.declarativeConditionCollection.Add(updatedRuleObject as RuleExpressionCondition);
                    return true;
                }
            }
            return false;
        }

        protected override object OnNewInternal()
        {
            using (RuleConditionDialog dialog = new RuleConditionDialog(base.Activity, null))
            {
                if (DialogResult.OK == dialog.ShowDialog(this))
                {
                    RuleExpressionCondition item = new RuleExpressionCondition {
                        Expression = dialog.Expression,
                        Name = this.CreateNewName()
                    };
                    this.declarativeConditionCollection.Add(item);
                    return item;
                }
            }
            return null;
        }

        protected override string OnRenameInternal(object ruleObject)
        {
            RuleExpressionCondition item = ruleObject as RuleExpressionCondition;
            using (RenameRuleObjectDialog dialog = new RenameRuleObjectDialog(base.Activity.Site, item.Name, new RenameRuleObjectDialog.NameValidatorDelegate(this.IsUniqueName), this))
            {
                if ((dialog.ShowDialog(this) == DialogResult.OK) && (dialog.RuleObjectName != item.Name))
                {
                    this.declarativeConditionCollection.Remove(item);
                    item.Name = dialog.RuleObjectName;
                    this.declarativeConditionCollection.Add(item);
                    return dialog.RuleObjectName;
                }
            }
            return null;
        }

        protected override void UpdateListViewItem(object ruleObject, ListViewItem listViewItem)
        {
            RuleExpressionCondition condition = ruleObject as RuleExpressionCondition;
            ITypeProvider service = (ITypeProvider) base.Activity.Site.GetService(typeof(ITypeProvider));
            if (service == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingService, new object[] { typeof(ITypeProvider).FullName }));
            }
            RuleValidation validation = new RuleValidation(base.Activity, service, false);
            bool flag = condition.Validate(validation);
            listViewItem.Tag = condition;
            listViewItem.Text = condition.Name;
            string text = flag ? Messages.Yes : Messages.No;
            if (listViewItem.SubItems.Count == 1)
            {
                listViewItem.SubItems.Add(text);
            }
            else
            {
                listViewItem.SubItems[1].Text = text;
            }
        }

        protected override void UpdatePreview(TextBox previewBox, object ruleObject)
        {
            RuleExpressionCondition condition = ruleObject as RuleExpressionCondition;
            if ((condition != null) && (condition.Expression != null))
            {
                RuleExpressionCondition condition2 = new RuleExpressionCondition(condition.Expression);
                System.Workflow.Activities.Common.NativeMethods.SendMessage(previewBox.Handle, 11, IntPtr.Zero, IntPtr.Zero);
                previewBox.Lines = condition2.ToString().Split(new char[] { '\n' });
                System.Workflow.Activities.Common.NativeMethods.SendMessage(previewBox.Handle, 11, new IntPtr(1), IntPtr.Zero);
                previewBox.Invalidate();
            }
            else
            {
                previewBox.Text = string.Empty;
            }
        }

        protected override string ConfirmDeleteMessageText
        {
            get
            {
                return Messages.ConditionConfirmDeleteMessageText;
            }
        }

        protected override string ConfirmDeleteTitleText
        {
            get
            {
                return Messages.DeleteCondition;
            }
        }

        protected override string DescriptionText
        {
            get
            {
                return Messages.ConditionDescriptionText;
            }
        }

        internal override string DuplicateNameErrorText
        {
            get
            {
                return Messages.ConditionDuplicateNameErrorText;
            }
        }

        internal override string EmptyNameErrorText
        {
            get
            {
                return Messages.ConditionEmptyNameErrorText;
            }
        }

        internal override string NewNameLabelText
        {
            get
            {
                return Messages.ConditionNewNameLableText;
            }
        }

        protected override string PreviewLabelText
        {
            get
            {
                return Messages.ConditionPreviewLabelText;
            }
        }

        internal override string RenameTitleText
        {
            get
            {
                return Messages.ConditionRenameTitleText;
            }
        }

        protected override string TitleText
        {
            get
            {
                return Messages.ConditionTitleText;
            }
        }
    }
}

