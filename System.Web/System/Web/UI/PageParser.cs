namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    public sealed class PageParser : TemplateControlParser
    {
        private int _codePage;
        private string _configMasterPageFile;
        private string _culture;
        private string _errorPage;
        private int _lcid;
        private int _mainDirectiveLineNumber = 1;
        private bool _mainDirectiveMasterPageSet;
        private Type _masterPageType;
        private OutputCacheLocation _outputCacheLocation;
        private Type _previousPageType;
        private string _responseEncoding;
        private string _styleSheetTheme;
        private TraceEnable _traceEnabled;
        private System.Web.TraceMode _traceMode = System.Web.TraceMode.Default;
        private int _transactionMode;
        internal const string defaultDirectiveName = "page";
        private static Type s_defaultApplicationBaseType;
        private static Type s_defaultPageBaseType;
        private static Type s_defaultPageParserFilterType;
        private static Type s_defaultUserContorlBaseType;
        private static bool s_enableLongStringsAsResources = true;
        private static object s_lock = new object();

        public PageParser()
        {
            this.flags[0x80000] = true;
            this.flags[0x100000] = true;
            this.flags[0x400000] = true;
        }

        private void ApplyBaseType()
        {
            if (DefaultPageBaseType != null)
            {
                base.BaseType = DefaultPageBaseType;
            }
            else if ((base.PagesConfig != null) && (base.PagesConfig.PageBaseTypeInternal != null))
            {
                base.BaseType = base.PagesConfig.PageBaseTypeInternal;
            }
        }

        internal override RootBuilder CreateDefaultFileLevelBuilder()
        {
            return new FileLevelPageControlBuilder();
        }

        private void EnsureMasterPageFileFromConfigApplied()
        {
            if (!this._mainDirectiveMasterPageSet)
            {
                if (this._configMasterPageFile != null)
                {
                    int num = base._lineNumber;
                    base._lineNumber = this._mainDirectiveLineNumber;
                    try
                    {
                        if (this._configMasterPageFile.Length > 0)
                        {
                            Type referencedType = base.GetReferencedType(this._configMasterPageFile);
                            if (!typeof(MasterPage).IsAssignableFrom(referencedType))
                            {
                                base.ProcessError(System.Web.SR.GetString("Invalid_master_base", new object[] { this._configMasterPageFile }));
                            }
                        }
                        if (((FileLevelPageControlBuilder) base.RootBuilder).ContentBuilderEntries != null)
                        {
                            base.RootBuilder.SetControlType(base.BaseType);
                            base.RootBuilder.PreprocessAttribute(string.Empty, "MasterPageFile", this._configMasterPageFile, true);
                        }
                    }
                    finally
                    {
                        base._lineNumber = num;
                    }
                }
                this._mainDirectiveMasterPageSet = true;
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static IHttpHandler GetCompiledPageInstance(string virtualPath, string inputFile, HttpContext context)
        {
            if (!string.IsNullOrEmpty(inputFile))
            {
                inputFile = Path.GetFullPath(inputFile);
            }
            if (inputFile != null)
            {
                lock (s_lock)
                {
                    return GetCompiledPageInstance(VirtualPath.Create(virtualPath), inputFile, context);
                }
            }
            return GetCompiledPageInstance(VirtualPath.Create(virtualPath), inputFile, context);
        }

        private static IHttpHandler GetCompiledPageInstance(VirtualPath virtualPath, string inputFile, HttpContext context)
        {
            IHttpHandler handler;
            if (context != null)
            {
                virtualPath = context.Request.FilePathObject.Combine(virtualPath);
            }
            object state = null;
            try
            {
                try
                {
                    if (inputFile != null)
                    {
                        state = HostingEnvironment.AddVirtualPathToFileMapping(virtualPath, inputFile);
                    }
                    BuildResultCompiledType type = (BuildResultCompiledType) BuildManager.GetVPathBuildResult(context, virtualPath, false, true, true, true);
                    handler = (IHttpHandler) HttpRuntime.CreatePublicInstance(type.ResultType);
                }
                finally
                {
                    if (state != null)
                    {
                        HostingEnvironment.ClearVirtualPathToFileMapping(state);
                    }
                }
            }
            catch
            {
                throw;
            }
            return handler;
        }

        internal override void HandlePostParse()
        {
            base.HandlePostParse();
            this.EnsureMasterPageFileFromConfigApplied();
        }

        private void ParseTransactionAttribute(string name, string value)
        {
            object obj2 = System.Web.UI.Util.GetEnumAttribute(name, value, typeof(TransactionOption));
            if (obj2 != null)
            {
                this._transactionMode = (int) obj2;
                if (this._transactionMode != 0)
                {
                    if (!HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                    {
                        throw new HttpException(System.Web.SR.GetString("Insufficient_trust_for_attribute", new object[] { "transaction" }));
                    }
                    base.AddAssemblyDependency(typeof(TransactionOption).Assembly);
                }
            }
        }

        internal override void PostProcessMainDirectiveAttributes(IDictionary parseData)
        {
            if (!this.flags[0x80000] && (this._errorPage != null))
            {
                base.ProcessError(System.Web.SR.GetString("Error_page_not_supported_when_buffering_off"));
            }
            else if ((this._culture != null) && (this._lcid > 0))
            {
                base.ProcessError(System.Web.SR.GetString("Attributes_mutually_exclusive", new object[] { "Culture", "LCID" }));
            }
            else if ((this._responseEncoding != null) && (this._codePage > 0))
            {
                base.ProcessError(System.Web.SR.GetString("Attributes_mutually_exclusive", new object[] { "ResponseEncoding", "CodePage" }));
            }
            else if (this.AsyncMode && this.AspCompatMode)
            {
                base.ProcessError(System.Web.SR.GetString("Async_and_aspcompat"));
            }
            else if (this.AsyncMode && (this._transactionMode != 0))
            {
                base.ProcessError(System.Web.SR.GetString("Async_and_transaction"));
            }
            else
            {
                base.PostProcessMainDirectiveAttributes(parseData);
            }
        }

        internal override void ProcessConfigSettings()
        {
            base.ProcessConfigSettings();
            if (base.PagesConfig != null)
            {
                if (!base.PagesConfig.Buffer)
                {
                    base._mainDirectiveConfigSettings["buffer"] = System.Web.UI.Util.GetStringFromBool(base.PagesConfig.Buffer);
                }
                if (!base.PagesConfig.EnableViewStateMac)
                {
                    base._mainDirectiveConfigSettings["enableviewstatemac"] = System.Web.UI.Util.GetStringFromBool(base.PagesConfig.EnableViewStateMac);
                }
                if (!base.PagesConfig.EnableEventValidation)
                {
                    base._mainDirectiveConfigSettings["enableEventValidation"] = System.Web.UI.Util.GetStringFromBool(base.PagesConfig.EnableEventValidation);
                }
                if (base.PagesConfig.SmartNavigation)
                {
                    base._mainDirectiveConfigSettings["smartnavigation"] = System.Web.UI.Util.GetStringFromBool(base.PagesConfig.SmartNavigation);
                }
                if ((base.PagesConfig.ThemeInternal != null) && (base.PagesConfig.Theme.Length != 0))
                {
                    base._mainDirectiveConfigSettings["theme"] = base.PagesConfig.Theme;
                }
                if ((base.PagesConfig.StyleSheetThemeInternal != null) && (base.PagesConfig.StyleSheetThemeInternal.Length != 0))
                {
                    base._mainDirectiveConfigSettings["stylesheettheme"] = base.PagesConfig.StyleSheetThemeInternal;
                }
                if ((base.PagesConfig.MasterPageFileInternal != null) && (base.PagesConfig.MasterPageFileInternal.Length != 0))
                {
                    this._configMasterPageFile = base.PagesConfig.MasterPageFileInternal;
                }
                if (base.PagesConfig.ViewStateEncryptionMode != ViewStateEncryptionMode.Auto)
                {
                    base._mainDirectiveConfigSettings["viewStateEncryptionMode"] = Enum.Format(typeof(ViewStateEncryptionMode), base.PagesConfig.ViewStateEncryptionMode, "G");
                }
                if (base.PagesConfig.MaintainScrollPositionOnPostBack)
                {
                    base._mainDirectiveConfigSettings["maintainScrollPositionOnPostBack"] = System.Web.UI.Util.GetStringFromBool(base.PagesConfig.MaintainScrollPositionOnPostBack);
                }
                if (base.PagesConfig.MaxPageStateFieldLength != Page.DefaultMaxPageStateFieldLength)
                {
                    base._mainDirectiveConfigSettings["maxPageStateFieldLength"] = base.PagesConfig.MaxPageStateFieldLength;
                }
                this.flags[0x100000] = (base.PagesConfig.EnableSessionState == PagesEnableSessionState.True) || (base.PagesConfig.EnableSessionState == PagesEnableSessionState.ReadOnly);
                this.flags[0x200000] = base.PagesConfig.EnableSessionState == PagesEnableSessionState.ReadOnly;
                this.flags[0x400000] = base.PagesConfig.ValidateRequest;
                this.flags[0x40] = HttpRuntime.ApartmentThreading;
            }
            this.ApplyBaseType();
        }

        internal override void ProcessDirective(string directiveName, IDictionary directive)
        {
            if (StringUtil.EqualsIgnoreCase(directiveName, "previousPageType"))
            {
                if (this._previousPageType != null)
                {
                    base.ProcessError(System.Web.SR.GetString("Only_one_directive_allowed", new object[] { directiveName }));
                }
                else
                {
                    this._previousPageType = base.GetDirectiveType(directive, directiveName);
                    System.Web.UI.Util.CheckAssignableType(typeof(Page), this._previousPageType);
                }
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "masterType"))
            {
                if (this._masterPageType != null)
                {
                    base.ProcessError(System.Web.SR.GetString("Only_one_directive_allowed", new object[] { directiveName }));
                }
                else
                {
                    this._masterPageType = base.GetDirectiveType(directive, directiveName);
                    System.Web.UI.Util.CheckAssignableType(typeof(MasterPage), this._masterPageType);
                }
            }
            else
            {
                base.ProcessDirective(directiveName, directive);
            }
        }

        internal override void ProcessMainDirective(IDictionary mainDirective)
        {
            this._mainDirectiveLineNumber = base._lineNumber;
            base.ProcessMainDirective(mainDirective);
        }

        internal override bool ProcessMainDirectiveAttribute(string deviceName, string name, string value, IDictionary parseData)
        {
            switch (name)
            {
                case "errorpage":
                    this._errorPage = System.Web.UI.Util.GetNonEmptyAttribute(name, value);
                    return false;

                case "contenttype":
                    System.Web.UI.Util.GetNonEmptyAttribute(name, value);
                    return false;

                case "theme":
                    if (!base.IsExpressionBuilderValue(value))
                    {
                        System.Web.UI.Util.CheckThemeAttribute(value);
                        return false;
                    }
                    return false;

                case "stylesheettheme":
                    base.ValidateBuiltInAttribute(deviceName, name, value);
                    System.Web.UI.Util.CheckThemeAttribute(value);
                    this._styleSheetTheme = value;
                    return true;

                case "enablesessionstate":
                    this.flags[0x100000] = true;
                    this.flags[0x200000] = false;
                    if (!System.Web.UI.Util.IsFalseString(value))
                    {
                        if (StringUtil.EqualsIgnoreCase(value, "readonly"))
                        {
                            this.flags[0x200000] = true;
                        }
                        else if (!System.Web.UI.Util.IsTrueString(value))
                        {
                            base.ProcessError(System.Web.SR.GetString("Enablesessionstate_must_be_true_false_or_readonly"));
                        }
                        break;
                    }
                    this.flags[0x100000] = false;
                    break;

                case "culture":
                    this._culture = System.Web.UI.Util.GetNonEmptyAttribute(name, value);
                    if (!HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                    {
                        throw new HttpException(System.Web.SR.GetString("Insufficient_trust_for_attribute", new object[] { "culture" }));
                    }
                    if (!StringUtil.EqualsIgnoreCase(value, HttpApplication.AutoCulture))
                    {
                        CultureInfo info;
                        try
                        {
                            if (StringUtil.StringStartsWithIgnoreCase(value, HttpApplication.AutoCulture))
                            {
                                this._culture = this._culture.Substring(5);
                            }
                            info = HttpServerUtility.CreateReadOnlyCultureInfo(this._culture);
                        }
                        catch
                        {
                            base.ProcessError(System.Web.SR.GetString("Invalid_attribute_value", new object[] { this._culture, "culture" }));
                            return false;
                        }
                        if (info.IsNeutralCulture)
                        {
                            base.ProcessError(System.Web.SR.GetString("Invalid_culture_attribute", new object[] { System.Web.UI.Util.GetSpecificCulturesFormattedList(info) }));
                        }
                    }
                    return false;

                case "lcid":
                    if (!base.IsExpressionBuilderValue(value))
                    {
                        this._lcid = System.Web.UI.Util.GetNonNegativeIntegerAttribute(name, value);
                        try
                        {
                            HttpServerUtility.CreateReadOnlyCultureInfo(this._lcid);
                        }
                        catch
                        {
                            base.ProcessError(System.Web.SR.GetString("Invalid_attribute_value", new object[] { this._lcid.ToString(CultureInfo.InvariantCulture), "lcid" }));
                        }
                        return false;
                    }
                    return false;

                case "uiculture":
                    System.Web.UI.Util.GetNonEmptyAttribute(name, value);
                    return false;

                case "responseencoding":
                    if (!base.IsExpressionBuilderValue(value))
                    {
                        this._responseEncoding = System.Web.UI.Util.GetNonEmptyAttribute(name, value);
                        Encoding.GetEncoding(this._responseEncoding);
                        return false;
                    }
                    return false;

                case "codepage":
                    if (!base.IsExpressionBuilderValue(value))
                    {
                        this._codePage = System.Web.UI.Util.GetNonNegativeIntegerAttribute(name, value);
                        Encoding.GetEncoding(this._codePage);
                        return false;
                    }
                    return false;

                case "transaction":
                    base.OnFoundAttributeRequiringCompilation(name);
                    this.ParseTransactionAttribute(name, value);
                    goto Label_05CF;

                case "aspcompat":
                    base.OnFoundAttributeRequiringCompilation(name);
                    this.flags[0x40] = System.Web.UI.Util.GetBooleanAttribute(name, value);
                    if (this.flags[0x40] && !HttpRuntime.HasUnmanagedPermission())
                    {
                        throw new HttpException(System.Web.SR.GetString("Insufficient_trust_for_attribute", new object[] { "AspCompat" }));
                    }
                    goto Label_05CF;

                case "async":
                    base.OnFoundAttributeRequiringCompilation(name);
                    this.flags[0x800000] = System.Web.UI.Util.GetBooleanAttribute(name, value);
                    if (!HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                    {
                        throw new HttpException(System.Web.SR.GetString("Insufficient_trust_for_attribute", new object[] { "async" }));
                    }
                    goto Label_05CF;

                case "tracemode":
                {
                    object obj2 = System.Web.UI.Util.GetEnumAttribute(name, value, typeof(TraceModeInternal));
                    this._traceMode = (System.Web.TraceMode) obj2;
                    goto Label_05CF;
                }
                case "trace":
                    if (!System.Web.UI.Util.GetBooleanAttribute(name, value))
                    {
                        this._traceEnabled = TraceEnable.Disable;
                    }
                    else
                    {
                        this._traceEnabled = TraceEnable.Enable;
                    }
                    goto Label_05CF;

                case "smartnavigation":
                    base.ValidateBuiltInAttribute(deviceName, name, value);
                    return !System.Web.UI.Util.GetBooleanAttribute(name, value);

                case "maintainscrollpositiononpostback":
                    return !System.Web.UI.Util.GetBooleanAttribute(name, value);

                case "validaterequest":
                    this.flags[0x400000] = System.Web.UI.Util.GetBooleanAttribute(name, value);
                    goto Label_05CF;

                case "clienttarget":
                    if (!base.IsExpressionBuilderValue(value))
                    {
                        HttpCapabilitiesDefaultProvider.GetUserAgentFromClientTarget(base.CurrentVirtualPath, value);
                        return false;
                    }
                    return false;

                case "masterpagefile":
                    if (!base.IsExpressionBuilderValue(value))
                    {
                        if (value.Length > 0)
                        {
                            Type referencedType = base.GetReferencedType(value);
                            if (!typeof(MasterPage).IsAssignableFrom(referencedType))
                            {
                                base.ProcessError(System.Web.SR.GetString("Invalid_master_base", new object[] { value }));
                            }
                            if (deviceName.Length > 0)
                            {
                                this.EnsureMasterPageFileFromConfigApplied();
                            }
                        }
                        this._mainDirectiveMasterPageSet = true;
                        return false;
                    }
                    return false;

                default:
                    return base.ProcessMainDirectiveAttribute(deviceName, name, value, parseData);
            }
            if (this.flags[0x100000])
            {
                base.OnFoundAttributeRequiringCompilation(name);
            }
        Label_05CF:
            base.ValidateBuiltInAttribute(deviceName, name, value);
            return true;
        }

        internal override void ProcessOutputCacheDirective(string directiveName, IDictionary directive)
        {
            bool val = false;
            string andRemoveNonEmptyAttribute = System.Web.UI.Util.GetAndRemoveNonEmptyAttribute(directive, "varybycontentencoding");
            if (andRemoveNonEmptyAttribute != null)
            {
                base.OutputCacheParameters.VaryByContentEncoding = andRemoveNonEmptyAttribute;
            }
            string str2 = System.Web.UI.Util.GetAndRemoveNonEmptyAttribute(directive, "varybyheader");
            if (str2 != null)
            {
                base.OutputCacheParameters.VaryByHeader = str2;
            }
            object obj2 = System.Web.UI.Util.GetAndRemoveEnumAttribute(directive, typeof(OutputCacheLocation), "location");
            if (obj2 != null)
            {
                this._outputCacheLocation = (OutputCacheLocation) obj2;
                base.OutputCacheParameters.Location = this._outputCacheLocation;
            }
            string depString = System.Web.UI.Util.GetAndRemoveNonEmptyAttribute(directive, "sqldependency");
            if (depString != null)
            {
                base.OutputCacheParameters.SqlDependency = depString;
                SqlCacheDependency.ValidateOutputCacheDependencyString(depString, true);
            }
            if (System.Web.UI.Util.GetAndRemoveBooleanAttribute(directive, "nostore", ref val))
            {
                base.OutputCacheParameters.NoStore = val;
            }
            base.ProcessOutputCacheDirective(directiveName, directive);
        }

        internal override void ProcessUnknownMainDirectiveAttribute(string filter, string attribName, string value)
        {
            if (attribName == "asynctimeout")
            {
                int nonNegativeIntegerAttribute = System.Web.UI.Util.GetNonNegativeIntegerAttribute(attribName, value);
                value = new TimeSpan(0, 0, nonNegativeIntegerAttribute).ToString();
            }
            base.ProcessUnknownMainDirectiveAttribute(filter, attribName, value);
        }

        internal bool AspCompatMode
        {
            get
            {
                return this.flags[0x40];
            }
        }

        internal bool AsyncMode
        {
            get
            {
                return this.flags[0x800000];
            }
        }

        public static Type DefaultApplicationBaseType
        {
            get
            {
                return s_defaultApplicationBaseType;
            }
            set
            {
                if ((value != null) && !typeof(HttpApplication).IsAssignableFrom(value))
                {
                    throw ExceptionUtil.PropertyInvalid("DefaultApplicationBaseType");
                }
                BuildManager.ThrowIfPreAppStartNotRunning();
                s_defaultApplicationBaseType = value;
            }
        }

        internal override Type DefaultBaseType
        {
            get
            {
                return typeof(Page);
            }
        }

        internal override string DefaultDirectiveName
        {
            get
            {
                return "page";
            }
        }

        internal override Type DefaultFileLevelBuilderType
        {
            get
            {
                return typeof(FileLevelPageControlBuilder);
            }
        }

        public static Type DefaultPageBaseType
        {
            get
            {
                return s_defaultPageBaseType;
            }
            set
            {
                if ((value != null) && !typeof(Page).IsAssignableFrom(value))
                {
                    throw ExceptionUtil.PropertyInvalid("DefaultPageBaseType");
                }
                BuildManager.ThrowIfPreAppStartNotRunning();
                s_defaultPageBaseType = value;
            }
        }

        public static Type DefaultPageParserFilterType
        {
            get
            {
                return s_defaultPageParserFilterType;
            }
            set
            {
                if ((value != null) && !typeof(PageParserFilter).IsAssignableFrom(value))
                {
                    throw ExceptionUtil.PropertyInvalid("DefaultPageParserFilterType");
                }
                BuildManager.ThrowIfPreAppStartNotRunning();
                s_defaultPageParserFilterType = value;
            }
        }

        public static Type DefaultUserControlBaseType
        {
            get
            {
                return s_defaultUserContorlBaseType;
            }
            set
            {
                if ((value != null) && !typeof(UserControl).IsAssignableFrom(value))
                {
                    throw ExceptionUtil.PropertyInvalid("DefaultUserControlBaseType");
                }
                BuildManager.ThrowIfPreAppStartNotRunning();
                s_defaultUserContorlBaseType = value;
            }
        }

        public static bool EnableLongStringsAsResources
        {
            get
            {
                return s_enableLongStringsAsResources;
            }
            set
            {
                BuildManager.ThrowIfPreAppStartNotRunning();
                s_enableLongStringsAsResources = value;
            }
        }

        internal override bool FDurationRequiredOnOutputCache
        {
            get
            {
                return (this._outputCacheLocation != OutputCacheLocation.None);
            }
        }

        internal bool FReadOnlySessionState
        {
            get
            {
                return this.flags[0x200000];
            }
        }

        internal bool FRequiresSessionState
        {
            get
            {
                return this.flags[0x100000];
            }
        }

        internal override bool FVaryByParamsRequiredOnOutputCache
        {
            get
            {
                return (this._outputCacheLocation != OutputCacheLocation.None);
            }
        }

        internal Type MasterPageType
        {
            get
            {
                return this._masterPageType;
            }
        }

        internal Type PreviousPageType
        {
            get
            {
                return this._previousPageType;
            }
        }

        internal string StyleSheetTheme
        {
            get
            {
                return this._styleSheetTheme;
            }
        }

        internal TraceEnable TraceEnabled
        {
            get
            {
                return this._traceEnabled;
            }
        }

        internal System.Web.TraceMode TraceMode
        {
            get
            {
                return this._traceMode;
            }
        }

        internal int TransactionMode
        {
            get
            {
                return this._transactionMode;
            }
        }

        internal override string UnknownOutputCacheAttributeError
        {
            get
            {
                return "Attr_not_supported_in_pagedirective";
            }
        }

        internal bool ValidateRequest
        {
            get
            {
                return this.flags[0x400000];
            }
        }

        private enum TraceModeInternal
        {
            SortByTime,
            SortByCategory
        }
    }
}

