namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [Designer("System.Web.UI.Design.WebControls.WebParts.EditorPartDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false)]
    public abstract class EditorPart : Part
    {
        private System.Web.UI.WebControls.WebParts.WebPartManager _webPartManager;
        private WebPart _webPartToEdit;
        private EditorZoneBase _zone;

        protected EditorPart()
        {
        }

        public abstract bool ApplyChanges();
        internal string CreateErrorMessage(string exceptionMessage)
        {
            if ((this.Context != null) && this.Context.IsCustomErrorEnabled)
            {
                return System.Web.SR.GetString("EditorPart_ErrorSettingProperty");
            }
            return System.Web.SR.GetString("EditorPart_ErrorSettingPropertyWithExceptionMessage", new object[] { exceptionMessage });
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override IDictionary GetDesignModeState()
        {
            IDictionary dictionary = new HybridDictionary(1);
            dictionary["Zone"] = this.Zone;
            return dictionary;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Zone == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("EditorPart_MustBeInZone", new object[] { this.ID }));
            }
            if (!this.Display)
            {
                this.Visible = false;
            }
        }

        private void RenderDisplayName(HtmlTextWriter writer, string displayName, string associatedClientID)
        {
            if (this.Zone != null)
            {
                this.Zone.LabelStyle.AddAttributesToRender(writer, this);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.For, associatedClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.WriteEncodedText(displayName);
            writer.RenderEndTag();
        }

        internal void RenderPropertyEditors(HtmlTextWriter writer, string[] propertyDisplayNames, string[] propertyDescriptions, WebControl[] propertyEditors, string[] errorMessages)
        {
            if (propertyDisplayNames.Length != 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "4");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                for (int i = 0; i < propertyDisplayNames.Length; i++)
                {
                    WebControl control = propertyEditors[i];
                    if ((this.Zone != null) && !this.Zone.EditUIStyle.IsEmpty)
                    {
                        control.ApplyStyle(this.Zone.EditUIStyle);
                    }
                    string str = (propertyDescriptions != null) ? propertyDescriptions[i] : null;
                    if (!string.IsNullOrEmpty(str))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Title, str);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (control is CheckBox)
                    {
                        control.RenderControl(writer);
                        writer.Write("&nbsp;");
                        this.RenderDisplayName(writer, propertyDisplayNames[i], control.ClientID);
                    }
                    else
                    {
                        string clientID;
                        CompositeControl control2 = control as CompositeControl;
                        if (control2 != null)
                        {
                            clientID = control2.Controls[0].ClientID;
                        }
                        else
                        {
                            clientID = control.ClientID;
                        }
                        this.RenderDisplayName(writer, propertyDisplayNames[i] + ":", clientID);
                        writer.WriteBreak();
                        writer.WriteLine();
                        control.RenderControl(writer);
                    }
                    writer.WriteBreak();
                    writer.WriteLine();
                    string str3 = errorMessages[i];
                    if (!string.IsNullOrEmpty(str3))
                    {
                        if ((this.Zone != null) && !this.Zone.ErrorStyle.IsEmpty)
                        {
                            this.Zone.ErrorStyle.AddAttributesToRender(writer, this);
                        }
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                        writer.WriteEncodedText(str3);
                        writer.RenderEndTag();
                        writer.WriteBreak();
                        writer.WriteLine();
                    }
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                writer.RenderEndTag();
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override void SetDesignModeState(IDictionary data)
        {
            if (data != null)
            {
                object obj2 = data["Zone"];
                if (obj2 != null)
                {
                    this.SetZone((EditorZoneBase) obj2);
                }
            }
        }

        internal void SetWebPartManager(System.Web.UI.WebControls.WebParts.WebPartManager webPartManager)
        {
            this._webPartManager = webPartManager;
        }

        internal void SetWebPartToEdit(WebPart webPartToEdit)
        {
            this._webPartToEdit = webPartToEdit;
        }

        internal void SetZone(EditorZoneBase zone)
        {
            this._zone = zone;
        }

        public abstract void SyncChanges();

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Display
        {
            get
            {
                if (!base.DesignMode)
                {
                    if (this.WebPartToEdit == null)
                    {
                        return false;
                    }
                    if (this.WebPartToEdit is ProxyWebPart)
                    {
                        return false;
                    }
                    if ((!this.WebPartToEdit.AllowEdit && this.WebPartToEdit.IsShared) && ((this.WebPartManager != null) && (this.WebPartManager.Personalization.Scope == PersonalizationScope.User)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DisplayTitle
        {
            get
            {
                string title = this.Title;
                if (string.IsNullOrEmpty(title))
                {
                    title = System.Web.SR.GetString("Part_Untitled");
                }
                return title;
            }
        }

        protected System.Web.UI.WebControls.WebParts.WebPartManager WebPartManager
        {
            get
            {
                return this._webPartManager;
            }
        }

        protected WebPart WebPartToEdit
        {
            get
            {
                return this._webPartToEdit;
            }
        }

        protected EditorZoneBase Zone
        {
            get
            {
                return this._zone;
            }
        }
    }
}

