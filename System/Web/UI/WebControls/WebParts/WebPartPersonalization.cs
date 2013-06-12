namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration.Provider;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    [TypeConverter(typeof(EmptyStringExpandableObjectConverter))]
    public class WebPartPersonalization
    {
        private PersonalizationScope _currentScope;
        private bool _enabled;
        private bool _initialized;
        private bool _initializedSet;
        private PersonalizationScope _initialScope;
        private System.Web.UI.WebControls.WebParts.WebPartManager _owner;
        private PersonalizationState _personalizationState;
        private PersonalizationProvider _provider;
        private string _providerName;
        private bool _scopeToggled;
        private bool _shouldResetPersonalizationState;
        private IDictionary _userCapabilities;
        public static readonly WebPartUserCapability EnterSharedScopeUserCapability = new WebPartUserCapability("enterSharedScope");
        public static readonly WebPartUserCapability ModifyStateUserCapability = new WebPartUserCapability("modifyState");

        public WebPartPersonalization(System.Web.UI.WebControls.WebParts.WebPartManager owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this._owner = owner;
            this._enabled = true;
        }

        protected internal virtual void ApplyPersonalizationState()
        {
            if (this.IsEnabled)
            {
                this.EnsurePersonalizationState();
                this._personalizationState.ApplyWebPartManagerPersonalization();
            }
        }

        protected internal virtual void ApplyPersonalizationState(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (this.IsEnabled)
            {
                this.EnsurePersonalizationState();
                this._personalizationState.ApplyWebPartPersonalization(webPart);
            }
        }

        private void ApplyPersonalizationState(Control control, PersonalizationInfo info)
        {
            ITrackingPersonalizable personalizable = control as ITrackingPersonalizable;
            IPersonalizable personalizable2 = control as IPersonalizable;
            if (personalizable != null)
            {
                personalizable.BeginLoad();
            }
            if (((personalizable2 != null) && (info.CustomProperties != null)) && (info.CustomProperties.Count > 0))
            {
                personalizable2.Load(info.CustomProperties);
            }
            if ((info.Properties != null) && (info.Properties.Count > 0))
            {
                BlobPersonalizationState.SetPersonalizedProperties(control, info.Properties);
            }
            if (personalizable != null)
            {
                personalizable.EndLoad();
            }
        }

        protected virtual void ChangeScope(PersonalizationScope scope)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            if (scope != this._currentScope)
            {
                if ((scope == PersonalizationScope.Shared) && !this.CanEnterSharedScope)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartPersonalization_CannotEnterSharedScope"));
                }
                this._currentScope = scope;
                this._scopeToggled = true;
            }
        }

        private void CopyPersonalizationState(Control controlA, Control controlB)
        {
            PersonalizationInfo info = this.ExtractPersonalizationState(controlA);
            this.ApplyPersonalizationState(controlB, info);
        }

        protected internal virtual void CopyPersonalizationState(WebPart webPartA, WebPart webPartB)
        {
            if (webPartA == null)
            {
                throw new ArgumentNullException("webPartA");
            }
            if (webPartB == null)
            {
                throw new ArgumentNullException("webPartB");
            }
            if (webPartA.GetType() != webPartB.GetType())
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartPersonalization_SameType", new object[] { "webPartA", "webPartB" }));
            }
            this.CopyPersonalizationState((Control) webPartA, (Control) webPartB);
            GenericWebPart part = webPartA as GenericWebPart;
            GenericWebPart part2 = webPartB as GenericWebPart;
            if ((part != null) && (part2 != null))
            {
                Control childControl = part.ChildControl;
                Control controlB = part2.ChildControl;
                if (childControl == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "ChildControl" }), "webPartA");
                }
                if (controlB == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "ChildControl" }), "webPartB");
                }
                if (childControl.GetType() != controlB.GetType())
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPartPersonalization_SameType", new object[] { "webPartA.ChildControl", "webPartB.ChildControl" }));
                }
                this.CopyPersonalizationState(childControl, controlB);
            }
            this.SetDirty(webPartB);
        }

        private void DeterminePersonalizationProvider()
        {
            string providerName = this.ProviderName;
            if (string.IsNullOrEmpty(providerName))
            {
                this._provider = PersonalizationAdministration.Provider;
            }
            else
            {
                PersonalizationProvider provider = PersonalizationAdministration.Providers[providerName];
                if (provider == null)
                {
                    throw new ProviderException(System.Web.SR.GetString("WebPartPersonalization_ProviderNotFound", new object[] { providerName }));
                }
                this._provider = provider;
            }
        }

        public void EnsureEnabled(bool ensureModifiable)
        {
            if (!(ensureModifiable ? this.IsModifiable : this.IsEnabled))
            {
                string str;
                if (ensureModifiable)
                {
                    str = System.Web.SR.GetString("WebPartPersonalization_PersonalizationNotModifiable");
                }
                else
                {
                    str = System.Web.SR.GetString("WebPartPersonalization_PersonalizationNotEnabled");
                }
                throw new InvalidOperationException(str);
            }
        }

        private void EnsurePersonalizationState()
        {
            if (this._personalizationState == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartPersonalization_PersonalizationStateNotLoaded"));
            }
        }

        protected internal virtual void ExtractPersonalizationState()
        {
            if (this.IsEnabled && !this.ShouldResetPersonalizationState)
            {
                this.EnsurePersonalizationState();
                this._personalizationState.ExtractWebPartManagerPersonalization();
            }
        }

        private PersonalizationInfo ExtractPersonalizationState(Control control)
        {
            ITrackingPersonalizable personalizable = control as ITrackingPersonalizable;
            IPersonalizable personalizable2 = control as IPersonalizable;
            if (personalizable != null)
            {
                personalizable.BeginSave();
            }
            PersonalizationInfo info = new PersonalizationInfo();
            if (personalizable2 != null)
            {
                info.CustomProperties = new PersonalizationDictionary();
                personalizable2.Save(info.CustomProperties);
            }
            info.Properties = BlobPersonalizationState.GetPersonalizedProperties(control, PersonalizationScope.Shared);
            if (personalizable != null)
            {
                personalizable.EndSave();
            }
            return info;
        }

        protected internal virtual void ExtractPersonalizationState(WebPart webPart)
        {
            if (this.IsEnabled && !this.ShouldResetPersonalizationState)
            {
                this.EnsurePersonalizationState();
                this._personalizationState.ExtractWebPartPersonalization(webPart);
            }
        }

        protected internal virtual string GetAuthorizationFilter(string webPartID)
        {
            if (string.IsNullOrEmpty(webPartID))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("webPartID");
            }
            this.EnsureEnabled(false);
            this.EnsurePersonalizationState();
            return this._personalizationState.GetAuthorizationFilter(webPartID);
        }

        protected virtual PersonalizationScope Load()
        {
            if (!this.Enabled)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartPersonalization_PersonalizationNotEnabled"));
            }
            this.DeterminePersonalizationProvider();
            Page page = this.WebPartManager.Page;
            if (page == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "WebPartManager.Page" }));
            }
            HttpRequest requestInternal = page.RequestInternal;
            if (requestInternal == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "WebPartManager.Page.Request" }));
            }
            if (requestInternal.IsAuthenticated)
            {
                this._userCapabilities = this._provider.DetermineUserCapabilities(this.WebPartManager);
            }
            this._personalizationState = this._provider.LoadPersonalizationState(this.WebPartManager, false);
            if (this._personalizationState == null)
            {
                throw new ProviderException(System.Web.SR.GetString("WebPartPersonalization_CannotLoadPersonalization"));
            }
            return this._provider.DetermineInitialScope(this.WebPartManager, this._personalizationState);
        }

        internal void LoadInternal()
        {
            if (this.Enabled)
            {
                this._currentScope = this.Load();
                this._initialized = true;
            }
            this._initializedSet = true;
        }

        public virtual void ResetPersonalizationState()
        {
            this.EnsureEnabled(true);
            if (this._provider == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartPersonalization_CantCallMethodBeforeInit", new object[] { "ResetPersonalizationState", "WebPartPersonalization" }));
            }
            this._provider.ResetPersonalizationState(this.WebPartManager);
            this.ShouldResetPersonalizationState = true;
            Page page = this.WebPartManager.Page;
            if (page == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "WebPartManager.Page" }));
            }
            this.TransferToCurrentPage(page);
        }

        protected virtual void Save()
        {
            this.EnsureEnabled(true);
            this.EnsurePersonalizationState();
            if (this._provider == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartPersonalization_CantCallMethodBeforeInit", new object[] { "Save", "WebPartPersonalization" }));
            }
            if (this._personalizationState.IsDirty && !this.ShouldResetPersonalizationState)
            {
                this._provider.SavePersonalizationState(this._personalizationState);
            }
        }

        internal void SaveInternal()
        {
            if (this.IsModifiable)
            {
                this.Save();
            }
        }

        protected internal virtual void SetDirty()
        {
            if (this.IsEnabled)
            {
                this.EnsurePersonalizationState();
                this._personalizationState.SetWebPartManagerDirty();
            }
        }

        protected internal virtual void SetDirty(WebPart webPart)
        {
            if (this.IsEnabled)
            {
                this.EnsurePersonalizationState();
                this._personalizationState.SetWebPartDirty(webPart);
            }
        }

        public virtual void ToggleScope()
        {
            this.EnsureEnabled(false);
            Page page = this.WebPartManager.Page;
            if (page == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "WebPartManager.Page" }));
            }
            if (!page.IsExportingWebPart)
            {
                Page previousPage = page.PreviousPage;
                if ((previousPage != null) && !previousPage.IsCrossPagePostBack)
                {
                    System.Web.UI.WebControls.WebParts.WebPartManager currentWebPartManager = System.Web.UI.WebControls.WebParts.WebPartManager.GetCurrentWebPartManager(previousPage);
                    if ((currentWebPartManager != null) && currentWebPartManager.Personalization.ScopeToggled)
                    {
                        return;
                    }
                }
                if (this._currentScope == PersonalizationScope.Shared)
                {
                    this.ChangeScope(PersonalizationScope.User);
                }
                else
                {
                    this.ChangeScope(PersonalizationScope.Shared);
                }
                this.TransferToCurrentPage(page);
            }
        }

        private void TransferToCurrentPage(Page page)
        {
            HttpRequest requestInternal = page.RequestInternal;
            if (requestInternal == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "WebPartManager.Page.Request" }));
            }
            string currentExecutionFilePath = requestInternal.CurrentExecutionFilePath;
            if ((page.Form == null) || string.Equals(page.Form.Method, "post", StringComparison.OrdinalIgnoreCase))
            {
                string clientQueryString = page.ClientQueryString;
                if (!string.IsNullOrEmpty(clientQueryString))
                {
                    currentExecutionFilePath = currentExecutionFilePath + "?" + clientQueryString;
                }
            }
            IScriptManager scriptManager = page.ScriptManager;
            if ((scriptManager != null) && scriptManager.IsInAsyncPostBack)
            {
                requestInternal.Response.Redirect(currentExecutionFilePath);
            }
            else
            {
                page.Server.Transfer(currentExecutionFilePath, false);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanEnterSharedScope
        {
            get
            {
                IDictionary userCapabilities = this.UserCapabilities;
                return ((userCapabilities != null) && userCapabilities.Contains(EnterSharedScopeUserCapability));
            }
        }

        [WebSysDescription("WebPartPersonalization_Enabled"), NotifyParentProperty(true), DefaultValue(true)]
        public virtual bool Enabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                if ((!this.WebPartManager.DesignMode && this._initializedSet) && (value != this.Enabled))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartPersonalization_MustSetBeforeInit", new object[] { "Enabled", "WebPartPersonalization" }));
                }
                this._enabled = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool HasPersonalizationState
        {
            get
            {
                if (this._provider == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartPersonalization_CantUsePropertyBeforeInit", new object[] { "HasPersonalizationState", "WebPartPersonalization" }));
                }
                Page page = this.WebPartManager.Page;
                if (page == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "WebPartManager.Page" }));
                }
                HttpRequest requestInternal = page.RequestInternal;
                if (requestInternal == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "WebPartManager.Page.Request" }));
                }
                PersonalizationStateQuery query = new PersonalizationStateQuery {
                    PathToMatch = requestInternal.AppRelativeCurrentExecutionFilePath
                };
                if ((this.Scope == PersonalizationScope.User) && requestInternal.IsAuthenticated)
                {
                    query.UsernameToMatch = page.User.Identity.Name;
                }
                return (this._provider.GetCountOfState(this.Scope, query) > 0);
            }
        }

        [NotifyParentProperty(true), DefaultValue(0), WebSysDescription("WebPartPersonalization_InitialScope")]
        public virtual PersonalizationScope InitialScope
        {
            get
            {
                return this._initialScope;
            }
            set
            {
                if ((value < PersonalizationScope.User) || (value > PersonalizationScope.Shared))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if ((!this.WebPartManager.DesignMode && this._initializedSet) && (value != this.InitialScope))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartPersonalization_MustSetBeforeInit", new object[] { "InitialScope", "WebPartPersonalization" }));
                }
                this._initialScope = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsEnabled
        {
            get
            {
                return this.IsInitialized;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected bool IsInitialized
        {
            get
            {
                return this._initialized;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsModifiable
        {
            get
            {
                IDictionary userCapabilities = this.UserCapabilities;
                return ((userCapabilities != null) && userCapabilities.Contains(ModifyStateUserCapability));
            }
        }

        [DefaultValue(""), WebSysDescription("WebPartPersonalization_ProviderName"), NotifyParentProperty(true)]
        public virtual string ProviderName
        {
            get
            {
                if (this._providerName == null)
                {
                    return string.Empty;
                }
                return this._providerName;
            }
            set
            {
                if ((!this.WebPartManager.DesignMode && this._initializedSet) && !string.Equals(value, this.ProviderName, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartPersonalization_MustSetBeforeInit", new object[] { "ProviderName", "WebPartPersonalization" }));
                }
                this._providerName = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public PersonalizationScope Scope
        {
            get
            {
                return this._currentScope;
            }
        }

        internal bool ScopeToggled
        {
            get
            {
                return this._scopeToggled;
            }
        }

        protected bool ShouldResetPersonalizationState
        {
            get
            {
                return this._shouldResetPersonalizationState;
            }
            set
            {
                this._shouldResetPersonalizationState = value;
            }
        }

        protected virtual IDictionary UserCapabilities
        {
            get
            {
                if (this._userCapabilities == null)
                {
                    this._userCapabilities = new HybridDictionary();
                }
                return this._userCapabilities;
            }
        }

        protected System.Web.UI.WebControls.WebParts.WebPartManager WebPartManager
        {
            get
            {
                return this._owner;
            }
        }

        private sealed class PersonalizationInfo
        {
            public PersonalizationDictionary CustomProperties;
            public IDictionary Properties;
        }
    }
}

