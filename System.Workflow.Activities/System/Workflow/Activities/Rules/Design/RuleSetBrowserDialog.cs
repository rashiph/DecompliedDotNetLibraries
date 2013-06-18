namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class RuleSetBrowserDialog : BasicBrowserDialog
    {
        private RuleSetCollection ruleSetCollection;

        public RuleSetBrowserDialog(Activity activity, string name) : base(activity, name)
        {
            RuleDefinitions definitions = ConditionHelper.Load_Rules_DT(activity.Site, Helpers.GetRootActivity(activity));
            if (definitions != null)
            {
                this.ruleSetCollection = definitions.RuleSets;
            }
            base.InitializeListView(this.ruleSetCollection, name);
        }

        protected override string GetObjectName(object ruleObject)
        {
            RuleSet set = ruleObject as RuleSet;
            return set.Name;
        }

        internal override bool IsUniqueName(string ruleName)
        {
            return !this.ruleSetCollection.Contains(ruleName);
        }

        protected override void OnDeleteInternal(object ruleObject)
        {
            RuleSet set = ruleObject as RuleSet;
            this.ruleSetCollection.Remove(set.Name);
        }

        protected override bool OnEditInternal(object currentRuleObject, out object updatedRuleObject)
        {
            RuleSet ruleSet = currentRuleObject as RuleSet;
            updatedRuleObject = null;
            using (RuleSetDialog dialog = new RuleSetDialog(base.Activity, ruleSet))
            {
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    this.ruleSetCollection.Remove(ruleSet.Name);
                    this.ruleSetCollection.Add(dialog.RuleSet);
                    updatedRuleObject = dialog.RuleSet;
                    return true;
                }
            }
            return false;
        }

        protected override object OnNewInternal()
        {
            using (RuleSetDialog dialog = new RuleSetDialog(base.Activity, null))
            {
                if (DialogResult.OK == dialog.ShowDialog(this))
                {
                    RuleSet ruleSet = dialog.RuleSet;
                    ruleSet.Name = this.ruleSetCollection.GenerateRuleSetName();
                    this.ruleSetCollection.Add(ruleSet);
                    return ruleSet;
                }
            }
            return null;
        }

        protected override string OnRenameInternal(object ruleObject)
        {
            RuleSet item = ruleObject as RuleSet;
            using (RenameRuleObjectDialog dialog = new RenameRuleObjectDialog(base.Activity.Site, item.Name, new RenameRuleObjectDialog.NameValidatorDelegate(this.IsUniqueName), this))
            {
                if ((dialog.ShowDialog(this) == DialogResult.OK) && (dialog.RuleObjectName != item.Name))
                {
                    this.ruleSetCollection.Remove(item);
                    item.Name = dialog.RuleObjectName;
                    this.ruleSetCollection.Add(item);
                    return dialog.RuleObjectName;
                }
            }
            return null;
        }

        protected override void UpdateListViewItem(object ruleObject, ListViewItem listViewItem)
        {
            bool flag;
            RuleSet set = ruleObject as RuleSet;
            ValidationManager serviceProvider = new ValidationManager(base.Activity.Site);
            ITypeProvider service = (ITypeProvider) serviceProvider.GetService(typeof(ITypeProvider));
            RuleValidation validation = new RuleValidation(base.Activity, service, false);
            using (WorkflowCompilationContext.CreateScope(serviceProvider))
            {
                flag = set.Validate(validation);
            }
            listViewItem.Tag = set;
            listViewItem.Text = set.Name;
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
            RuleSet ruleSet = ruleObject as RuleSet;
            System.Workflow.Activities.Common.NativeMethods.SendMessage(previewBox.Handle, 11, IntPtr.Zero, IntPtr.Zero);
            previewBox.Lines = DesignerHelpers.GetRuleSetPreview(ruleSet).Split(new char[] { '\n' });
            System.Workflow.Activities.Common.NativeMethods.SendMessage(previewBox.Handle, 11, new IntPtr(1), IntPtr.Zero);
            previewBox.Invalidate();
        }

        protected override string ConfirmDeleteMessageText
        {
            get
            {
                return Messages.RuleSetConfirmDeleteMessageText;
            }
        }

        protected override string ConfirmDeleteTitleText
        {
            get
            {
                return Messages.DeleteRuleSet;
            }
        }

        protected override string DescriptionText
        {
            get
            {
                return Messages.RuleSetDescriptionText;
            }
        }

        internal override string DuplicateNameErrorText
        {
            get
            {
                return Messages.RuleSetDuplicateNameErrorText;
            }
        }

        internal override string EmptyNameErrorText
        {
            get
            {
                return Messages.RuleSetEmptyNameErrorText;
            }
        }

        internal override string NewNameLabelText
        {
            get
            {
                return Messages.RuleSetNewNameLableText;
            }
        }

        protected override string PreviewLabelText
        {
            get
            {
                return Messages.RuleSetPreviewLabelText;
            }
        }

        internal override string RenameTitleText
        {
            get
            {
                return Messages.RuleSetRenameTitleText;
            }
        }

        protected override string TitleText
        {
            get
            {
                return Messages.RuleSetTitleText;
            }
        }
    }
}

