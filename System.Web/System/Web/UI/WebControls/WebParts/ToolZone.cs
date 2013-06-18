namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public abstract class ToolZone : WebZone, IPostBackEventHandler
    {
        private WebPartDisplayModeCollection _associatedDisplayModes;
        private Style _editUIStyle;
        private WebPartVerb _headerCloseVerb;
        private Style _headerVerbStyle;
        private Style _instructionTextStyle;
        private Style _labelStyle;
        private const int baseIndex = 0;
        private const int editUIStyleIndex = 1;
        private const string headerCloseEventArgument = "headerClose";
        private const int headerCloseVerbIndex = 3;
        private const int headerVerbStyleIndex = 4;
        private const int instructionTextStyleIndex = 5;
        private const int labelStyleIndex = 6;
        private const int viewStateArrayLength = 7;

        protected ToolZone(ICollection associatedDisplayModes)
        {
            if ((associatedDisplayModes == null) || (associatedDisplayModes.Count == 0))
            {
                throw new ArgumentNullException("associatedDisplayModes");
            }
            this._associatedDisplayModes = new WebPartDisplayModeCollection();
            foreach (WebPartDisplayMode mode in associatedDisplayModes)
            {
                this._associatedDisplayModes.Add(mode);
            }
            this._associatedDisplayModes.SetReadOnly("ToolZone_DisplayModesReadOnly");
        }

        protected ToolZone(WebPartDisplayMode associatedDisplayMode)
        {
            if (associatedDisplayMode == null)
            {
                throw new ArgumentNullException("associatedDisplayMode");
            }
            this._associatedDisplayModes = new WebPartDisplayModeCollection();
            this._associatedDisplayModes.Add(associatedDisplayMode);
            this._associatedDisplayModes.SetReadOnly("ToolZone_DisplayModesReadOnly");
        }

        protected abstract void Close();
        protected override void LoadViewState(object savedState)
        {
            if (savedState == null)
            {
                base.LoadViewState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 7)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.EditUIStyle).LoadViewState(objArray[1]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.HeaderCloseVerb).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.HeaderVerbStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.InstructionTextStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.LabelStyle).LoadViewState(objArray[6]);
                }
            }
        }

        protected virtual void OnDisplayModeChanged(object sender, WebPartDisplayModeEventArgs e)
        {
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            WebPartManager webPartManager = base.WebPartManager;
            if (webPartManager != null)
            {
                webPartManager.DisplayModeChanged += new WebPartDisplayModeEventHandler(this.OnDisplayModeChanged);
                webPartManager.SelectedWebPartChanged += new WebPartEventHandler(this.OnSelectedWebPartChanged);
            }
        }

        protected virtual void OnSelectedWebPartChanged(object sender, WebPartEventArgs e)
        {
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            if ((string.Equals(eventArgument, "headerClose", StringComparison.OrdinalIgnoreCase) && this.HeaderCloseVerb.Visible) && this.HeaderCloseVerb.Enabled)
            {
                this.Close();
            }
        }

        protected override void RenderFooter(HtmlTextWriter writer)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "4px");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            this.RenderVerbs(writer);
            writer.RenderEndTag();
        }

        protected override void RenderHeader(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            TitleStyle headerStyle = base.HeaderStyle;
            if (!headerStyle.IsEmpty)
            {
                Style style2 = new Style();
                if (!headerStyle.ForeColor.IsEmpty)
                {
                    style2.ForeColor = headerStyle.ForeColor;
                }
                style2.Font.CopyFrom(headerStyle.Font);
                if (!headerStyle.Font.Size.IsEmpty)
                {
                    style2.Font.Size = new FontUnit(new Unit(100.0, UnitType.Percentage));
                }
                if (!style2.IsEmpty)
                {
                    style2.AddAttributesToRender(writer, this);
                }
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            HorizontalAlign horizontalAlign = headerStyle.HorizontalAlign;
            if (horizontalAlign != HorizontalAlign.NotSet)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(HorizontalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Align, converter.ConvertToString(horizontalAlign));
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(this.HeaderText);
            writer.RenderEndTag();
            WebPartVerb headerCloseVerb = this.HeaderCloseVerb;
            if (headerCloseVerb.Visible)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                ZoneLinkButton button = new ZoneLinkButton(this, headerCloseVerb.EventArgument) {
                    Text = headerCloseVerb.Text,
                    ImageUrl = headerCloseVerb.ImageUrl,
                    ToolTip = headerCloseVerb.Description,
                    Enabled = headerCloseVerb.Enabled,
                    Page = this.Page
                };
                button.ApplyStyle(this.HeaderVerbStyle);
                button.RenderControl(writer);
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        protected virtual void RenderVerb(HtmlTextWriter writer, WebPartVerb verb)
        {
            WebControl control;
            string eventArgument = verb.EventArgument;
            if (this.VerbButtonType == ButtonType.Button)
            {
                ZoneButton button = new ZoneButton(this, eventArgument) {
                    Text = verb.Text
                };
                control = button;
            }
            else
            {
                ZoneLinkButton button2 = new ZoneLinkButton(this, eventArgument) {
                    Text = verb.Text
                };
                if (this.VerbButtonType == ButtonType.Image)
                {
                    button2.ImageUrl = verb.ImageUrl;
                }
                control = button2;
            }
            control.ApplyStyle(base.VerbStyle);
            control.ToolTip = verb.Description;
            control.Enabled = verb.Enabled;
            control.Page = this.Page;
            control.RenderControl(writer);
        }

        protected virtual void RenderVerbs(HtmlTextWriter writer)
        {
        }

        internal void RenderVerbsInternal(HtmlTextWriter writer, ICollection verbs)
        {
            ArrayList list = new ArrayList();
            foreach (WebPartVerb verb in verbs)
            {
                if (verb.Visible)
                {
                    list.Add(verb);
                }
            }
            if (list.Count > 0)
            {
                bool flag = true;
                foreach (WebPartVerb verb2 in list)
                {
                    if (!flag)
                    {
                        writer.Write("&nbsp;");
                    }
                    this.RenderVerb(writer, verb2);
                    flag = false;
                }
            }
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[7];
            objArray[0] = base.SaveViewState();
            objArray[1] = (this._editUIStyle != null) ? ((IStateManager) this._editUIStyle).SaveViewState() : null;
            objArray[3] = (this._headerCloseVerb != null) ? ((IStateManager) this._headerCloseVerb).SaveViewState() : null;
            objArray[4] = (this._headerVerbStyle != null) ? ((IStateManager) this._headerVerbStyle).SaveViewState() : null;
            objArray[5] = (this._instructionTextStyle != null) ? ((IStateManager) this._instructionTextStyle).SaveViewState() : null;
            objArray[6] = (this._labelStyle != null) ? ((IStateManager) this._labelStyle).SaveViewState() : null;
            for (int i = 0; i < 7; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._editUIStyle != null)
            {
                ((IStateManager) this._editUIStyle).TrackViewState();
            }
            if (this._headerCloseVerb != null)
            {
                ((IStateManager) this._headerCloseVerb).TrackViewState();
            }
            if (this._headerVerbStyle != null)
            {
                ((IStateManager) this._headerVerbStyle).TrackViewState();
            }
            if (this._instructionTextStyle != null)
            {
                ((IStateManager) this._instructionTextStyle).TrackViewState();
            }
            if (this._labelStyle != null)
            {
                ((IStateManager) this._labelStyle).TrackViewState();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public WebPartDisplayModeCollection AssociatedDisplayModes
        {
            get
            {
                return this._associatedDisplayModes;
            }
        }

        protected virtual bool Display
        {
            get
            {
                if (base.WebPartManager != null)
                {
                    WebPartDisplayModeCollection associatedDisplayModes = this.AssociatedDisplayModes;
                    if (associatedDisplayModes != null)
                    {
                        return associatedDisplayModes.Contains(base.WebPartManager.DisplayMode);
                    }
                }
                return false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), WebCategory("Styles"), WebSysDescription("ToolZone_EditUIStyle")]
        public Style EditUIStyle
        {
            get
            {
                if (this._editUIStyle == null)
                {
                    this._editUIStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._editUIStyle).TrackViewState();
                    }
                }
                return this._editUIStyle;
            }
        }

        [WebCategory("Verbs"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), WebSysDescription("ToolZone_HeaderCloseVerb")]
        public virtual WebPartVerb HeaderCloseVerb
        {
            get
            {
                if (this._headerCloseVerb == null)
                {
                    this._headerCloseVerb = new WebPartHeaderCloseVerb();
                    this._headerCloseVerb.EventArgument = "headerClose";
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._headerCloseVerb).TrackViewState();
                    }
                }
                return this._headerCloseVerb;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("ToolZone_HeaderVerbStyle"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public Style HeaderVerbStyle
        {
            get
            {
                if (this._headerVerbStyle == null)
                {
                    this._headerVerbStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._headerVerbStyle).TrackViewState();
                    }
                }
                return this._headerVerbStyle;
            }
        }

        [WebCategory("Behavior"), WebSysDefaultValue(""), Localizable(true), WebSysDescription("ToolZone_InstructionText")]
        public virtual string InstructionText
        {
            get
            {
                string str = (string) this.ViewState["InstructionText"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["InstructionText"] = value;
            }
        }

        [DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), NotifyParentProperty(true), WebSysDescription("ToolZone_InstructionTextStyle")]
        public Style InstructionTextStyle
        {
            get
            {
                if (this._instructionTextStyle == null)
                {
                    this._instructionTextStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._instructionTextStyle).TrackViewState();
                    }
                }
                return this._instructionTextStyle;
            }
        }

        [WebSysDescription("ToolZone_LabelStyle"), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DefaultValue((string) null)]
        public Style LabelStyle
        {
            get
            {
                if (this._labelStyle == null)
                {
                    this._labelStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._labelStyle).TrackViewState();
                    }
                }
                return this._labelStyle;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
        public override bool Visible
        {
            get
            {
                return (this.Display && base.Visible);
            }
            set
            {
                if (!base.DesignMode)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ToolZone_CantSetVisible"));
                }
            }
        }
    }
}

