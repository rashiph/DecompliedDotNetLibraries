namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [Browsable(false)]
    public sealed class CompleteWizardStep : TemplatedWizardStep
    {
        internal override Wizard Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                if (!(value is CreateUserWizard) && (value != null))
                {
                    throw new HttpException(System.Web.SR.GetString("CompleteWizardStep_OnlyAllowedInCreateUserWizard"));
                }
                base.Owner = value;
            }
        }

        [Browsable(false), Themeable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Filterable(false)]
        public override WizardStepType StepType
        {
            get
            {
                return WizardStepType.Complete;
            }
            set
            {
                throw new InvalidOperationException(System.Web.SR.GetString("CreateUserWizardStep_StepTypeCannotBeSet"));
            }
        }

        [WebSysDefaultValue("CreateUserWizard_DefaultCompleteTitleText"), Localizable(true)]
        public override string Title
        {
            get
            {
                string titleInternal = base.TitleInternal;
                if (titleInternal == null)
                {
                    return System.Web.SR.GetString("CreateUserWizard_DefaultCompleteTitleText");
                }
                return titleInternal;
            }
            set
            {
                base.Title = value;
            }
        }
    }
}

