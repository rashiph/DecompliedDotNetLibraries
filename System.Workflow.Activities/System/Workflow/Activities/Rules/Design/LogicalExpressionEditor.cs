namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.CodeDom;
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

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    internal sealed class LogicalExpressionEditor : UITypeEditor
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
            RuleConditionReference instance = typeDescriptorContext.Instance as RuleConditionReference;
            if (((instance == null) || (instance.ConditionName == null)) || (instance.ConditionName.Length <= 0))
            {
                throw new ArgumentException(Messages.ConditionNameNotSet);
            }
            Activity component = null;
            IReferenceService service = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            if (service != null)
            {
                component = service.GetComponent(typeDescriptorContext.Instance) as Activity;
            }
            RuleConditionCollection conditions = null;
            RuleDefinitions definitions = ConditionHelper.Load_Rules_DT(serviceProvider, Helpers.GetRootActivity(component));
            if (definitions != null)
            {
                conditions = definitions.Conditions;
            }
            if ((conditions != null) && !conditions.Contains(instance.ConditionName))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.ConditionNotFound, new object[] { instance.ConditionName }));
            }
            this.editorService = (IWindowsFormsEditorService) serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            if (this.editorService != null)
            {
                CodeExpression expression = typeDescriptorContext.PropertyDescriptor.GetValue(typeDescriptorContext.Instance) as CodeExpression;
                try
                {
                    using (RuleConditionDialog dialog = new RuleConditionDialog(component, expression))
                    {
                        if (DialogResult.OK == this.editorService.ShowDialog(dialog))
                        {
                            obj2 = dialog.Expression;
                        }
                    }
                }
                catch (NotSupportedException)
                {
                    DesignerHelpers.DisplayError(Messages.Error_ExpressionNotSupported, Messages.ConditionEditor, serviceProvider);
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

