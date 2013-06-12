namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [Designer("System.Web.UI.Design.WebControls.WebParts.WebPartDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class WebPart : Part, IWebPart, IWebActionable, IWebEditable
    {
        private bool _allowClose = true;
        private bool _allowConnect = true;
        private bool _allowEdit = true;
        private bool _allowHide = true;
        private bool _allowMinimize = true;
        private bool _allowZoneChange = true;
        private string _authorizationFilter;
        private string _catalogIconImageUrl;
        private PartChromeState _chromeState = PartChromeState.Normal;
        private string _connectErrorMessage;
        private WebPartExportMode _exportMode = WebPartExportMode.None;
        private bool _hasSharedData;
        private bool _hasUserData;
        private WebPartHelpMode _helpMode = WebPartHelpMode.Navigate;
        private string _helpUrl;
        private bool _hidden;
        private string _importErrorMessage;
        private bool _isClosed;
        private bool _isShared;
        private bool _isStandalone = true;
        private bool _isStatic = true;
        private string _titleIconImageUrl;
        private string _titleUrl;
        private Dictionary<ProviderConnectionPoint, int> _trackerCounter;
        private System.Web.UI.WebControls.WebParts.WebPartManager _webPartManager;
        private WebPartZoneBase _zone;
        private string _zoneID;
        private int _zoneIndex;
        private const string titleBarIDPrefix = "WebPartTitle_";
        internal const string WholePartIDPrefix = "WebPart_";

        protected WebPart()
        {
        }

        public virtual EditorPartCollection CreateEditorParts()
        {
            return EditorPartCollection.Empty;
        }

        protected internal virtual void OnClosing(EventArgs e)
        {
        }

        protected internal virtual void OnConnectModeChanged(EventArgs e)
        {
        }

        protected internal virtual void OnDeleting(EventArgs e)
        {
        }

        protected internal virtual void OnEditModeChanged(EventArgs e)
        {
        }

        internal override void PreRenderRecursiveInternal()
        {
            if (this.IsStandalone)
            {
                if (this.Hidden)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPart_NotStandalone", new object[] { "Hidden", this.ID }));
                }
            }
            else if (!this.Visible)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPart_OnlyStandalone", new object[] { "Visible", this.ID }));
            }
            base.PreRenderRecursiveInternal();
        }

        internal void SetConnectErrorMessage(string connectErrorMessage)
        {
            if (string.IsNullOrEmpty(this._connectErrorMessage))
            {
                this._connectErrorMessage = connectErrorMessage;
            }
        }

        internal void SetHasSharedData(bool hasSharedData)
        {
            this._hasSharedData = hasSharedData;
        }

        internal void SetHasUserData(bool hasUserData)
        {
            this._hasUserData = hasUserData;
        }

        internal void SetIsClosed(bool isClosed)
        {
            this._isClosed = isClosed;
        }

        internal void SetIsShared(bool isShared)
        {
            this._isShared = isShared;
        }

        internal void SetIsStandalone(bool isStandalone)
        {
            this._isStandalone = isStandalone;
        }

        internal void SetIsStatic(bool isStatic)
        {
            this._isStatic = isStatic;
        }

        protected void SetPersonalizationDirty()
        {
            if (this.WebPartManager == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartManagerRequired"));
            }
            this.WebPartManager.Personalization.SetDirty(this);
        }

        public static void SetPersonalizationDirty(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "Page" }), "control");
            }
            System.Web.UI.WebControls.WebParts.WebPartManager currentWebPartManager = System.Web.UI.WebControls.WebParts.WebPartManager.GetCurrentWebPartManager(control.Page);
            if (currentWebPartManager == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartManagerRequired"));
            }
            WebPart genericWebPart = currentWebPartManager.GetGenericWebPart(control);
            if (genericWebPart == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPart_NonWebPart"), "control");
            }
            genericWebPart.SetPersonalizationDirty();
        }

        internal void SetWebPartManager(System.Web.UI.WebControls.WebParts.WebPartManager webPartManager)
        {
            this._webPartManager = webPartManager;
        }

        internal void SetZoneIndex(int zoneIndex)
        {
            if (zoneIndex < 0)
            {
                throw new ArgumentOutOfRangeException("zoneIndex");
            }
            this._zoneIndex = zoneIndex;
        }

        internal Control ToControl()
        {
            GenericWebPart part = this as GenericWebPart;
            if (part == null)
            {
                return this;
            }
            Control childControl = part.ChildControl;
            if (childControl == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("GenericWebPart_ChildControlIsNull"));
            }
            return childControl;
        }

        protected override void TrackViewState()
        {
            if (this.WebPartManager != null)
            {
                this.WebPartManager.Personalization.ApplyPersonalizationState(this);
            }
            base.TrackViewState();
        }

        [WebSysDescription("WebPart_AllowClose"), DefaultValue(true), Personalizable(PersonalizationScope.Shared), Themeable(false), WebCategory("WebPartBehavior")]
        public virtual bool AllowClose
        {
            get
            {
                return this._allowClose;
            }
            set
            {
                this._allowClose = value;
            }
        }

        [WebSysDescription("WebPart_AllowConnect"), DefaultValue(true), Personalizable(PersonalizationScope.Shared), Themeable(false), WebCategory("WebPartBehavior")]
        public virtual bool AllowConnect
        {
            get
            {
                return this._allowConnect;
            }
            set
            {
                this._allowConnect = value;
            }
        }

        [WebSysDescription("WebPart_AllowEdit"), DefaultValue(true), Personalizable(PersonalizationScope.Shared), Themeable(false), WebCategory("WebPartBehavior")]
        public virtual bool AllowEdit
        {
            get
            {
                return this._allowEdit;
            }
            set
            {
                this._allowEdit = value;
            }
        }

        [Personalizable(PersonalizationScope.Shared), DefaultValue(true), WebSysDescription("WebPart_AllowHide"), Themeable(false), WebCategory("WebPartBehavior")]
        public virtual bool AllowHide
        {
            get
            {
                return this._allowHide;
            }
            set
            {
                this._allowHide = value;
            }
        }

        [DefaultValue(true), WebSysDescription("WebPart_AllowMinimize"), Personalizable(PersonalizationScope.Shared), Themeable(false), WebCategory("WebPartBehavior")]
        public virtual bool AllowMinimize
        {
            get
            {
                return this._allowMinimize;
            }
            set
            {
                this._allowMinimize = value;
            }
        }

        [DefaultValue(true), WebSysDescription("WebPart_AllowZoneChange"), Personalizable(PersonalizationScope.Shared), Themeable(false), WebCategory("WebPartBehavior")]
        public virtual bool AllowZoneChange
        {
            get
            {
                return this._allowZoneChange;
            }
            set
            {
                this._allowZoneChange = value;
            }
        }

        [Themeable(false), DefaultValue(""), Personalizable(PersonalizationScope.Shared), WebCategory("WebPartBehavior"), WebSysDescription("WebPart_AuthorizationFilter")]
        public virtual string AuthorizationFilter
        {
            get
            {
                if (this._authorizationFilter == null)
                {
                    return string.Empty;
                }
                return this._authorizationFilter;
            }
            set
            {
                this._authorizationFilter = value;
            }
        }

        [DefaultValue(""), WebSysDescription("WebPart_CatalogIconImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("WebPartAppearance"), Personalizable(PersonalizationScope.Shared)]
        public virtual string CatalogIconImageUrl
        {
            get
            {
                if (this._catalogIconImageUrl == null)
                {
                    return string.Empty;
                }
                return this._catalogIconImageUrl;
            }
            set
            {
                if (CrossSiteScriptingValidation.IsDangerousUrl(value))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPart_BadUrl", new object[] { value }), "value");
                }
                this._catalogIconImageUrl = value;
            }
        }

        [Personalizable]
        public override PartChromeState ChromeState
        {
            get
            {
                return this._chromeState;
            }
            set
            {
                if ((value < PartChromeState.Normal) || (value > PartChromeState.Minimized))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._chromeState = value;
            }
        }

        [Personalizable]
        public override PartChromeType ChromeType
        {
            get
            {
                return base.ChromeType;
            }
            set
            {
                base.ChromeType = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string ConnectErrorMessage
        {
            get
            {
                if (this._connectErrorMessage == null)
                {
                    return string.Empty;
                }
                return this._connectErrorMessage;
            }
        }

        [Personalizable(PersonalizationScope.Shared)]
        public override string Description
        {
            get
            {
                return base.Description;
            }
            set
            {
                base.Description = value;
            }
        }

        [Personalizable]
        public override ContentDirection Direction
        {
            get
            {
                return base.Direction;
            }
            set
            {
                base.Direction = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string DisplayTitle
        {
            get
            {
                if (this._webPartManager != null)
                {
                    return this._webPartManager.GetDisplayTitle(this);
                }
                string title = this.Title;
                if (string.IsNullOrEmpty(title))
                {
                    title = System.Web.SR.GetString("Part_Untitled");
                }
                return title;
            }
        }

        [DefaultValue(0), WebSysDescription("WebPart_ExportMode"), Personalizable(PersonalizationScope.Shared), WebCategory("WebPartBehavior"), Themeable(false)]
        public virtual WebPartExportMode ExportMode
        {
            get
            {
                return this._exportMode;
            }
            set
            {
                if ((base.ControlState >= ControlState.Loaded) && ((this.WebPartManager == null) || ((this.WebPartManager.Personalization.Scope == PersonalizationScope.User) && this.IsShared)))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPart_CantSetExportMode"));
                }
                if ((value < WebPartExportMode.None) || (value > WebPartExportMode.NonSensitiveData))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._exportMode = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool HasSharedData
        {
            get
            {
                return this._hasSharedData;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool HasUserData
        {
            get
            {
                return this._hasUserData;
            }
        }

        [Personalizable]
        public override Unit Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
            }
        }

        [Themeable(false), Personalizable(PersonalizationScope.Shared), DefaultValue(2), WebCategory("WebPartBehavior"), WebSysDescription("WebPart_HelpMode")]
        public virtual WebPartHelpMode HelpMode
        {
            get
            {
                return this._helpMode;
            }
            set
            {
                if ((value < WebPartHelpMode.Modal) || (value > WebPartHelpMode.Navigate))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._helpMode = value;
            }
        }

        [Personalizable(PersonalizationScope.Shared), WebCategory("WebPartBehavior"), WebSysDescription("WebPart_HelpUrl"), UrlProperty, DefaultValue(""), Themeable(false), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string HelpUrl
        {
            get
            {
                if (this._helpUrl == null)
                {
                    return string.Empty;
                }
                return this._helpUrl;
            }
            set
            {
                if (CrossSiteScriptingValidation.IsDangerousUrl(value))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPart_BadUrl", new object[] { value }), "value");
                }
                this._helpUrl = value;
            }
        }

        [WebSysDescription("WebPart_Hidden"), DefaultValue(false), Personalizable, Themeable(false), WebCategory("WebPartAppearance")]
        public virtual bool Hidden
        {
            get
            {
                return this._hidden;
            }
            set
            {
                this._hidden = value;
            }
        }

        [WebCategory("WebPartAppearance"), Localizable(true), WebSysDefaultValue("WebPart_DefaultImportErrorMessage"), Personalizable(PersonalizationScope.Shared), WebSysDescription("WebPart_ImportErrorMessage")]
        public virtual string ImportErrorMessage
        {
            get
            {
                if (this._importErrorMessage == null)
                {
                    return System.Web.SR.GetString("WebPart_DefaultImportErrorMessage");
                }
                return this._importErrorMessage;
            }
            set
            {
                this._importErrorMessage = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsClosed
        {
            get
            {
                return this._isClosed;
            }
        }

        internal bool IsOrphaned
        {
            get
            {
                return ((this.Zone == null) && !this.IsClosed);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsShared
        {
            get
            {
                return this._isShared;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsStandalone
        {
            get
            {
                return this._isStandalone;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsStatic
        {
            get
            {
                return this._isStatic;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), Localizable(true)]
        public virtual string Subtitle
        {
            get
            {
                return string.Empty;
            }
        }

        [Personalizable]
        public override string Title
        {
            get
            {
                return base.Title;
            }
            set
            {
                base.Title = value;
            }
        }

        internal string TitleBarID
        {
            get
            {
                return ("WebPartTitle_" + this.ID);
            }
        }

        [WebSysDescription("WebPart_TitleIconImageUrl"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("WebPartAppearance"), Personalizable(PersonalizationScope.Shared)]
        public virtual string TitleIconImageUrl
        {
            get
            {
                if (this._titleIconImageUrl == null)
                {
                    return string.Empty;
                }
                return this._titleIconImageUrl;
            }
            set
            {
                if (CrossSiteScriptingValidation.IsDangerousUrl(value))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPart_BadUrl", new object[] { value }), "value");
                }
                this._titleIconImageUrl = value;
            }
        }

        [DefaultValue(""), WebSysDescription("WebPart_TitleUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, Personalizable(PersonalizationScope.Shared), Themeable(false), WebCategory("WebPartBehavior")]
        public virtual string TitleUrl
        {
            get
            {
                if (this._titleUrl == null)
                {
                    return string.Empty;
                }
                return this._titleUrl;
            }
            set
            {
                if (CrossSiteScriptingValidation.IsDangerousUrl(value))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPart_BadUrl", new object[] { value }), "value");
                }
                this._titleUrl = value;
            }
        }

        internal Dictionary<ProviderConnectionPoint, int> TrackerCounter
        {
            get
            {
                if (this._trackerCounter == null)
                {
                    this._trackerCounter = new Dictionary<ProviderConnectionPoint, int>();
                }
                return this._trackerCounter;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual WebPartVerbCollection Verbs
        {
            get
            {
                return WebPartVerbCollection.Empty;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual object WebBrowsableObject
        {
            get
            {
                return this;
            }
        }

        protected System.Web.UI.WebControls.WebParts.WebPartManager WebPartManager
        {
            get
            {
                return this._webPartManager;
            }
        }

        internal string WholePartID
        {
            get
            {
                return ("WebPart_" + this.ID);
            }
        }

        [Personalizable]
        public override Unit Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public WebPartZoneBase Zone
        {
            get
            {
                if (this._zone == null)
                {
                    string zoneID = this.ZoneID;
                    if (!string.IsNullOrEmpty(zoneID) && (this.WebPartManager != null))
                    {
                        WebPartZoneCollection zones = this.WebPartManager.Zones;
                        if (zones != null)
                        {
                            this._zone = zones[zoneID];
                        }
                    }
                }
                return this._zone;
            }
        }

        internal string ZoneID
        {
            get
            {
                return this._zoneID;
            }
            set
            {
                if (this.ZoneID != value)
                {
                    this._zoneID = value;
                    this._zone = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ZoneIndex
        {
            get
            {
                return this._zoneIndex;
            }
        }

        internal sealed class ZoneIndexComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                WebPart part = (WebPart) x;
                WebPart part2 = (WebPart) y;
                int num = part.ZoneIndex - part2.ZoneIndex;
                if (num == 0)
                {
                    num = part.ID.CompareTo(part2.ID);
                }
                return num;
            }
        }
    }
}

