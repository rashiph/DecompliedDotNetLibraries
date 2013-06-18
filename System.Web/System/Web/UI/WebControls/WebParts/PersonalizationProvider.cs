namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;

    public abstract class PersonalizationProvider : ProviderBase
    {
        private ICollection _supportedUserCapabilities;
        private const string scopeFieldName = "__WPPS";
        private const string sharedScopeFieldValue = "s";
        private const string userScopeFieldValue = "u";

        protected PersonalizationProvider()
        {
        }

        protected virtual IList CreateSupportedUserCapabilities()
        {
            ArrayList list = new ArrayList();
            list.Add(WebPartPersonalization.EnterSharedScopeUserCapability);
            list.Add(WebPartPersonalization.ModifyStateUserCapability);
            return list;
        }

        public virtual PersonalizationScope DetermineInitialScope(WebPartManager webPartManager, PersonalizationState loadedState)
        {
            if (webPartManager == null)
            {
                throw new ArgumentNullException("webPartManager");
            }
            Page page = webPartManager.Page;
            if (page == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "Page" }), "webPartManager");
            }
            HttpRequest requestInternal = page.RequestInternal;
            if (requestInternal == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "Page.Request" }), "webPartManager");
            }
            PersonalizationScope initialScope = webPartManager.Personalization.InitialScope;
            IPrincipal user = null;
            if (requestInternal.IsAuthenticated)
            {
                user = page.User;
            }
            if (user == null)
            {
                initialScope = PersonalizationScope.Shared;
            }
            else
            {
                if (page.IsPostBack)
                {
                    switch (page.Request["__WPPS"])
                    {
                        case "s":
                            initialScope = PersonalizationScope.Shared;
                            break;

                        case "u":
                            initialScope = PersonalizationScope.User;
                            break;
                    }
                }
                else if ((page.PreviousPage != null) && !page.PreviousPage.IsCrossPagePostBack)
                {
                    WebPartManager currentWebPartManager = WebPartManager.GetCurrentWebPartManager(page.PreviousPage);
                    if (currentWebPartManager != null)
                    {
                        initialScope = currentWebPartManager.Personalization.Scope;
                    }
                }
                else if (page.IsExportingWebPart)
                {
                    initialScope = page.IsExportingWebPartShared ? PersonalizationScope.Shared : PersonalizationScope.User;
                }
                if ((initialScope == PersonalizationScope.Shared) && !webPartManager.Personalization.CanEnterSharedScope)
                {
                    initialScope = PersonalizationScope.User;
                }
            }
            string hiddenFieldInitialValue = (initialScope == PersonalizationScope.Shared) ? "s" : "u";
            page.ClientScript.RegisterHiddenField("__WPPS", hiddenFieldInitialValue);
            return initialScope;
        }

        public virtual IDictionary DetermineUserCapabilities(WebPartManager webPartManager)
        {
            if (webPartManager == null)
            {
                throw new ArgumentNullException("webPartManager");
            }
            Page page = webPartManager.Page;
            if (page == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "Page" }), "webPartManager");
            }
            HttpRequest requestInternal = page.RequestInternal;
            if (requestInternal == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "Page.Request" }), "webPartManager");
            }
            IPrincipal user = null;
            if (requestInternal.IsAuthenticated)
            {
                user = page.User;
            }
            if (user != null)
            {
                if (this._supportedUserCapabilities == null)
                {
                    this._supportedUserCapabilities = this.CreateSupportedUserCapabilities();
                }
                if ((this._supportedUserCapabilities != null) && (this._supportedUserCapabilities.Count != 0))
                {
                    WebPartsSection webParts = RuntimeConfig.GetConfig().WebParts;
                    if (webParts != null)
                    {
                        WebPartsPersonalizationAuthorization authorization = webParts.Personalization.Authorization;
                        if (authorization != null)
                        {
                            IDictionary dictionary = new HybridDictionary();
                            foreach (WebPartUserCapability capability in this._supportedUserCapabilities)
                            {
                                if (authorization.IsUserAllowed(user, capability.Name))
                                {
                                    dictionary[capability] = capability;
                                }
                            }
                            return dictionary;
                        }
                    }
                }
            }
            return new HybridDictionary();
        }

        public abstract PersonalizationStateInfoCollection FindState(PersonalizationScope scope, PersonalizationStateQuery query, int pageIndex, int pageSize, out int totalRecords);
        public abstract int GetCountOfState(PersonalizationScope scope, PersonalizationStateQuery query);
        private void GetParameters(WebPartManager webPartManager, out string path, out string userName)
        {
            if (webPartManager == null)
            {
                throw new ArgumentNullException("webPartManager");
            }
            Page page = webPartManager.Page;
            if (page == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "Page" }), "webPartManager");
            }
            HttpRequest requestInternal = page.RequestInternal;
            if (requestInternal == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "Page.Request" }), "webPartManager");
            }
            path = requestInternal.AppRelativeCurrentExecutionFilePath;
            userName = null;
            if ((webPartManager.Personalization.Scope == PersonalizationScope.User) && page.Request.IsAuthenticated)
            {
                userName = page.User.Identity.Name;
            }
        }

        protected abstract void LoadPersonalizationBlobs(WebPartManager webPartManager, string path, string userName, ref byte[] sharedDataBlob, ref byte[] userDataBlob);
        public virtual PersonalizationState LoadPersonalizationState(WebPartManager webPartManager, bool ignoreCurrentUser)
        {
            string str;
            string str2;
            if (webPartManager == null)
            {
                throw new ArgumentNullException("webPartManager");
            }
            this.GetParameters(webPartManager, out str, out str2);
            if (ignoreCurrentUser)
            {
                str2 = null;
            }
            byte[] sharedDataBlob = null;
            byte[] userDataBlob = null;
            this.LoadPersonalizationBlobs(webPartManager, str, str2, ref sharedDataBlob, ref userDataBlob);
            BlobPersonalizationState state = new BlobPersonalizationState(webPartManager);
            state.LoadDataBlobs(sharedDataBlob, userDataBlob);
            return state;
        }

        protected abstract void ResetPersonalizationBlob(WebPartManager webPartManager, string path, string userName);
        public virtual void ResetPersonalizationState(WebPartManager webPartManager)
        {
            string str;
            string str2;
            if (webPartManager == null)
            {
                throw new ArgumentNullException("webPartManager");
            }
            this.GetParameters(webPartManager, out str, out str2);
            this.ResetPersonalizationBlob(webPartManager, str, str2);
        }

        public abstract int ResetState(PersonalizationScope scope, string[] paths, string[] usernames);
        public abstract int ResetUserState(string path, DateTime userInactiveSinceDate);
        protected abstract void SavePersonalizationBlob(WebPartManager webPartManager, string path, string userName, byte[] dataBlob);
        public virtual void SavePersonalizationState(PersonalizationState state)
        {
            string str;
            string str2;
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            BlobPersonalizationState state2 = state as BlobPersonalizationState;
            if (state2 == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationProvider_WrongType"), "state");
            }
            WebPartManager webPartManager = state2.WebPartManager;
            this.GetParameters(webPartManager, out str, out str2);
            byte[] dataBlob = null;
            bool isEmpty = state2.IsEmpty;
            if (!isEmpty)
            {
                dataBlob = state2.SaveDataBlob();
                isEmpty = (dataBlob == null) || (dataBlob.Length == 0);
            }
            if (isEmpty)
            {
                this.ResetPersonalizationBlob(webPartManager, str, str2);
            }
            else
            {
                this.SavePersonalizationBlob(webPartManager, str, str2, dataBlob);
            }
        }

        public abstract string ApplicationName { get; set; }
    }
}

