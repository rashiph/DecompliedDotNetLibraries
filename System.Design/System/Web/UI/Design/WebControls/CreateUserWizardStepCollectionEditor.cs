namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class CreateUserWizardStepCollectionEditor : WizardStepCollectionEditor
    {
        public CreateUserWizardStepCollectionEditor(Type type) : base(type)
        {
        }

        protected override bool CanRemoveInstance(object value)
        {
            return (!(value is CompleteWizardStep) && !(value is CreateUserWizardStep));
        }

        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            CollectionEditor.CollectionForm form = base.CreateCollectionForm();
            form.Text = System.Design.SR.GetString("CreateUserWizardStepCollectionEditor_Caption");
            return form;
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.CreateUserWizard.StepCollectionEditor";
            }
        }
    }
}

