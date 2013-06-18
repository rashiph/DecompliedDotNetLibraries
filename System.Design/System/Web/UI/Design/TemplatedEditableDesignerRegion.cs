namespace System.Web.UI.Design
{
    using System;
    using System.Design;

    public class TemplatedEditableDesignerRegion : EditableDesignerRegion
    {
        private bool _isSingleInstance;
        private System.Web.UI.Design.TemplateDefinition _templateDefinition;

        public TemplatedEditableDesignerRegion(System.Web.UI.Design.TemplateDefinition templateDefinition) : base(templateDefinition.Designer, templateDefinition.Name, templateDefinition.ServerControlsOnly)
        {
            this._templateDefinition = templateDefinition;
        }

        public virtual bool IsSingleInstanceTemplate
        {
            get
            {
                return this._isSingleInstance;
            }
            set
            {
                this._isSingleInstance = value;
            }
        }

        public override bool SupportsDataBinding
        {
            get
            {
                return this._templateDefinition.SupportsDataBinding;
            }
            set
            {
                throw new InvalidOperationException(System.Design.SR.GetString("TemplateEditableDesignerRegion_CannotSetSupportsDataBinding"));
            }
        }

        public System.Web.UI.Design.TemplateDefinition TemplateDefinition
        {
            get
            {
                return this._templateDefinition;
            }
        }
    }
}

