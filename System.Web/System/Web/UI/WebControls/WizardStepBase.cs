namespace System.Web.UI.WebControls
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ToolboxItem(false), Bindable(false), ControlBuilder(typeof(WizardStepControlBuilder))]
    public abstract class WizardStepBase : System.Web.UI.WebControls.View
    {
        private System.Web.UI.WebControls.Wizard _owner;

        protected WizardStepBase()
        {
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                base.LoadViewState(savedState);
                if ((this.Owner != null) && ((this.ViewState["Title"] != null) || (this.ViewState["StepType"] != null)))
                {
                    this.Owner.OnWizardStepsChanged();
                }
            }
        }

        protected internal override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if ((this.Owner == null) && !base.DesignMode)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WizardStep_WrongContainment"));
            }
        }

        protected internal override void RenderChildren(HtmlTextWriter writer)
        {
            if (this.Owner.ShouldRenderChildControl)
            {
                base.RenderChildren(writer);
            }
        }

        [Themeable(false), WebSysDescription("WizardStep_AllowReturn"), DefaultValue(true), Filterable(false), WebCategory("Behavior")]
        public virtual bool AllowReturn
        {
            get
            {
                object obj2 = this.ViewState["AllowReturn"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["AllowReturn"] = value;
            }
        }

        [Browsable(true)]
        public override bool EnableTheming
        {
            get
            {
                return base.EnableTheming;
            }
            set
            {
                base.EnableTheming = value;
            }
        }

        public override string ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                if ((this.Owner != null) && this.Owner.DesignMode)
                {
                    if (!CodeGenerator.IsValidLanguageIndependentIdentifier(value))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Invalid_identifier", new object[] { value }));
                    }
                    if ((value != null) && value.Equals(this.Owner.ID, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Id_already_used", new object[] { value }));
                    }
                    foreach (WizardStepBase base2 in this.Owner.WizardSteps)
                    {
                        if (((base2 != this) && (base2.ID != null)) && base2.ID.Equals(value, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new ArgumentException(System.Web.SR.GetString("Id_already_used", new object[] { value }));
                        }
                    }
                }
                base.ID = value;
            }
        }

        [WebSysDescription("WizardStep_Name"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), WebCategory("Appearance")]
        public virtual string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Title))
                {
                    return this.Title;
                }
                if (!string.IsNullOrEmpty(this.ID))
                {
                    return this.ID;
                }
                return null;
            }
        }

        internal virtual System.Web.UI.WebControls.Wizard Owner
        {
            get
            {
                return this._owner;
            }
            set
            {
                this._owner = value;
            }
        }

        [WebSysDescription("WizardStep_StepType"), DefaultValue(0), WebCategory("Behavior")]
        public virtual WizardStepType StepType
        {
            get
            {
                object obj2 = this.ViewState["StepType"];
                if (obj2 != null)
                {
                    return (WizardStepType) obj2;
                }
                return WizardStepType.Auto;
            }
            set
            {
                if ((value < WizardStepType.Auto) || (value > WizardStepType.Step))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.StepType != value)
                {
                    this.ViewState["StepType"] = value;
                    if (this.Owner != null)
                    {
                        this.Owner.OnWizardStepsChanged();
                    }
                }
            }
        }

        [WebCategory("Appearance"), Localizable(true), DefaultValue(""), WebSysDescription("WizardStep_Title")]
        public virtual string Title
        {
            get
            {
                string str = (string) this.ViewState["Title"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                if (this.Title != value)
                {
                    this.ViewState["Title"] = value;
                    if (this.Owner != null)
                    {
                        this.Owner.OnWizardStepsChanged();
                    }
                }
            }
        }

        internal string TitleInternal
        {
            get
            {
                return (string) this.ViewState["Title"];
            }
        }

        [Browsable(false), WebCategory("Appearance"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public System.Web.UI.WebControls.Wizard Wizard
        {
            get
            {
                return this.Owner;
            }
        }
    }
}

