namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    internal sealed class RuleSetDefinitionEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object o)
        {
            if (typeDescriptorContext == null)
            {
                throw new ArgumentNullException("typeDescriptorContext");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            object obj2 = o;
            WorkflowDesignerLoader loader = serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if ((loader != null) && loader.InDebugMode)
            {
                throw new InvalidOperationException(Messages.DebugModeEditsDisallowed);
            }
            RuleSetReference instance = typeDescriptorContext.Instance as RuleSetReference;
            if (((instance == null) || (instance.RuleSetName == null)) || (instance.RuleSetName.Length <= 0))
            {
                throw new ArgumentException(Messages.RuleSetNameNotSet);
            }
            Activity component = null;
            IReferenceService service = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            if (service != null)
            {
                component = service.GetComponent(typeDescriptorContext.Instance) as Activity;
            }
            RuleSetCollection ruleSets = null;
            RuleDefinitions definitions = ConditionHelper.Load_Rules_DT(serviceProvider, System.Workflow.Activities.Common.Helpers.GetRootActivity(component));
            if (definitions != null)
            {
                ruleSets = definitions.RuleSets;
            }
            if ((ruleSets != null) && !ruleSets.Contains(instance.RuleSetName))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.RuleSetNotFound, new object[] { instance.RuleSetName }));
            }
            this.editorService = (IWindowsFormsEditorService) serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            if (this.editorService != null)
            {
                RuleSet ruleSet = ruleSets[instance.RuleSetName];
                using (RuleSetDialog dialog = new RuleSetDialog(component, ruleSet))
                {
                    if (DialogResult.OK == this.editorService.ShowDialog(dialog))
                    {
                        obj2 = dialog.RuleSet;
                    }
                }
            }
            return obj2;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

