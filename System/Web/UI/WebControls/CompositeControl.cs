namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.CompositeControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class CompositeControl : WebControl, INamingContainer, ICompositeControlDesignerAccessor
    {
        protected CompositeControl()
        {
        }

        public override void DataBind()
        {
            this.OnDataBinding(EventArgs.Empty);
            this.EnsureChildControls();
            this.DataBindChildren();
        }

        protected virtual void RecreateChildControls()
        {
            base.ChildControlsCreated = false;
            this.EnsureChildControls();
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (base.DesignMode)
            {
                this.EnsureChildControls();
            }
            base.Render(writer);
        }

        void ICompositeControlDesignerAccessor.RecreateChildControls()
        {
            this.RecreateChildControls();
        }

        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }
    }
}

