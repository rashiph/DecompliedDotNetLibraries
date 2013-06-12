namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [Bindable(false), ControlBuilder(typeof(WizardStepControlBuilder)), ParseChildren(true), Themeable(true), PersistChildren(false), ToolboxItem(false)]
    public class TemplatedWizardStep : WizardStepBase
    {
        private Control _contentContainer;
        private ITemplate _contentTemplate;
        private Control _navigationContainer;
        private ITemplate _navigationTemplate;

        [WebSysDescription("TemplatedWizardStep_ContentTemplate"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(Wizard)), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public virtual ITemplate ContentTemplate
        {
            get
            {
                return this._contentTemplate;
            }
            set
            {
                this._contentTemplate = value;
                if ((this.Owner != null) && (base.ControlState > ControlState.Constructed))
                {
                    this.Owner.RequiresControlsRecreation();
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control ContentTemplateContainer
        {
            get
            {
                return this._contentContainer;
            }
            internal set
            {
                this._contentContainer = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(Wizard)), Browsable(false), DefaultValue((string) null), WebSysDescription("TemplatedWizardStep_CustomNavigationTemplate")]
        public virtual ITemplate CustomNavigationTemplate
        {
            get
            {
                return this._navigationTemplate;
            }
            set
            {
                this._navigationTemplate = value;
                if ((this.Owner != null) && (base.ControlState > ControlState.Constructed))
                {
                    this.Owner.RequiresControlsRecreation();
                }
            }
        }

        [Browsable(false), Bindable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control CustomNavigationTemplateContainer
        {
            get
            {
                return this._navigationContainer;
            }
            internal set
            {
                this._navigationContainer = value;
            }
        }

        [Browsable(true)]
        public override string SkinID
        {
            get
            {
                return base.SkinID;
            }
            set
            {
                base.SkinID = value;
            }
        }
    }
}

