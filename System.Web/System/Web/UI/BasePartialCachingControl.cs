namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.UI.HtmlControls;
    using System.Web.Util;

    [ToolboxItem(false)]
    public abstract class BasePartialCachingControl : Control
    {
        internal Control _cachedCtrl;
        private CacheDependency _cacheDependency;
        private PartialCachingCacheEntry _cacheEntry;
        private string _cacheKey;
        private ControlCachePolicy _cachePolicy;
        internal bool _cachingDisabled;
        private string _cssStyleString;
        internal string _ctrlID;
        internal string _guid;
        private long _nonVaryHashCode;
        private string _outputString;
        internal string _provider;
        private ArrayList _registeredCallDataForEventValidation;
        private ArrayList _registeredStyleInfo;
        internal string _sqlDependency;
        internal bool _useSlidingExpiration;
        internal DateTime _utcExpirationTime;
        internal string[] _varyByControlsCollection;
        internal string _varyByCustom;
        internal HttpCacheVaryByParams _varyByParamsCollection;
        internal const char varySeparator = ';';
        internal const string varySeparatorString = ";";

        protected BasePartialCachingControl()
        {
        }

        private string ComputeNonVaryCacheKey(HashCodeCombiner combinedHashCode)
        {
            combinedHashCode.AddObject(this._guid);
            HttpBrowserCapabilities browser = this.Context.Request.Browser;
            if (browser != null)
            {
                combinedHashCode.AddObject(browser.TagWriter);
            }
            return ("l" + combinedHashCode.CombinedHashString);
        }

        private string ComputeVaryCacheKey(HashCodeCombiner combinedHashCode, ControlCachedVary cachedVary)
        {
            combinedHashCode.AddInt(1);
            NameValueCollection requestValueCollection = this.Page.RequestValueCollection;
            if (requestValueCollection == null)
            {
                requestValueCollection = this.Page.GetCollectionBasedOnMethod(true);
            }
            if (cachedVary._varyByParams != null)
            {
                ICollection is2;
                if ((cachedVary._varyByParams.Length == 1) && (cachedVary._varyByParams[0] == "*"))
                {
                    is2 = requestValueCollection;
                }
                else
                {
                    is2 = cachedVary._varyByParams;
                }
                foreach (string str in is2)
                {
                    combinedHashCode.AddCaseInsensitiveString(str);
                    string s = requestValueCollection[str];
                    if (s != null)
                    {
                        combinedHashCode.AddObject(s);
                    }
                }
            }
            if (cachedVary._varyByControls != null)
            {
                string str3;
                if (this.NamingContainer == this.Page)
                {
                    str3 = string.Empty;
                }
                else
                {
                    str3 = this.NamingContainer.UniqueID + base.IdSeparator;
                }
                str3 = str3 + this._ctrlID + base.IdSeparator;
                foreach (string str4 in cachedVary._varyByControls)
                {
                    string str5 = str3 + str4.Trim();
                    combinedHashCode.AddCaseInsensitiveString(str5);
                    if (requestValueCollection[str5] != null)
                    {
                        combinedHashCode.AddObject(requestValueCollection[str5]);
                    }
                }
            }
            if (cachedVary._varyByCustom != null)
            {
                string varyByCustomString = this.Context.ApplicationInstance.GetVaryByCustomString(this.Context, cachedVary._varyByCustom);
                if (varyByCustomString != null)
                {
                    combinedHashCode.AddObject(varyByCustomString);
                }
            }
            return ("l" + combinedHashCode.CombinedHashString);
        }

        internal abstract Control CreateCachedControl();
        public override void Dispose()
        {
            if (this._cacheDependency != null)
            {
                this._cacheDependency.Dispose();
                this._cacheDependency = null;
            }
            base.Dispose();
        }

        private string GetCssStyleRenderString(Type htmlTextWriterType)
        {
            if (this._registeredStyleInfo == null)
            {
                return null;
            }
            StringWriter tw = new StringWriter(CultureInfo.CurrentCulture);
            CssTextWriter cssWriter = new CssTextWriter(Page.CreateHtmlTextWriterFromType(tw, htmlTextWriterType));
            foreach (SelectorStyleInfo info in this._registeredStyleInfo)
            {
                HtmlHead.RenderCssRule(cssWriter, info.selector, info.style, info.urlResolver);
            }
            return tw.ToString();
        }

        internal override void InitRecursive(Control namingContainer)
        {
            HashCodeCombiner combinedHashCode = new HashCodeCombiner();
            this._cacheKey = this.ComputeNonVaryCacheKey(combinedHashCode);
            this._nonVaryHashCode = combinedHashCode.CombinedHash;
            PartialCachingCacheEntry entry = null;
            object fragment = OutputCache.GetFragment(this._cacheKey, this._provider);
            if (fragment != null)
            {
                ControlCachedVary cachedVary = fragment as ControlCachedVary;
                if (cachedVary != null)
                {
                    string key = this.ComputeVaryCacheKey(combinedHashCode, cachedVary);
                    entry = (PartialCachingCacheEntry) OutputCache.GetFragment(key, this._provider);
                    if ((entry != null) && (entry._cachedVaryId != cachedVary.CachedVaryId))
                    {
                        entry = null;
                        OutputCache.RemoveFragment(key, this._provider);
                    }
                }
                else
                {
                    entry = (PartialCachingCacheEntry) fragment;
                }
            }
            if (entry == null)
            {
                this._cacheEntry = new PartialCachingCacheEntry();
                this._cachedCtrl = this.CreateCachedControl();
                this.Controls.Add(this._cachedCtrl);
                this.Page.PushCachingControl(this);
                base.InitRecursive(namingContainer);
                this.Page.PopCachingControl();
            }
            else
            {
                this._outputString = entry.OutputString;
                this._cssStyleString = entry.CssStyleString;
                if (entry.RegisteredClientCalls != null)
                {
                    foreach (RegisterCallData data in entry.RegisteredClientCalls)
                    {
                        switch (data.Type)
                        {
                            case ClientAPIRegisterType.WebFormsScript:
                                this.Page.RegisterWebFormsScript();
                                break;

                            case ClientAPIRegisterType.PostBackScript:
                                this.Page.RegisterPostBackScript();
                                break;

                            case ClientAPIRegisterType.FocusScript:
                                this.Page.RegisterFocusScript();
                                break;

                            case ClientAPIRegisterType.ClientScriptBlocks:
                            case ClientAPIRegisterType.ClientScriptBlocksWithoutTags:
                            case ClientAPIRegisterType.ClientStartupScripts:
                            case ClientAPIRegisterType.ClientStartupScriptsWithoutTags:
                                this.Page.ClientScript.RegisterScriptBlock(data.Key, data.StringParam2, data.Type);
                                break;

                            case ClientAPIRegisterType.OnSubmitStatement:
                                this.Page.ClientScript.RegisterOnSubmitStatementInternal(data.Key, data.StringParam2);
                                break;

                            case ClientAPIRegisterType.ArrayDeclaration:
                                this.Page.ClientScript.RegisterArrayDeclaration(data.StringParam1, data.StringParam2);
                                break;

                            case ClientAPIRegisterType.HiddenField:
                                this.Page.ClientScript.RegisterHiddenField(data.StringParam1, data.StringParam2);
                                break;

                            case ClientAPIRegisterType.ExpandoAttribute:
                                this.Page.ClientScript.RegisterExpandoAttribute(data.StringParam1, data.StringParam2, data.StringParam3, false);
                                break;

                            case ClientAPIRegisterType.EventValidation:
                                if (this._registeredCallDataForEventValidation == null)
                                {
                                    this._registeredCallDataForEventValidation = new ArrayList();
                                }
                                this._registeredCallDataForEventValidation.Add(data);
                                break;
                        }
                    }
                }
                base.InitRecursive(namingContainer);
            }
        }

        internal override void LoadRecursive()
        {
            if (this._outputString != null)
            {
                base.LoadRecursive();
            }
            else
            {
                this.Page.PushCachingControl(this);
                base.LoadRecursive();
                this.Page.PopCachingControl();
            }
        }

        internal override void PreRenderRecursiveInternal()
        {
            if (this._outputString != null)
            {
                base.PreRenderRecursiveInternal();
                if ((this._cssStyleString != null) && (this.Page.Header != null))
                {
                    this.Page.Header.RegisterCssStyleString(this._cssStyleString);
                }
            }
            else
            {
                this.Page.PushCachingControl(this);
                base.PreRenderRecursiveInternal();
                this.Page.PopCachingControl();
            }
        }

        internal void RegisterArrayDeclaration(string arrayName, string arrayValue)
        {
            this.RegisterClientCall(ClientAPIRegisterType.ArrayDeclaration, arrayName, arrayValue);
        }

        private void RegisterClientCall(ClientAPIRegisterType type, string stringParam1, string stringParam2)
        {
            this.RegisterClientCall(type, stringParam1, stringParam2, null);
        }

        private void RegisterClientCall(ClientAPIRegisterType type, ScriptKey scriptKey, string stringParam2)
        {
            RegisterCallData data = new RegisterCallData {
                Type = type,
                Key = scriptKey,
                StringParam2 = stringParam2
            };
            if (this._cacheEntry.RegisteredClientCalls == null)
            {
                this._cacheEntry.RegisteredClientCalls = new ArrayList();
            }
            this._cacheEntry.RegisteredClientCalls.Add(data);
        }

        private void RegisterClientCall(ClientAPIRegisterType type, string stringParam1, string stringParam2, string stringParam3)
        {
            RegisterCallData data = new RegisterCallData {
                Type = type,
                StringParam1 = stringParam1,
                StringParam2 = stringParam2,
                StringParam3 = stringParam3
            };
            if (this._cacheEntry.RegisteredClientCalls == null)
            {
                this._cacheEntry.RegisteredClientCalls = new ArrayList();
            }
            this._cacheEntry.RegisteredClientCalls.Add(data);
        }

        internal void RegisterExpandoAttribute(string controlID, string attributeName, string attributeValue)
        {
            this.RegisterClientCall(ClientAPIRegisterType.ExpandoAttribute, controlID, attributeName, attributeValue);
        }

        internal void RegisterFocusScript()
        {
            this.RegisterClientCall(ClientAPIRegisterType.FocusScript, string.Empty, null);
        }

        internal void RegisterForEventValidation(string uniqueID, string argument)
        {
            this.RegisterClientCall(ClientAPIRegisterType.EventValidation, uniqueID, argument);
        }

        internal void RegisterHiddenField(string hiddenFieldName, string hiddenFieldInitialValue)
        {
            this.RegisterClientCall(ClientAPIRegisterType.HiddenField, hiddenFieldName, hiddenFieldInitialValue);
        }

        internal void RegisterOnSubmitStatement(ScriptKey key, string script)
        {
            this.RegisterClientCall(ClientAPIRegisterType.OnSubmitStatement, key, script);
        }

        internal void RegisterPostBackScript()
        {
            this.RegisterClientCall(ClientAPIRegisterType.PostBackScript, string.Empty, null);
        }

        internal void RegisterScriptBlock(ClientAPIRegisterType type, ScriptKey key, string script)
        {
            this.RegisterClientCall(type, key, script);
        }

        internal void RegisterStyleInfo(SelectorStyleInfo selectorInfo)
        {
            if (this._registeredStyleInfo == null)
            {
                this._registeredStyleInfo = new ArrayList();
            }
            this._registeredStyleInfo.Add(selectorInfo);
        }

        private void RegisterValidationEvents()
        {
            if (this._registeredCallDataForEventValidation != null)
            {
                foreach (RegisterCallData data in this._registeredCallDataForEventValidation)
                {
                    this.Page.ClientScript.RegisterForEventValidation(data.StringParam1, data.StringParam2);
                }
            }
        }

        internal void RegisterWebFormsScript()
        {
            this.RegisterClientCall(ClientAPIRegisterType.WebFormsScript, string.Empty, null);
        }

        protected internal override void Render(HtmlTextWriter output)
        {
            CacheDependency dependency = null;
            if (this._outputString != null)
            {
                output.Write(this._outputString);
                this.RegisterValidationEvents();
            }
            else if (this._cachingDisabled || !RuntimeConfig.GetAppConfig().OutputCache.EnableFragmentCache)
            {
                this._cachedCtrl.RenderControl(output);
            }
            else
            {
                string str;
                DateTime noAbsoluteExpiration;
                TimeSpan noSlidingExpiration;
                if (this._sqlDependency != null)
                {
                    dependency = SqlCacheDependency.CreateOutputCacheDependency(this._sqlDependency);
                }
                this._cacheEntry.CssStyleString = this.GetCssStyleRenderString(output.GetType());
                StringWriter tw = new StringWriter();
                HtmlTextWriter writer = Page.CreateHtmlTextWriterFromType(tw, output.GetType());
                TextWriter writer3 = this.Context.Response.SwitchWriter(tw);
                try
                {
                    this.Page.PushCachingControl(this);
                    this._cachedCtrl.RenderControl(writer);
                    this.Page.PopCachingControl();
                }
                finally
                {
                    this.Context.Response.SwitchWriter(writer3);
                }
                this._cacheEntry.OutputString = tw.ToString();
                output.Write(this._cacheEntry.OutputString);
                CacheDependency dependencies = this._cacheDependency;
                if (dependency != null)
                {
                    if (dependencies == null)
                    {
                        dependencies = dependency;
                    }
                    else
                    {
                        AggregateCacheDependency dependency3 = new AggregateCacheDependency();
                        dependency3.Add(new CacheDependency[] { dependencies });
                        dependency3.Add(new CacheDependency[] { dependency });
                        dependencies = dependency3;
                    }
                }
                ControlCachedVary cachedVary = null;
                if (((this._varyByParamsCollection == null) && (this._varyByControlsCollection == null)) && (this._varyByCustom == null))
                {
                    str = this._cacheKey;
                }
                else
                {
                    string[] varyByParams = null;
                    if (this._varyByParamsCollection != null)
                    {
                        varyByParams = this._varyByParamsCollection.GetParams();
                    }
                    cachedVary = new ControlCachedVary(varyByParams, this._varyByControlsCollection, this._varyByCustom);
                    HashCodeCombiner combinedHashCode = new HashCodeCombiner(this._nonVaryHashCode);
                    str = this.ComputeVaryCacheKey(combinedHashCode, cachedVary);
                }
                if (this._useSlidingExpiration)
                {
                    noAbsoluteExpiration = Cache.NoAbsoluteExpiration;
                    noSlidingExpiration = (TimeSpan) (this._utcExpirationTime - DateTime.UtcNow);
                }
                else
                {
                    noAbsoluteExpiration = this._utcExpirationTime;
                    noSlidingExpiration = Cache.NoSlidingExpiration;
                }
                try
                {
                    OutputCache.InsertFragment(this._cacheKey, cachedVary, str, this._cacheEntry, dependencies, noAbsoluteExpiration, noSlidingExpiration, this._provider);
                }
                catch
                {
                    if (dependencies != null)
                    {
                        dependencies.Dispose();
                    }
                    throw;
                }
            }
        }

        internal void SetVaryByParamsCollectionFromString(string varyByParams)
        {
            if (varyByParams != null)
            {
                string[] parameters = varyByParams.Split(new char[] { ';' });
                this._varyByParamsCollection = new HttpCacheVaryByParams();
                this._varyByParamsCollection.ResetFromParams(parameters);
            }
        }

        public ControlCachePolicy CachePolicy
        {
            get
            {
                if (this._cachePolicy == null)
                {
                    this._cachePolicy = new ControlCachePolicy(this);
                }
                return this._cachePolicy;
            }
        }

        public CacheDependency Dependency
        {
            get
            {
                return this._cacheDependency;
            }
            set
            {
                this._cacheDependency = value;
            }
        }

        internal TimeSpan Duration
        {
            get
            {
                if (this._utcExpirationTime == DateTime.MaxValue)
                {
                    return TimeSpan.MaxValue;
                }
                return (TimeSpan) (this._utcExpirationTime - DateTime.UtcNow);
            }
            set
            {
                if (value == TimeSpan.MaxValue)
                {
                    this._utcExpirationTime = DateTime.MaxValue;
                }
                else
                {
                    this._utcExpirationTime = DateTime.UtcNow.Add(value);
                }
            }
        }

        internal string VaryByControl
        {
            get
            {
                if (this._varyByControlsCollection == null)
                {
                    return string.Empty;
                }
                return string.Join(";", this._varyByControlsCollection);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this._varyByControlsCollection = null;
                }
                else
                {
                    this._varyByControlsCollection = value.Split(new char[] { ';' });
                }
            }
        }

        internal HttpCacheVaryByParams VaryByParams
        {
            get
            {
                if (this._varyByParamsCollection == null)
                {
                    this._varyByParamsCollection = new HttpCacheVaryByParams();
                    this._varyByParamsCollection.IgnoreParams = true;
                }
                return this._varyByParamsCollection;
            }
        }
    }
}

