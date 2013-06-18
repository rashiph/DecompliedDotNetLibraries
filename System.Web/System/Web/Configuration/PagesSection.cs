namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;

    public sealed class PagesSection : ConfigurationSection
    {
        private System.Web.UI.ClientIDMode? _clientIDMode;
        private Version _controlRenderingCompatibilityVersion;
        private static readonly Version _controlRenderingDefaultVersion = VersionUtil.Framework40;
        private static readonly Version _controlRenderingMinimumVersion = VersionUtil.Framework35;
        private string _masterPageFile;
        private Type _pageBaseType;
        private Type _pageParserFilterType;
        private static readonly ConfigurationProperty _propAsyncTimeout = new ConfigurationProperty("asyncTimeout", typeof(TimeSpan), TimeSpan.FromSeconds((double) Page.DefaultAsyncTimeoutSeconds), StdValidatorsAndConverters.TimeSpanSecondsConverter, StdValidatorsAndConverters.PositiveTimeSpanValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propAutoEventWireup = new ConfigurationProperty("autoEventWireup", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propBuffer = new ConfigurationProperty("buffer", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propClientIDMode = new ConfigurationProperty("clientIDMode", typeof(System.Web.UI.ClientIDMode), System.Web.UI.ClientIDMode.Predictable, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCompilationMode = new ConfigurationProperty("compilationMode", typeof(System.Web.UI.CompilationMode), System.Web.UI.CompilationMode.Always, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propControlRenderingCompatibilityVersion = new ConfigurationProperty("controlRenderingCompatibilityVersion", typeof(Version), _controlRenderingDefaultVersion, StdValidatorsAndConverters.VersionConverter, new VersionValidator(_controlRenderingMinimumVersion), ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propControls = new ConfigurationProperty("controls", typeof(TagPrefixCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propEnableEventValidation = new ConfigurationProperty("enableEventValidation", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableSessionState = new ConfigurationProperty("enableSessionState", typeof(string), "true", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableViewState = new ConfigurationProperty("enableViewState", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableViewStateMac = new ConfigurationProperty("enableViewStateMac", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propIgnoreDeviceFilters = new ConfigurationProperty("ignoreDeviceFilters", typeof(IgnoreDeviceFilterElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propMaintainScrollPosition = new ConfigurationProperty("maintainScrollPositionOnPostBack", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMasterPageFile = new ConfigurationProperty("masterPageFile", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxPageStateFieldLength = new ConfigurationProperty("maxPageStateFieldLength", typeof(int), Page.DefaultMaxPageStateFieldLength, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propNamespaces = new ConfigurationProperty("namespaces", typeof(NamespaceCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propPageBaseType = new ConfigurationProperty("pageBaseType", typeof(string), "System.Web.UI.Page", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPageParserFilterType = new ConfigurationProperty("pageParserFilterType", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRenderAllHiddenFieldsAtTopOfForm = new ConfigurationProperty("renderAllHiddenFieldsAtTopOfForm", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propSmartNavigation = new ConfigurationProperty("smartNavigation", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propStyleSheetTheme = new ConfigurationProperty("styleSheetTheme", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propTagMapping = new ConfigurationProperty("tagMapping", typeof(TagMapCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propTheme = new ConfigurationProperty("theme", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUserControlBaseType = new ConfigurationProperty("userControlBaseType", typeof(string), "System.Web.UI.UserControl", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propValidateRequest = new ConfigurationProperty("validateRequest", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propViewStateEncryptionMode = new ConfigurationProperty("viewStateEncryptionMode", typeof(System.Web.UI.ViewStateEncryptionMode), System.Web.UI.ViewStateEncryptionMode.Auto, ConfigurationPropertyOptions.None);
        private bool _styleSheetThemeChecked;
        private TagNamespaceRegisterEntryTable _tagNamespaceRegisterEntries;
        private bool _themeChecked;
        private Type _userControlBaseType;
        private Hashtable _userControlRegisterEntries;
        private VirtualPath _virtualPath;

        static PagesSection()
        {
            _properties.Add(_propBuffer);
            _properties.Add(_propControlRenderingCompatibilityVersion);
            _properties.Add(_propEnableSessionState);
            _properties.Add(_propEnableViewState);
            _properties.Add(_propEnableViewStateMac);
            _properties.Add(_propEnableEventValidation);
            _properties.Add(_propSmartNavigation);
            _properties.Add(_propAutoEventWireup);
            _properties.Add(_propPageBaseType);
            _properties.Add(_propUserControlBaseType);
            _properties.Add(_propValidateRequest);
            _properties.Add(_propMasterPageFile);
            _properties.Add(_propTheme);
            _properties.Add(_propStyleSheetTheme);
            _properties.Add(_propNamespaces);
            _properties.Add(_propControls);
            _properties.Add(_propTagMapping);
            _properties.Add(_propMaxPageStateFieldLength);
            _properties.Add(_propCompilationMode);
            _properties.Add(_propPageParserFilterType);
            _properties.Add(_propViewStateEncryptionMode);
            _properties.Add(_propMaintainScrollPosition);
            _properties.Add(_propAsyncTimeout);
            _properties.Add(_propRenderAllHiddenFieldsAtTopOfForm);
            _properties.Add(_propClientIDMode);
            _properties.Add(_propIgnoreDeviceFilters);
        }

        internal PageParserFilter CreateControlTypeFilter()
        {
            Type pageParserFilterTypeInternal = this.PageParserFilterTypeInternal;
            if (pageParserFilterTypeInternal == null)
            {
                return null;
            }
            return (PageParserFilter) HttpRuntime.CreateNonPublicInstance(pageParserFilterTypeInternal);
        }

        protected override void DeserializeSection(XmlReader reader)
        {
            base.DeserializeSection(reader);
            WebContext hostingContext = base.EvaluationContext.HostingContext as WebContext;
            if (hostingContext != null)
            {
                this._virtualPath = VirtualPath.CreateNonRelativeTrailingSlashAllowNull(hostingContext.Path);
            }
        }

        internal void FillInRegisterEntries()
        {
            TagNamespaceRegisterEntryTable table = new TagNamespaceRegisterEntryTable();
            foreach (TagNamespaceRegisterEntry entry in DefaultTagNamespaceRegisterEntries)
            {
                table[entry.TagPrefix] = new ArrayList(new object[] { entry });
            }
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (TagPrefixInfo info in this.Controls)
            {
                if (!string.IsNullOrEmpty(info.TagName))
                {
                    UserControlRegisterEntry entry2 = new UserControlRegisterEntry(info.TagPrefix, info.TagName) {
                        ComesFromConfig = true
                    };
                    try
                    {
                        entry2.UserControlSource = VirtualPath.CreateNonRelative(info.Source);
                    }
                    catch (Exception exception)
                    {
                        throw new ConfigurationErrorsException(exception.Message, exception, info.ElementInformation.Properties["src"].Source, info.ElementInformation.Properties["src"].LineNumber);
                    }
                    hashtable[entry2.Key] = entry2;
                }
                else if (!string.IsNullOrEmpty(info.Namespace))
                {
                    TagNamespaceRegisterEntry entry3 = new TagNamespaceRegisterEntry(info.TagPrefix, info.Namespace, info.Assembly);
                    ArrayList list = null;
                    list = (ArrayList) table[info.TagPrefix];
                    if (list == null)
                    {
                        list = new ArrayList();
                        table[info.TagPrefix] = list;
                    }
                    list.Add(entry3);
                }
            }
            this._tagNamespaceRegisterEntries = table;
            this._userControlRegisterEntries = hashtable;
        }

        [ConfigurationProperty("asyncTimeout", DefaultValue="00:00:45"), TypeConverter(typeof(TimeSpanSecondsConverter)), TimeSpanValidator(MinValueString="00:00:00", MaxValueString="10675199.02:48:05.4775807")]
        public TimeSpan AsyncTimeout
        {
            get
            {
                return (TimeSpan) base[_propAsyncTimeout];
            }
            set
            {
                base[_propAsyncTimeout] = value;
            }
        }

        [ConfigurationProperty("autoEventWireup", DefaultValue=true)]
        public bool AutoEventWireup
        {
            get
            {
                return (bool) base[_propAutoEventWireup];
            }
            set
            {
                base[_propAutoEventWireup] = value;
            }
        }

        [ConfigurationProperty("buffer", DefaultValue=true)]
        public bool Buffer
        {
            get
            {
                return (bool) base[_propBuffer];
            }
            set
            {
                base[_propBuffer] = value;
            }
        }

        [ConfigurationProperty("clientIDMode", DefaultValue=2)]
        public System.Web.UI.ClientIDMode ClientIDMode
        {
            get
            {
                if (!this._clientIDMode.HasValue)
                {
                    this._clientIDMode = new System.Web.UI.ClientIDMode?((System.Web.UI.ClientIDMode) base[_propClientIDMode]);
                }
                return this._clientIDMode.Value;
            }
            set
            {
                base[_propClientIDMode] = value;
                this._clientIDMode = new System.Web.UI.ClientIDMode?(value);
            }
        }

        [ConfigurationProperty("compilationMode", DefaultValue=2)]
        public System.Web.UI.CompilationMode CompilationMode
        {
            get
            {
                return (System.Web.UI.CompilationMode) base[_propCompilationMode];
            }
            set
            {
                base[_propCompilationMode] = value;
            }
        }

        [ConfigurationProperty("controlRenderingCompatibilityVersion", DefaultValue="4.0"), TypeConverter(typeof(VersionConverter)), ConfigurationValidator(typeof(VersionValidator))]
        public Version ControlRenderingCompatibilityVersion
        {
            get
            {
                if (this._controlRenderingCompatibilityVersion == null)
                {
                    this._controlRenderingCompatibilityVersion = (Version) base[_propControlRenderingCompatibilityVersion];
                }
                return this._controlRenderingCompatibilityVersion;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base[_propControlRenderingCompatibilityVersion] = value;
                this._controlRenderingCompatibilityVersion = value;
            }
        }

        [ConfigurationProperty("controls")]
        public TagPrefixCollection Controls
        {
            get
            {
                return (TagPrefixCollection) base[_propControls];
            }
        }

        internal static ICollection DefaultTagNamespaceRegisterEntries
        {
            get
            {
                TagNamespaceRegisterEntry entry = new TagNamespaceRegisterEntry("asp", "System.Web.UI.WebControls", "System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                TagNamespaceRegisterEntry entry2 = new TagNamespaceRegisterEntry("mobile", "System.Web.UI.MobileControls", "System.Web.Mobile, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                return new TagNamespaceRegisterEntry[] { entry, entry2 };
            }
        }

        [ConfigurationProperty("enableEventValidation", DefaultValue=true)]
        public bool EnableEventValidation
        {
            get
            {
                return (bool) base[_propEnableEventValidation];
            }
            set
            {
                base[_propEnableEventValidation] = value;
            }
        }

        [ConfigurationProperty("enableSessionState", DefaultValue="true")]
        public PagesEnableSessionState EnableSessionState
        {
            get
            {
                switch (((string) base[_propEnableSessionState]))
                {
                    case "true":
                        return PagesEnableSessionState.True;

                    case "false":
                        return PagesEnableSessionState.False;

                    case "ReadOnly":
                        return PagesEnableSessionState.ReadOnly;
                }
                string name = _propEnableSessionState.Name;
                string str2 = "true, false, ReadOnly";
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_enum_attribute", new object[] { name, str2 }));
            }
            set
            {
                string str = "true";
                switch (value)
                {
                    case PagesEnableSessionState.False:
                        str = "false";
                        break;

                    case PagesEnableSessionState.ReadOnly:
                        str = "ReadOnly";
                        break;

                    case PagesEnableSessionState.True:
                        str = "true";
                        break;

                    default:
                        str = "true";
                        break;
                }
                base[_propEnableSessionState] = str;
            }
        }

        [ConfigurationProperty("enableViewState", DefaultValue=true)]
        public bool EnableViewState
        {
            get
            {
                return (bool) base[_propEnableViewState];
            }
            set
            {
                base[_propEnableViewState] = value;
            }
        }

        [ConfigurationProperty("enableViewStateMac", DefaultValue=true)]
        public bool EnableViewStateMac
        {
            get
            {
                return (bool) base[_propEnableViewStateMac];
            }
            set
            {
                base[_propEnableViewStateMac] = value;
            }
        }

        [ConfigurationProperty("ignoreDeviceFilters")]
        public IgnoreDeviceFilterElementCollection IgnoreDeviceFilters
        {
            get
            {
                return (IgnoreDeviceFilterElementCollection) base[_propIgnoreDeviceFilters];
            }
        }

        [ConfigurationProperty("maintainScrollPositionOnPostBack", DefaultValue=false)]
        public bool MaintainScrollPositionOnPostBack
        {
            get
            {
                return (bool) base[_propMaintainScrollPosition];
            }
            set
            {
                base[_propMaintainScrollPosition] = value;
            }
        }

        [ConfigurationProperty("masterPageFile", DefaultValue="")]
        public string MasterPageFile
        {
            get
            {
                return (string) base[_propMasterPageFile];
            }
            set
            {
                base[_propMasterPageFile] = value;
            }
        }

        internal string MasterPageFileInternal
        {
            get
            {
                if (this._masterPageFile == null)
                {
                    string masterPageFile = this.MasterPageFile;
                    if (!string.IsNullOrEmpty(masterPageFile))
                    {
                        VirtualPath path;
                        if (System.Web.Util.UrlPath.IsAbsolutePhysicalPath(masterPageFile))
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("Physical_path_not_allowed", new object[] { masterPageFile }), base.ElementInformation.Properties["masterPageFile"].Source, base.ElementInformation.Properties["masterPageFile"].LineNumber);
                        }
                        try
                        {
                            path = VirtualPath.CreateNonRelative(masterPageFile);
                        }
                        catch (Exception exception)
                        {
                            throw new ConfigurationErrorsException(exception.Message, exception, base.ElementInformation.Properties["masterPageFile"].Source, base.ElementInformation.Properties["masterPageFile"].LineNumber);
                        }
                        if (!Util.VirtualFileExistsWithAssert(path))
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("FileName_does_not_exist", new object[] { masterPageFile }), base.ElementInformation.Properties["masterPageFile"].Source, base.ElementInformation.Properties["masterPageFile"].LineNumber);
                        }
                        string extension = System.Web.Util.UrlPath.GetExtension(masterPageFile);
                        Type c = CompilationUtil.GetBuildProviderTypeFromExtension(this._virtualPath, extension, BuildProviderAppliesTo.Web, false);
                        if (!typeof(MasterPageBuildProvider).IsAssignableFrom(c))
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("Bad_masterPage_ext"), base.ElementInformation.Properties["masterPageFile"].Source, base.ElementInformation.Properties["masterPageFile"].LineNumber);
                        }
                        masterPageFile = path.AppRelativeVirtualPathString;
                    }
                    else
                    {
                        masterPageFile = string.Empty;
                    }
                    this._masterPageFile = masterPageFile;
                }
                return this._masterPageFile;
            }
        }

        [ConfigurationProperty("maxPageStateFieldLength", DefaultValue=-1)]
        public int MaxPageStateFieldLength
        {
            get
            {
                return (int) base[_propMaxPageStateFieldLength];
            }
            set
            {
                base[_propMaxPageStateFieldLength] = value;
            }
        }

        [ConfigurationProperty("namespaces")]
        public NamespaceCollection Namespaces
        {
            get
            {
                return (NamespaceCollection) base[_propNamespaces];
            }
        }

        [ConfigurationProperty("pageBaseType", DefaultValue="System.Web.UI.Page")]
        public string PageBaseType
        {
            get
            {
                return (string) base[_propPageBaseType];
            }
            set
            {
                base[_propPageBaseType] = value;
            }
        }

        internal Type PageBaseTypeInternal
        {
            get
            {
                if ((this._pageBaseType == null) && (base.ElementInformation.Properties[_propPageBaseType.Name].ValueOrigin != PropertyValueOrigin.Default))
                {
                    lock (this)
                    {
                        if (this._pageBaseType == null)
                        {
                            Type userBaseType = ConfigUtil.GetType(this.PageBaseType, "pageBaseType", this);
                            ConfigUtil.CheckBaseType(typeof(Page), userBaseType, "pageBaseType", this);
                            this._pageBaseType = userBaseType;
                        }
                    }
                }
                return this._pageBaseType;
            }
        }

        [ConfigurationProperty("pageParserFilterType", DefaultValue="")]
        public string PageParserFilterType
        {
            get
            {
                return (string) base[_propPageParserFilterType];
            }
            set
            {
                base[_propPageParserFilterType] = value;
            }
        }

        internal Type PageParserFilterTypeInternal
        {
            get
            {
                if (PageParser.DefaultPageParserFilterType != null)
                {
                    return PageParser.DefaultPageParserFilterType;
                }
                if ((this._pageParserFilterType == null) && !string.IsNullOrEmpty(this.PageParserFilterType))
                {
                    Type userBaseType = ConfigUtil.GetType(this.PageParserFilterType, "pageParserFilterType", this);
                    ConfigUtil.CheckBaseType(typeof(PageParserFilter), userBaseType, "pageParserFilterType", this);
                    this._pageParserFilterType = userBaseType;
                }
                return this._pageParserFilterType;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("renderAllHiddenFieldsAtTopOfForm", DefaultValue=true)]
        public bool RenderAllHiddenFieldsAtTopOfForm
        {
            get
            {
                return (bool) base[_propRenderAllHiddenFieldsAtTopOfForm];
            }
            set
            {
                base[_propRenderAllHiddenFieldsAtTopOfForm] = value;
            }
        }

        [ConfigurationProperty("smartNavigation", DefaultValue=false)]
        public bool SmartNavigation
        {
            get
            {
                return (bool) base[_propSmartNavigation];
            }
            set
            {
                base[_propSmartNavigation] = value;
            }
        }

        [ConfigurationProperty("styleSheetTheme", DefaultValue="")]
        public string StyleSheetTheme
        {
            get
            {
                return (string) base[_propStyleSheetTheme];
            }
            set
            {
                base[_propStyleSheetTheme] = value;
            }
        }

        internal string StyleSheetThemeInternal
        {
            get
            {
                string styleSheetTheme = this.StyleSheetTheme;
                if (!this._styleSheetThemeChecked)
                {
                    if (!string.IsNullOrEmpty(styleSheetTheme) && !Util.ThemeExists(styleSheetTheme))
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Page_theme_not_found", new object[] { styleSheetTheme }), base.ElementInformation.Properties["styleSheetTheme"].Source, base.ElementInformation.Properties["styleSheetTheme"].LineNumber);
                    }
                    this._styleSheetThemeChecked = true;
                }
                return styleSheetTheme;
            }
        }

        [ConfigurationProperty("tagMapping")]
        public TagMapCollection TagMapping
        {
            get
            {
                return (TagMapCollection) base[_propTagMapping];
            }
        }

        internal TagNamespaceRegisterEntryTable TagNamespaceRegisterEntriesInternal
        {
            get
            {
                if (this._tagNamespaceRegisterEntries == null)
                {
                    lock (this)
                    {
                        if (this._tagNamespaceRegisterEntries == null)
                        {
                            this.FillInRegisterEntries();
                        }
                    }
                }
                return this._tagNamespaceRegisterEntries;
            }
        }

        [ConfigurationProperty("theme", DefaultValue="")]
        public string Theme
        {
            get
            {
                return (string) base[_propTheme];
            }
            set
            {
                base[_propTheme] = value;
            }
        }

        internal string ThemeInternal
        {
            get
            {
                string theme = this.Theme;
                if (!this._themeChecked)
                {
                    if (!string.IsNullOrEmpty(theme) && !Util.ThemeExists(theme))
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Page_theme_not_found", new object[] { theme }), base.ElementInformation.Properties["theme"].Source, base.ElementInformation.Properties["theme"].LineNumber);
                    }
                    this._themeChecked = true;
                }
                return theme;
            }
        }

        [ConfigurationProperty("userControlBaseType", DefaultValue="System.Web.UI.UserControl")]
        public string UserControlBaseType
        {
            get
            {
                return (string) base[_propUserControlBaseType];
            }
            set
            {
                base[_propUserControlBaseType] = value;
            }
        }

        internal Type UserControlBaseTypeInternal
        {
            get
            {
                if ((this._userControlBaseType == null) && (base.ElementInformation.Properties[_propUserControlBaseType.Name].ValueOrigin != PropertyValueOrigin.Default))
                {
                    lock (this)
                    {
                        if (this._userControlBaseType == null)
                        {
                            Type userBaseType = ConfigUtil.GetType(this.UserControlBaseType, "userControlBaseType", this);
                            ConfigUtil.CheckBaseType(typeof(UserControl), userBaseType, "userControlBaseType", this);
                            this._userControlBaseType = userBaseType;
                        }
                    }
                }
                return this._userControlBaseType;
            }
        }

        internal Hashtable UserControlRegisterEntriesInternal
        {
            get
            {
                if (this._userControlRegisterEntries == null)
                {
                    lock (this)
                    {
                        if (this._userControlRegisterEntries == null)
                        {
                            this.FillInRegisterEntries();
                        }
                    }
                }
                return this._userControlRegisterEntries;
            }
        }

        [ConfigurationProperty("validateRequest", DefaultValue=true)]
        public bool ValidateRequest
        {
            get
            {
                return (bool) base[_propValidateRequest];
            }
            set
            {
                base[_propValidateRequest] = value;
            }
        }

        [ConfigurationProperty("viewStateEncryptionMode", DefaultValue=0)]
        public System.Web.UI.ViewStateEncryptionMode ViewStateEncryptionMode
        {
            get
            {
                return (System.Web.UI.ViewStateEncryptionMode) base[_propViewStateEncryptionMode];
            }
            set
            {
                base[_propViewStateEncryptionMode] = value;
            }
        }
    }
}

