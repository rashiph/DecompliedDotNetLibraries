namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public abstract class EditorZoneBase : ToolZone
    {
        private bool _applyError;
        private WebPartVerb _applyVerb;
        private WebPartVerb _cancelVerb;
        private System.Web.UI.WebControls.WebParts.EditorPartChrome _editorPartChrome;
        private EditorPartCollection _editorParts;
        private WebPartVerb _okVerb;
        private const string applyEventArgument = "apply";
        private const int applyVerbIndex = 1;
        private const int baseIndex = 0;
        private const string cancelEventArgument = "cancel";
        private const int cancelVerbIndex = 2;
        private const string okEventArgument = "ok";
        private const int okVerbIndex = 3;
        private const int viewStateArrayLength = 4;

        protected EditorZoneBase() : base(WebPartManager.EditDisplayMode)
        {
        }

        private void ApplyAndSyncChanges()
        {
            if (this.WebPartToEdit != null)
            {
                EditorPartCollection editorParts = this.EditorParts;
                foreach (EditorPart part2 in editorParts)
                {
                    if ((part2.Display && part2.Visible) && ((part2.ChromeState == PartChromeState.Normal) && !part2.ApplyChanges()))
                    {
                        this._applyError = true;
                    }
                }
                if (!this._applyError)
                {
                    foreach (EditorPart part3 in editorParts)
                    {
                        part3.SyncChanges();
                    }
                }
            }
        }

        protected override void Close()
        {
            if (base.WebPartManager != null)
            {
                base.WebPartManager.EndWebPartEditing();
            }
        }

        protected internal override void CreateChildControls()
        {
            ControlCollection controls = this.Controls;
            controls.Clear();
            WebPart webPartToEdit = this.WebPartToEdit;
            foreach (EditorPart part2 in this.EditorParts)
            {
                if (webPartToEdit != null)
                {
                    part2.SetWebPartToEdit(webPartToEdit);
                    part2.SetWebPartManager(base.WebPartManager);
                }
                part2.SetZone(this);
                controls.Add(part2);
            }
        }

        protected virtual System.Web.UI.WebControls.WebParts.EditorPartChrome CreateEditorPartChrome()
        {
            return new System.Web.UI.WebControls.WebParts.EditorPartChrome(this);
        }

        protected abstract EditorPartCollection CreateEditorParts();
        protected void InvalidateEditorParts()
        {
            this._editorParts = null;
            base.ChildControlsCreated = false;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState == null)
            {
                base.LoadViewState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 4)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.ApplyVerb).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.CancelVerb).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.OKVerb).LoadViewState(objArray[3]);
                }
            }
        }

        protected override void OnDisplayModeChanged(object sender, WebPartDisplayModeEventArgs e)
        {
            this.InvalidateEditorParts();
            base.OnDisplayModeChanged(sender, e);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.EditorPartChrome.PerformPreRender();
        }

        protected override void OnSelectedWebPartChanged(object sender, WebPartEventArgs e)
        {
            if ((base.WebPartManager != null) && (base.WebPartManager.DisplayMode == WebPartManager.EditDisplayMode))
            {
                this.InvalidateEditorParts();
                if (e.WebPart != null)
                {
                    foreach (EditorPart part in this.EditorParts)
                    {
                        part.SyncChanges();
                    }
                }
            }
            base.OnSelectedWebPartChanged(sender, e);
        }

        protected override void RaisePostBackEvent(string eventArgument)
        {
            if (string.Equals(eventArgument, "apply", StringComparison.OrdinalIgnoreCase))
            {
                if ((this.ApplyVerb.Visible && this.ApplyVerb.Enabled) && (this.WebPartToEdit != null))
                {
                    this.ApplyAndSyncChanges();
                }
            }
            else if (string.Equals(eventArgument, "cancel", StringComparison.OrdinalIgnoreCase))
            {
                if ((this.CancelVerb.Visible && this.CancelVerb.Enabled) && (this.WebPartToEdit != null))
                {
                    this.Close();
                }
            }
            else if (string.Equals(eventArgument, "ok", StringComparison.OrdinalIgnoreCase))
            {
                if ((this.OKVerb.Visible && this.OKVerb.Enabled) && (this.WebPartToEdit != null))
                {
                    this.ApplyAndSyncChanges();
                    if (!this._applyError)
                    {
                        this.Close();
                    }
                }
            }
            else
            {
                base.RaisePostBackEvent(eventArgument);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            base.Render(writer);
        }

        protected override void RenderBody(HtmlTextWriter writer)
        {
            base.RenderBodyTableBeginTag(writer);
            if (base.DesignMode)
            {
                base.RenderDesignerRegionBeginTag(writer, Orientation.Vertical);
            }
            if (this.HasControls())
            {
                bool firstCell = true;
                this.RenderInstructionText(writer, ref firstCell);
                if (this._applyError)
                {
                    this.RenderErrorText(writer, ref firstCell);
                }
                System.Web.UI.WebControls.WebParts.EditorPartChrome editorPartChrome = this.EditorPartChrome;
                foreach (EditorPart part in this.EditorParts)
                {
                    if (part.Display && part.Visible)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        if (!firstCell)
                        {
                            writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingTop, "0");
                        }
                        else
                        {
                            firstCell = false;
                        }
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        editorPartChrome.RenderEditorPart(writer, part);
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                    }
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            else
            {
                this.RenderEmptyZoneText(writer);
            }
            if (base.DesignMode)
            {
                WebZone.RenderDesignerRegionEndTag(writer);
            }
            WebZone.RenderBodyTableEndTag(writer);
        }

        private void RenderEmptyZoneText(HtmlTextWriter writer)
        {
            string emptyZoneText = this.EmptyZoneText;
            if (!string.IsNullOrEmpty(emptyZoneText))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                Style emptyZoneTextStyle = base.EmptyZoneTextStyle;
                if (!emptyZoneTextStyle.IsEmpty)
                {
                    emptyZoneTextStyle.AddAttributesToRender(writer, this);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write(emptyZoneText);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        private void RenderErrorText(HtmlTextWriter writer, ref bool firstCell)
        {
            string errorText = this.ErrorText;
            if (!string.IsNullOrEmpty(errorText))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                firstCell = false;
                Label label = new Label {
                    Text = errorText,
                    Page = this.Page
                };
                label.ApplyStyle(base.ErrorStyle);
                label.RenderControl(writer);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        private void RenderInstructionText(HtmlTextWriter writer, ref bool firstCell)
        {
            string instructionText = this.InstructionText;
            if (!string.IsNullOrEmpty(instructionText))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                firstCell = false;
                Label label = new Label {
                    Text = instructionText,
                    Page = this.Page
                };
                label.ApplyStyle(base.InstructionTextStyle);
                label.RenderControl(writer);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        protected override void RenderVerbs(HtmlTextWriter writer)
        {
            base.RenderVerbsInternal(writer, new WebPartVerb[] { this.OKVerb, this.CancelVerb, this.ApplyVerb });
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { base.SaveViewState(), (this._applyVerb != null) ? ((IStateManager) this._applyVerb).SaveViewState() : null, (this._cancelVerb != null) ? ((IStateManager) this._cancelVerb).SaveViewState() : null, (this._okVerb != null) ? ((IStateManager) this._okVerb).SaveViewState() : null };
            for (int i = 0; i < 4; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._applyVerb != null)
            {
                ((IStateManager) this._applyVerb).TrackViewState();
            }
            if (this._cancelVerb != null)
            {
                ((IStateManager) this._cancelVerb).TrackViewState();
            }
            if (this._okVerb != null)
            {
                ((IStateManager) this._okVerb).TrackViewState();
            }
        }

        [WebCategory("Verbs"), WebSysDescription("EditorZoneBase_ApplyVerb"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public virtual WebPartVerb ApplyVerb
        {
            get
            {
                if (this._applyVerb == null)
                {
                    this._applyVerb = new WebPartEditorApplyVerb();
                    this._applyVerb.EventArgument = "apply";
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._applyVerb).TrackViewState();
                    }
                }
                return this._applyVerb;
            }
        }

        [DefaultValue((string) null), WebSysDescription("EditorZoneBase_CancelVerb"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Verbs")]
        public virtual WebPartVerb CancelVerb
        {
            get
            {
                if (this._cancelVerb == null)
                {
                    this._cancelVerb = new WebPartEditorCancelVerb();
                    this._cancelVerb.EventArgument = "cancel";
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._cancelVerb).TrackViewState();
                    }
                }
                return this._cancelVerb;
            }
        }

        protected override bool Display
        {
            get
            {
                return (base.Display && (this.WebPartToEdit != null));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Web.UI.WebControls.WebParts.EditorPartChrome EditorPartChrome
        {
            get
            {
                if (this._editorPartChrome == null)
                {
                    this._editorPartChrome = this.CreateEditorPartChrome();
                }
                return this._editorPartChrome;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EditorPartCollection EditorParts
        {
            get
            {
                if (this._editorParts == null)
                {
                    WebPart webPartToEdit = this.WebPartToEdit;
                    EditorPartCollection existingEditorParts = null;
                    if ((webPartToEdit != null) && (webPartToEdit != null))
                    {
                        existingEditorParts = webPartToEdit.CreateEditorParts();
                    }
                    EditorPartCollection parts2 = new EditorPartCollection(existingEditorParts, this.CreateEditorParts());
                    if (!base.DesignMode)
                    {
                        foreach (EditorPart part2 in parts2)
                        {
                            if (string.IsNullOrEmpty(part2.ID))
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("EditorZoneBase_NoEditorPartID"));
                            }
                        }
                    }
                    this._editorParts = parts2;
                    this.EnsureChildControls();
                }
                return this._editorParts;
            }
        }

        [WebSysDefaultValue("EditorZoneBase_DefaultEmptyZoneText")]
        public override string EmptyZoneText
        {
            get
            {
                string str = (string) this.ViewState["EmptyZoneText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("EditorZoneBase_DefaultEmptyZoneText");
            }
            set
            {
                this.ViewState["EmptyZoneText"] = value;
            }
        }

        [WebSysDefaultValue("EditorZoneBase_DefaultErrorText"), WebCategory("Behavior"), WebSysDescription("EditorZoneBase_ErrorText"), Localizable(true)]
        public virtual string ErrorText
        {
            get
            {
                string str = (string) this.ViewState["ErrorText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("EditorZoneBase_DefaultErrorText");
            }
            set
            {
                this.ViewState["ErrorText"] = value;
            }
        }

        [WebSysDefaultValue("EditorZoneBase_DefaultHeaderText")]
        public override string HeaderText
        {
            get
            {
                string str = (string) this.ViewState["HeaderText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("EditorZoneBase_DefaultHeaderText");
            }
            set
            {
                this.ViewState["HeaderText"] = value;
            }
        }

        [WebSysDefaultValue("EditorZoneBase_DefaultInstructionText")]
        public override string InstructionText
        {
            get
            {
                string str = (string) this.ViewState["InstructionText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("EditorZoneBase_DefaultInstructionText");
            }
            set
            {
                this.ViewState["InstructionText"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("EditorZoneBase_OKVerb"), PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), WebCategory("Verbs"), DefaultValue((string) null)]
        public virtual WebPartVerb OKVerb
        {
            get
            {
                if (this._okVerb == null)
                {
                    this._okVerb = new WebPartEditorOKVerb();
                    this._okVerb.EventArgument = "ok";
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._okVerb).TrackViewState();
                    }
                }
                return this._okVerb;
            }
        }

        protected WebPart WebPartToEdit
        {
            get
            {
                if ((base.WebPartManager != null) && (base.WebPartManager.DisplayMode == WebPartManager.EditDisplayMode))
                {
                    return base.WebPartManager.SelectedWebPart;
                }
                return null;
            }
        }
    }
}

