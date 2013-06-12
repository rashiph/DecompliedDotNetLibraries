namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.WebParts.EditorZoneDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SupportsEventValidation]
    public class EditorZone : EditorZoneBase
    {
        private ITemplate _zoneTemplate;

        protected override EditorPartCollection CreateEditorParts()
        {
            EditorPartCollection parts = new EditorPartCollection();
            if (this._zoneTemplate != null)
            {
                Control container = new NonParentingControl();
                this._zoneTemplate.InstantiateIn(container);
                if (!container.HasControls())
                {
                    return parts;
                }
                foreach (Control control2 in container.Controls)
                {
                    EditorPart part = control2 as EditorPart;
                    if (part != null)
                    {
                        parts.Add(part);
                    }
                    else
                    {
                        LiteralControl control3 = control2 as LiteralControl;
                        if (((control3 == null) || (control3.Text.Trim().Length != 0)) && !base.DesignMode)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("EditorZone_OnlyEditorParts", new object[] { this.ID }));
                        }
                    }
                }
            }
            return parts;
        }

        [TemplateInstance(TemplateInstance.Single), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(EditorZone))]
        public virtual ITemplate ZoneTemplate
        {
            get
            {
                return this._zoneTemplate;
            }
            set
            {
                base.InvalidateEditorParts();
                this._zoneTemplate = value;
            }
        }
    }
}

