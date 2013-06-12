namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [Designer("System.Web.UI.Design.WebControls.WebParts.WebPartZoneDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SupportsEventValidation]
    public class WebPartZone : WebPartZoneBase
    {
        private bool _registrationComplete;
        private ITemplate _zoneTemplate;

        private void AddWebPartToList(WebPartCollection webParts, Control control)
        {
            WebPart part = control as WebPart;
            if ((part == null) && !(control is LiteralControl))
            {
                WebPartManager webPartManager = base.WebPartManager;
                if (webPartManager != null)
                {
                    part = webPartManager.CreateWebPart(control);
                }
                else
                {
                    part = WebPartManager.CreateWebPartStatic(control);
                }
            }
            if (part != null)
            {
                webParts.Add(part);
            }
        }

        protected internal override WebPartCollection GetInitialWebParts()
        {
            WebPartCollection webParts = new WebPartCollection();
            if (this.ZoneTemplate != null)
            {
                Control container = new NonParentingControl();
                this.ZoneTemplate.InstantiateIn(container);
                if (!container.HasControls())
                {
                    return webParts;
                }
                foreach (Control control2 in container.Controls)
                {
                    if (control2 is ContentPlaceHolder)
                    {
                        if (control2.HasControls())
                        {
                            Control[] array = new Control[control2.Controls.Count];
                            control2.Controls.CopyTo(array, 0);
                            foreach (Control control3 in array)
                            {
                                this.AddWebPartToList(webParts, control3);
                            }
                        }
                    }
                    else
                    {
                        this.AddWebPartToList(webParts, control2);
                    }
                }
            }
            return webParts;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this._registrationComplete = true;
        }

        [TemplateInstance(TemplateInstance.Single), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), Browsable(false)]
        public virtual ITemplate ZoneTemplate
        {
            get
            {
                return this._zoneTemplate;
            }
            set
            {
                if (!base.DesignMode && this._registrationComplete)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPart_SetZoneTemplateTooLate"));
                }
                this._zoneTemplate = value;
            }
        }
    }
}

