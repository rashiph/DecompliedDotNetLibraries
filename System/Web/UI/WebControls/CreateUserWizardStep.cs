namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [Browsable(false)]
    public sealed class CreateUserWizardStep : TemplatedWizardStep
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override bool AllowReturn
        {
            get
            {
                return this.AllowReturnInternal;
            }
            set
            {
                throw new InvalidOperationException(System.Web.SR.GetString("CreateUserWizardStep_AllowReturnCannotBeSet"));
            }
        }

        internal bool AllowReturnInternal
        {
            get
            {
                object obj2 = this.ViewState["AllowReturnInternal"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["AllowReturnInternal"] = value;
            }
        }

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
                    throw new HttpException(System.Web.SR.GetString("CreateUserWizardStep_OnlyAllowedInCreateUserWizard"));
                }
                base.Owner = value;
            }
        }

        [Filterable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), Themeable(false)]
        public override WizardStepType StepType
        {
            get
            {
                return base.StepType;
            }
            set
            {
                throw new InvalidOperationException(System.Web.SR.GetString("CreateUserWizardStep_StepTypeCannotBeSet"));
            }
        }

        [WebSysDefaultValue("CreateUserWizard_DefaultCreateUserTitleText"), Localizable(true)]
        public override string Title
        {
            get
            {
                string titleInternal = base.TitleInternal;
                if (titleInternal == null)
                {
                    return System.Web.SR.GetString("CreateUserWizard_DefaultCreateUserTitleText");
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

