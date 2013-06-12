namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.UI.Adapters;
    using System.Web.Util;

    public class HttpCapabilitiesBase : IFilterResolutionService
    {
        private volatile bool _activexcontrols;
        private IDictionary _adapters;
        private Hashtable _adapterTypes;
        private volatile bool _aol;
        private volatile bool _backgroundsounds;
        private volatile bool _beta;
        private volatile string _browser;
        private static HttpCapabilitiesProvider _browserCapabilitiesProvider = null;
        private ArrayList _browsers;
        private volatile bool _canCombineFormsInDeck;
        private volatile bool _canInitiateVoiceCall;
        private volatile bool _canRenderAfterInputOrSelectElement;
        private volatile bool _canRenderEmptySelects;
        private volatile bool _canRenderInputAndSelectElementsTogether;
        private volatile bool _canRenderMixedSelects;
        private volatile bool _canRenderOneventAndPrevElementsTogether;
        private volatile bool _canRenderPostBackCards;
        private volatile bool _canRenderSetvarZeroWithMultiSelectionList;
        private volatile bool _canSendMail;
        private volatile bool _cdf;
        private static FactoryGenerator _controlAdapterFactoryGenerator;
        private static Hashtable _controlAdapterFactoryTable;
        private volatile bool _cookies;
        private volatile bool _crawler;
        private volatile int _defaultSubmitButtonLimit;
        private volatile System.Version _ecmascriptversion;
        private static HttpCapabilitiesBase _emptyHttpCapabilitiesBase;
        private static object _emptyHttpCapabilitiesBaseLock = new object();
        private volatile bool _frames;
        private volatile int _gatewayMajorVersion;
        private double _gatewayMinorVersion;
        private volatile string _gatewayVersion;
        private volatile bool _hasBackButton;
        private volatile bool _haveactivexcontrols;
        private volatile bool _haveaol;
        private volatile bool _havebackgroundsounds;
        private volatile bool _havebeta;
        private volatile bool _havebrowser;
        private volatile bool _haveCanCombineFormsInDeck;
        private volatile bool _haveCanInitiateVoiceCall;
        private volatile bool _haveCanRenderAfterInputOrSelectElement;
        private volatile bool _haveCanRenderEmptySelects;
        private volatile bool _haveCanRenderInputAndSelectElementsTogether;
        private volatile bool _haveCanRenderMixedSelects;
        private volatile bool _haveCanRenderOneventAndPrevElementsTogether;
        private volatile bool _haveCanRenderPostBackCards;
        private volatile bool _haveCanRenderSetvarZeroWithMultiSelectionList;
        private volatile bool _haveCanSendMail;
        private volatile bool _havecdf;
        private volatile bool _havecookies;
        private volatile bool _havecrawler;
        private volatile bool _haveDefaultSubmitButtonLimit;
        private volatile bool _haveecmascriptversion;
        private volatile bool _haveframes;
        private volatile bool _haveGatewayMajorVersion;
        private volatile bool _haveGatewayMinorVersion;
        private volatile bool _haveGatewayVersion;
        private volatile bool _haveHasBackButton;
        private volatile bool _haveHidesRightAlignedMultiselectScrollbars;
        private volatile bool _haveInputType;
        private volatile bool _haveIsColor;
        private volatile bool _haveIsMobileDevice;
        private volatile bool _havejavaapplets;
        private volatile bool _havejavascript;
        private volatile bool _havejscriptversion;
        private volatile bool _havemajorversion;
        private volatile bool _haveMaximumHrefLength;
        private volatile bool _haveMaximumRenderedPageSize;
        private volatile bool _haveMaximumSoftkeyLabelLength;
        private volatile bool _haveminorversion;
        private volatile bool _haveMobileDeviceManufacturer;
        private volatile bool _haveMobileDeviceModel;
        private volatile bool _havemsdomversion;
        private volatile bool _haveNumberOfSoftkeys;
        private volatile bool _haveplatform;
        private volatile bool _havePreferredImageMime;
        private volatile bool _havePreferredRenderingMime;
        private volatile bool _havePreferredRenderingType;
        private volatile bool _havePreferredRequestEncoding;
        private volatile bool _havePreferredResponseEncoding;
        private volatile bool _haveRendersBreakBeforeWmlSelectAndInput;
        private volatile bool _haveRendersBreaksAfterHtmlLists;
        private volatile bool _haveRendersBreaksAfterWmlAnchor;
        private volatile bool _haveRendersBreaksAfterWmlInput;
        private volatile bool _haveRendersWmlDoAcceptsInline;
        private volatile bool _haveRendersWmlSelectsAsMenuCards;
        private volatile bool _haveRequiredMetaTagNameValue;
        private volatile bool _haveRequiresAttributeColonSubstitution;
        private volatile bool _haveRequiresContentTypeMetaTag;
        private volatile bool _haverequiresControlStateInSession;
        private volatile bool _haveRequiresDBCSCharacter;
        private volatile bool _haveRequiresHtmlAdaptiveErrorReporting;
        private volatile bool _haveRequiresLeadingPageBreak;
        private volatile bool _haveRequiresNoBreakInFormatting;
        private volatile bool _haveRequiresOutputOptimization;
        private volatile bool _haveRequiresPhoneNumbersAsPlainText;
        private volatile bool _haveRequiresSpecialViewStateEncoding;
        private volatile bool _haveRequiresUniqueFilePathSuffix;
        private volatile bool _haveRequiresUniqueHtmlCheckboxNames;
        private volatile bool _haveRequiresUniqueHtmlInputNames;
        private volatile bool _haveRequiresUrlEncodedPostfieldValues;
        private volatile bool _haveScreenBitDepth;
        private volatile bool _haveScreenCharactersHeight;
        private volatile bool _haveScreenCharactersWidth;
        private volatile bool _haveScreenPixelsHeight;
        private volatile bool _haveScreenPixelsWidth;
        private volatile bool _haveSupportsAccesskeyAttribute;
        private volatile bool _haveSupportsBodyColor;
        private volatile bool _haveSupportsBold;
        private volatile bool _haveSupportsCacheControlMetaTag;
        private volatile bool _haveSupportsCallback;
        private volatile bool _haveSupportsCss;
        private volatile bool _haveSupportsDivAlign;
        private volatile bool _haveSupportsDivNoWrap;
        private volatile bool _haveSupportsEmptyStringInCookieValue;
        private volatile bool _haveSupportsFontColor;
        private volatile bool _haveSupportsFontName;
        private volatile bool _haveSupportsFontSize;
        private volatile bool _haveSupportsImageSubmit;
        private volatile bool _haveSupportsIModeSymbols;
        private volatile bool _haveSupportsInputIStyle;
        private volatile bool _haveSupportsInputMode;
        private volatile bool _haveSupportsItalic;
        private volatile bool _haveSupportsJPhoneMultiMediaAttributes;
        private volatile bool _haveSupportsJPhoneSymbols;
        private volatile bool _haveSupportsMaintainScrollPositionOnPostback;
        private volatile bool _haveSupportsQueryStringInFormAction;
        private volatile bool _haveSupportsRedirectWithCookie;
        private volatile bool _haveSupportsSelectMultiple;
        private volatile bool _haveSupportsUncheck;
        private volatile bool _haveSupportsXmlHttp;
        private volatile bool _havetables;
        private volatile bool _havetagwriter;
        private volatile bool _havetype;
        private volatile bool _havevbscript;
        private volatile bool _haveversion;
        private volatile bool _havew3cdomversion;
        private volatile bool _havewin16;
        private volatile bool _havewin32;
        private volatile bool _hidesRightAlignedMultiselectScrollbars;
        private string _htmlTextWriter;
        private volatile string _inputType;
        private volatile bool _isColor;
        private volatile bool _isMobileDevice;
        private IDictionary _items;
        private volatile bool _javaapplets;
        private volatile bool _javascript;
        private volatile System.Version _jscriptversion;
        private volatile int _majorversion;
        private volatile int _maximumHrefLength;
        private volatile int _maximumRenderedPageSize;
        private volatile int _maximumSoftkeyLabelLength;
        private double _minorversion;
        private volatile string _mobileDeviceManufacturer;
        private volatile string _mobileDeviceModel;
        private volatile System.Version _msdomversion;
        private volatile int _numberOfSoftkeys;
        private volatile string _platform;
        private volatile string _preferredImageMime;
        private volatile string _preferredRenderingMime;
        private volatile string _preferredRenderingType;
        private volatile string _preferredRequestEncoding;
        private volatile string _preferredResponseEncoding;
        private volatile bool _rendersBreakBeforeWmlSelectAndInput;
        private volatile bool _rendersBreaksAfterHtmlLists;
        private volatile bool _rendersBreaksAfterWmlAnchor;
        private volatile bool _rendersBreaksAfterWmlInput;
        private volatile bool _rendersWmlDoAcceptsInline;
        private volatile bool _rendersWmlSelectsAsMenuCards;
        private volatile string _requiredMetaTagNameValue;
        private volatile bool _requiresAttributeColonSubstitution;
        private volatile bool _requiresContentTypeMetaTag;
        private volatile bool _requiresControlStateInSession;
        private volatile bool _requiresDBCSCharacter;
        private volatile bool _requiresHtmlAdaptiveErrorReporting;
        private volatile bool _requiresLeadingPageBreak;
        private volatile bool _requiresNoBreakInFormatting;
        private volatile bool _requiresOutputOptimization;
        private volatile bool _requiresPhoneNumbersAsPlainText;
        private volatile bool _requiresSpecialViewStateEncoding;
        private volatile bool _requiresUniqueFilePathSuffix;
        private volatile bool _requiresUniqueHtmlCheckboxNames;
        private volatile bool _requiresUniqueHtmlInputNames;
        private volatile bool _requiresUrlEncodedPostfieldValues;
        private volatile int _screenBitDepth;
        private volatile int _screenCharactersHeight;
        private volatile int _screenCharactersWidth;
        private volatile int _screenPixelsHeight;
        private volatile int _screenPixelsWidth;
        private static object _staticLock = new object();
        private volatile bool _supportsAccesskeyAttribute;
        private volatile bool _supportsBodyColor;
        private volatile bool _supportsBold;
        private volatile bool _supportsCacheControlMetaTag;
        private volatile bool _supportsCallback;
        private volatile bool _supportsCss;
        private volatile bool _supportsDivAlign;
        private volatile bool _supportsDivNoWrap;
        private volatile bool _supportsEmptyStringInCookieValue;
        private volatile bool _supportsFontColor;
        private volatile bool _supportsFontName;
        private volatile bool _supportsFontSize;
        private volatile bool _supportsImageSubmit;
        private volatile bool _supportsIModeSymbols;
        private volatile bool _supportsInputIStyle;
        private volatile bool _supportsInputMode;
        private volatile bool _supportsItalic;
        private volatile bool _supportsJPhoneMultiMediaAttributes;
        private volatile bool _supportsJPhoneSymbols;
        private volatile bool _supportsMaintainScrollPositionOnPostback;
        private volatile bool _supportsQueryStringInFormAction;
        private volatile bool _supportsRedirectWithCookie;
        private volatile bool _supportsSelectMultiple;
        private volatile bool _supportsUncheck;
        private volatile bool _supportsXmlHttp;
        private volatile bool _tables;
        private volatile System.Type _tagwriter;
        private volatile string _type;
        private bool _useOptimizedCacheKey = true;
        private volatile bool _vbscript;
        private volatile string _version;
        private volatile System.Version _w3cdomversion;
        private volatile bool _win16;
        private volatile bool _win32;
        private static object s_nullAdapterSingleton = new object();

        public void AddBrowser(string browserName)
        {
            if (this._browsers == null)
            {
                lock (_staticLock)
                {
                    if (this._browsers == null)
                    {
                        this._browsers = new ArrayList(6);
                    }
                }
            }
            this._browsers.Add(browserName.ToLower(CultureInfo.InvariantCulture));
        }

        private Exception BuildParseError(Exception e, string capsKey)
        {
            ConfigurationErrorsException exception = new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_string_from_browser_caps", new object[] { e.Message, capsKey, this[capsKey] }), e);
            HttpUnhandledException exception2 = new HttpUnhandledException(null, null);
            exception2.SetFormatter(new UseLastUnhandledErrorFormatter(exception));
            return exception2;
        }

        private bool CapsParseBool(string capsKey)
        {
            bool flag;
            try
            {
                flag = bool.Parse(this[capsKey]);
            }
            catch (FormatException exception)
            {
                throw this.BuildParseError(exception, capsKey);
            }
            return flag;
        }

        private bool CapsParseBoolDefault(string capsKey, bool defaultValue)
        {
            string str = this[capsKey];
            if (str == null)
            {
                return defaultValue;
            }
            try
            {
                return bool.Parse(str);
            }
            catch (FormatException)
            {
                return defaultValue;
            }
        }

        public System.Web.UI.HtmlTextWriter CreateHtmlTextWriter(TextWriter w)
        {
            string htmlTextWriter = this.HtmlTextWriter;
            if ((htmlTextWriter != null) && (htmlTextWriter.Length != 0))
            {
                System.Web.UI.HtmlTextWriter writer = null;
                try
                {
                    System.Type type = BuildManager.GetType(htmlTextWriter, true, false);
                    object[] args = new object[] { w };
                    writer = (System.Web.UI.HtmlTextWriter) Activator.CreateInstance(type, args);
                    if (writer != null)
                    {
                        return writer;
                    }
                }
                catch
                {
                    throw new Exception(System.Web.SR.GetString("Could_not_create_type_instance", new object[] { htmlTextWriter }));
                }
            }
            return this.CreateHtmlTextWriterInternal(w);
        }

        internal System.Web.UI.HtmlTextWriter CreateHtmlTextWriterInternal(TextWriter tw)
        {
            System.Type tagWriter = this.TagWriter;
            if (tagWriter != null)
            {
                return Page.CreateHtmlTextWriterFromType(tw, tagWriter);
            }
            return new Html32TextWriter(tw);
        }

        public void DisableOptimizedCacheKey()
        {
            this._useOptimizedCacheKey = false;
        }

        internal ControlAdapter GetAdapter(Control control)
        {
            if ((this._adapters == null) || (this._adapters.Count == 0))
            {
                return null;
            }
            if (control == null)
            {
                return null;
            }
            System.Type type = control.GetType();
            object objA = this.AdapterTypes[type];
            if (object.ReferenceEquals(objA, s_nullAdapterSingleton))
            {
                return null;
            }
            System.Type adapterType = (System.Type) objA;
            if (adapterType == null)
            {
                System.Type baseType = type;
                string assemblyQualifiedName = null;
                string str2 = null;
                while ((str2 == null) && (baseType != typeof(Control)))
                {
                    assemblyQualifiedName = baseType.AssemblyQualifiedName;
                    str2 = (string) this.Adapters[assemblyQualifiedName];
                    if (str2 == null)
                    {
                        assemblyQualifiedName = baseType.FullName;
                        str2 = (string) this.Adapters[assemblyQualifiedName];
                    }
                    if (str2 != null)
                    {
                        break;
                    }
                    baseType = baseType.BaseType;
                }
                if (string.IsNullOrEmpty(str2))
                {
                    this.AdapterTypes[type] = s_nullAdapterSingleton;
                    return null;
                }
                adapterType = BuildManager.GetType(str2, false, false);
                if (adapterType == null)
                {
                    throw new Exception(System.Web.SR.GetString("ControlAdapters_TypeNotFound", new object[] { str2 }));
                }
                this.AdapterTypes[type] = adapterType;
            }
            ControlAdapter adapter = (ControlAdapter) this.GetAdapterFactory(adapterType).CreateInstance();
            adapter._control = control;
            return adapter;
        }

        private IWebObjectFactory GetAdapterFactory(System.Type adapterType)
        {
            if (_controlAdapterFactoryGenerator == null)
            {
                lock (_staticLock)
                {
                    if (_controlAdapterFactoryGenerator == null)
                    {
                        _controlAdapterFactoryTable = new Hashtable();
                        _controlAdapterFactoryGenerator = new FactoryGenerator();
                    }
                }
            }
            IWebObjectFactory factory = (IWebObjectFactory) _controlAdapterFactoryTable[adapterType];
            if (factory == null)
            {
                lock (_controlAdapterFactoryTable.SyncRoot)
                {
                    factory = (IWebObjectFactory) _controlAdapterFactoryTable[adapterType];
                    if (factory != null)
                    {
                        return factory;
                    }
                    try
                    {
                        factory = _controlAdapterFactoryGenerator.CreateFactory(adapterType);
                    }
                    catch
                    {
                        throw new Exception(System.Web.SR.GetString("Could_not_create_type_instance", new object[] { adapterType.ToString() }));
                    }
                    _controlAdapterFactoryTable[adapterType] = factory;
                }
            }
            return factory;
        }

        internal static HttpBrowserCapabilities GetBrowserCapabilities(HttpRequest request)
        {
            HttpCapabilitiesBase browserCapabilities = null;
            HttpCapabilitiesDefaultProvider browserCaps = RuntimeConfig.GetConfig(request.Context).BrowserCaps;
            if (browserCaps != null)
            {
                if (BrowserCapabilitiesProvider != null)
                {
                    browserCaps.BrowserCapabilitiesProvider = BrowserCapabilitiesProvider;
                }
                if (browserCaps.BrowserCapabilitiesProvider == null)
                {
                    browserCapabilities = browserCaps.Evaluate(request);
                }
                else
                {
                    browserCapabilities = browserCaps.BrowserCapabilitiesProvider.GetBrowserCapabilities(request);
                }
            }
            return (HttpBrowserCapabilities) browserCapabilities;
        }

        public System.Version[] GetClrVersions()
        {
            string userAgent = HttpCapabilitiesDefaultProvider.GetUserAgent(HttpContext.Current.Request);
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }
            MatchCollection matchs = new Regex(@"\.NET CLR (?'clrVersion'[0-9\.]*)").Matches(userAgent);
            if (matchs.Count == 0)
            {
                return new System.Version[] { new System.Version() };
            }
            ArrayList list = new ArrayList();
            foreach (Match match in matchs)
            {
                try
                {
                    System.Version version = new System.Version(match.Groups["clrVersion"].Value);
                    list.Add(version);
                }
                catch (FormatException)
                {
                }
            }
            list.Sort();
            return (System.Version[]) list.ToArray(typeof(System.Version));
        }

        [ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        public static HttpCapabilitiesBase GetConfigCapabilities(string configKey, HttpRequest request)
        {
            HttpCapabilitiesBase browserCapabilities = null;
            if (configKey == "system.web/browserCaps")
            {
                browserCapabilities = GetBrowserCapabilities(request);
            }
            else
            {
                HttpCapabilitiesDefaultProvider section = (HttpCapabilitiesDefaultProvider) request.Context.GetSection(configKey);
                if (section != null)
                {
                    if (BrowserCapabilitiesProvider != null)
                    {
                        section.BrowserCapabilitiesProvider = BrowserCapabilitiesProvider;
                    }
                    if (section.BrowserCapabilitiesProvider == null)
                    {
                        browserCapabilities = section.Evaluate(request);
                    }
                    else
                    {
                        browserCapabilities = section.BrowserCapabilitiesProvider.GetBrowserCapabilities(request);
                    }
                }
            }
            if (browserCapabilities == null)
            {
                browserCapabilities = EmptyHttpCapabilitiesBase;
            }
            return browserCapabilities;
        }

        protected virtual void Init()
        {
        }

        internal void InitInternal(HttpBrowserCapabilities browserCaps)
        {
            if (this._items != null)
            {
                throw new ArgumentException(System.Web.SR.GetString("Caps_cannot_be_inited_twice"));
            }
            this._items = browserCaps._items;
            this._adapters = browserCaps._adapters;
            this._browsers = browserCaps._browsers;
            this._htmlTextWriter = browserCaps._htmlTextWriter;
            this._useOptimizedCacheKey = browserCaps._useOptimizedCacheKey;
            this.Init();
        }

        public bool IsBrowser(string browserName)
        {
            if (!string.IsNullOrEmpty(browserName))
            {
                if (this._browsers == null)
                {
                    return false;
                }
                for (int i = 0; i < this._browsers.Count; i++)
                {
                    if (string.Equals(browserName, (string) this._browsers[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        int IFilterResolutionService.CompareFilters(string filter1, string filter2)
        {
            return BrowserCapabilitiesCompiler.BrowserCapabilitiesFactory.CompareFilters(filter1, filter2);
        }

        bool IFilterResolutionService.EvaluateFilter(string filterName)
        {
            return this.IsBrowser(filterName);
        }

        public bool ActiveXControls
        {
            get
            {
                if (!this._haveactivexcontrols)
                {
                    this._activexcontrols = this.CapsParseBool("activexcontrols");
                    this._haveactivexcontrols = true;
                }
                return this._activexcontrols;
            }
        }

        public IDictionary Adapters
        {
            get
            {
                if (this._adapters == null)
                {
                    lock (_staticLock)
                    {
                        if (this._adapters == null)
                        {
                            this._adapters = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        }
                    }
                }
                return this._adapters;
            }
        }

        private Hashtable AdapterTypes
        {
            get
            {
                if (this._adapterTypes == null)
                {
                    lock (_staticLock)
                    {
                        if (this._adapterTypes == null)
                        {
                            this._adapterTypes = Hashtable.Synchronized(new Hashtable());
                        }
                    }
                }
                return this._adapterTypes;
            }
        }

        public bool AOL
        {
            get
            {
                if (!this._haveaol)
                {
                    this._aol = this.CapsParseBool("aol");
                    this._haveaol = true;
                }
                return this._aol;
            }
        }

        public bool BackgroundSounds
        {
            get
            {
                if (!this._havebackgroundsounds)
                {
                    this._backgroundsounds = this.CapsParseBool("backgroundsounds");
                    this._havebackgroundsounds = true;
                }
                return this._backgroundsounds;
            }
        }

        public bool Beta
        {
            get
            {
                if (!this._havebeta)
                {
                    this._beta = this.CapsParseBool("beta");
                    this._havebeta = true;
                }
                return this._beta;
            }
        }

        public string Browser
        {
            get
            {
                if (!this._havebrowser)
                {
                    this._browser = this["browser"];
                    this._havebrowser = true;
                }
                return this._browser;
            }
        }

        public static HttpCapabilitiesProvider BrowserCapabilitiesProvider
        {
            get
            {
                return _browserCapabilitiesProvider;
            }
            set
            {
                _browserCapabilitiesProvider = value;
            }
        }

        public ArrayList Browsers
        {
            get
            {
                return this._browsers;
            }
        }

        public virtual bool CanCombineFormsInDeck
        {
            get
            {
                if (!this._haveCanCombineFormsInDeck)
                {
                    this._canCombineFormsInDeck = this.CapsParseBoolDefault("canCombineFormsInDeck", true);
                    this._haveCanCombineFormsInDeck = true;
                }
                return this._canCombineFormsInDeck;
            }
        }

        public virtual bool CanInitiateVoiceCall
        {
            get
            {
                if (!this._haveCanInitiateVoiceCall)
                {
                    this._canInitiateVoiceCall = this.CapsParseBoolDefault("canInitiateVoiceCall", false);
                    this._haveCanInitiateVoiceCall = true;
                }
                return this._canInitiateVoiceCall;
            }
        }

        public virtual bool CanRenderAfterInputOrSelectElement
        {
            get
            {
                if (!this._haveCanRenderAfterInputOrSelectElement)
                {
                    this._canRenderAfterInputOrSelectElement = this.CapsParseBoolDefault("canRenderAfterInputOrSelectElement", true);
                    this._haveCanRenderAfterInputOrSelectElement = true;
                }
                return this._canRenderAfterInputOrSelectElement;
            }
        }

        public virtual bool CanRenderEmptySelects
        {
            get
            {
                if (!this._haveCanRenderEmptySelects)
                {
                    this._canRenderEmptySelects = this.CapsParseBoolDefault("canRenderEmptySelects", true);
                    this._haveCanRenderEmptySelects = true;
                }
                return this._canRenderEmptySelects;
            }
        }

        public virtual bool CanRenderInputAndSelectElementsTogether
        {
            get
            {
                if (!this._haveCanRenderInputAndSelectElementsTogether)
                {
                    this._canRenderInputAndSelectElementsTogether = this.CapsParseBoolDefault("canRenderInputAndSelectElementsTogether", true);
                    this._haveCanRenderInputAndSelectElementsTogether = true;
                }
                return this._canRenderInputAndSelectElementsTogether;
            }
        }

        public virtual bool CanRenderMixedSelects
        {
            get
            {
                if (!this._haveCanRenderMixedSelects)
                {
                    this._canRenderMixedSelects = this.CapsParseBoolDefault("canRenderMixedSelects", true);
                    this._haveCanRenderMixedSelects = true;
                }
                return this._canRenderMixedSelects;
            }
        }

        public virtual bool CanRenderOneventAndPrevElementsTogether
        {
            get
            {
                if (!this._haveCanRenderOneventAndPrevElementsTogether)
                {
                    this._canRenderOneventAndPrevElementsTogether = this.CapsParseBoolDefault("canRenderOneventAndPrevElementsTogether", true);
                    this._haveCanRenderOneventAndPrevElementsTogether = true;
                }
                return this._canRenderOneventAndPrevElementsTogether;
            }
        }

        public virtual bool CanRenderPostBackCards
        {
            get
            {
                if (!this._haveCanRenderPostBackCards)
                {
                    this._canRenderPostBackCards = this.CapsParseBoolDefault("canRenderPostBackCards", true);
                    this._haveCanRenderPostBackCards = true;
                }
                return this._canRenderPostBackCards;
            }
        }

        public virtual bool CanRenderSetvarZeroWithMultiSelectionList
        {
            get
            {
                if (!this._haveCanRenderSetvarZeroWithMultiSelectionList)
                {
                    this._canRenderSetvarZeroWithMultiSelectionList = this.CapsParseBoolDefault("canRenderSetvarZeroWithMultiSelectionList", true);
                    this._haveCanRenderSetvarZeroWithMultiSelectionList = true;
                }
                return this._canRenderSetvarZeroWithMultiSelectionList;
            }
        }

        public virtual bool CanSendMail
        {
            get
            {
                if (!this._haveCanSendMail)
                {
                    this._canSendMail = this.CapsParseBoolDefault("canSendMail", true);
                    this._haveCanSendMail = true;
                }
                return this._canSendMail;
            }
        }

        public IDictionary Capabilities
        {
            get
            {
                return this._items;
            }
            set
            {
                this._items = value;
            }
        }

        public bool CDF
        {
            get
            {
                if (!this._havecdf)
                {
                    this._cdf = this.CapsParseBool("cdf");
                    this._havecdf = true;
                }
                return this._cdf;
            }
        }

        public System.Version ClrVersion
        {
            get
            {
                System.Version[] clrVersions = this.GetClrVersions();
                if (clrVersions != null)
                {
                    return clrVersions[clrVersions.Length - 1];
                }
                return null;
            }
        }

        public bool Cookies
        {
            get
            {
                if (!this._havecookies)
                {
                    this._cookies = this.CapsParseBool("cookies");
                    this._havecookies = true;
                }
                return this._cookies;
            }
        }

        public bool Crawler
        {
            get
            {
                if (!this._havecrawler)
                {
                    this._crawler = this.CapsParseBool("crawler");
                    this._havecrawler = true;
                }
                return this._crawler;
            }
        }

        public virtual int DefaultSubmitButtonLimit
        {
            get
            {
                if (!this._haveDefaultSubmitButtonLimit)
                {
                    string str = this["defaultSubmitButtonLimit"];
                    this._defaultSubmitButtonLimit = (str != null) ? Convert.ToInt32(this["defaultSubmitButtonLimit"], CultureInfo.InvariantCulture) : 1;
                    this._haveDefaultSubmitButtonLimit = true;
                }
                return this._defaultSubmitButtonLimit;
            }
        }

        public System.Version EcmaScriptVersion
        {
            get
            {
                if (!this._haveecmascriptversion)
                {
                    this._ecmascriptversion = new System.Version(this["ecmascriptversion"]);
                    this._haveecmascriptversion = true;
                }
                return this._ecmascriptversion;
            }
        }

        internal static HttpCapabilitiesBase EmptyHttpCapabilitiesBase
        {
            get
            {
                if (_emptyHttpCapabilitiesBase == null)
                {
                    lock (_emptyHttpCapabilitiesBaseLock)
                    {
                        if (_emptyHttpCapabilitiesBase != null)
                        {
                            return _emptyHttpCapabilitiesBase;
                        }
                        _emptyHttpCapabilitiesBase = new HttpCapabilitiesBase();
                    }
                }
                return _emptyHttpCapabilitiesBase;
            }
        }

        public bool Frames
        {
            get
            {
                if (!this._haveframes)
                {
                    this._frames = this.CapsParseBool("frames");
                    this._haveframes = true;
                }
                return this._frames;
            }
        }

        public virtual int GatewayMajorVersion
        {
            get
            {
                if (!this._haveGatewayMajorVersion)
                {
                    this._gatewayMajorVersion = Convert.ToInt32(this["gatewayMajorVersion"], CultureInfo.InvariantCulture);
                    this._haveGatewayMajorVersion = true;
                }
                return this._gatewayMajorVersion;
            }
        }

        public virtual double GatewayMinorVersion
        {
            get
            {
                if (!this._haveGatewayMinorVersion)
                {
                    this._gatewayMinorVersion = double.Parse(this["gatewayMinorVersion"], NumberStyles.Float, (IFormatProvider) NumberFormatInfo.InvariantInfo);
                    this._haveGatewayMinorVersion = true;
                }
                return this._gatewayMinorVersion;
            }
        }

        public virtual string GatewayVersion
        {
            get
            {
                if (!this._haveGatewayVersion)
                {
                    this._gatewayVersion = this["gatewayVersion"];
                    this._haveGatewayVersion = true;
                }
                return this._gatewayVersion;
            }
        }

        public virtual bool HasBackButton
        {
            get
            {
                if (!this._haveHasBackButton)
                {
                    this._hasBackButton = this.CapsParseBoolDefault("hasBackButton", true);
                    this._haveHasBackButton = true;
                }
                return this._hasBackButton;
            }
        }

        public virtual bool HidesRightAlignedMultiselectScrollbars
        {
            get
            {
                if (!this._haveHidesRightAlignedMultiselectScrollbars)
                {
                    this._hidesRightAlignedMultiselectScrollbars = this.CapsParseBoolDefault("hidesRightAlignedMultiselectScrollbars", false);
                    this._haveHidesRightAlignedMultiselectScrollbars = true;
                }
                return this._hidesRightAlignedMultiselectScrollbars;
            }
        }

        public string HtmlTextWriter
        {
            get
            {
                return this._htmlTextWriter;
            }
            set
            {
                this._htmlTextWriter = value;
            }
        }

        public string Id
        {
            get
            {
                if (this._browsers != null)
                {
                    return (string) this._browsers[this._browsers.Count - 1];
                }
                return string.Empty;
            }
        }

        public virtual string InputType
        {
            get
            {
                if (!this._haveInputType)
                {
                    this._inputType = this["inputType"];
                    this._haveInputType = true;
                }
                return this._inputType;
            }
        }

        public virtual bool IsColor
        {
            get
            {
                if (!this._haveIsColor)
                {
                    if (this["isColor"] == null)
                    {
                        this._isColor = false;
                    }
                    else
                    {
                        this._isColor = Convert.ToBoolean(this["isColor"], CultureInfo.InvariantCulture);
                    }
                    this._haveIsColor = true;
                }
                return this._isColor;
            }
        }

        public virtual bool IsMobileDevice
        {
            get
            {
                if (!this._haveIsMobileDevice)
                {
                    this._isMobileDevice = this.CapsParseBoolDefault("isMobileDevice", false);
                    this._haveIsMobileDevice = true;
                }
                return this._isMobileDevice;
            }
        }

        public virtual string this[string key]
        {
            get
            {
                return (string) this._items[key];
            }
        }

        public bool JavaApplets
        {
            get
            {
                if (!this._havejavaapplets)
                {
                    this._javaapplets = this.CapsParseBool("javaapplets");
                    this._havejavaapplets = true;
                }
                return this._javaapplets;
            }
        }

        [Obsolete("The recommended alternative is the EcmaScriptVersion property. A Major version value greater than or equal to 1 implies JavaScript support. http://go.microsoft.com/fwlink/?linkid=14202")]
        public bool JavaScript
        {
            get
            {
                if (!this._havejavascript)
                {
                    this._javascript = this.CapsParseBool("javascript");
                    this._havejavascript = true;
                }
                return this._javascript;
            }
        }

        public System.Version JScriptVersion
        {
            get
            {
                if (!this._havejscriptversion)
                {
                    this._jscriptversion = new System.Version(this["jscriptversion"]);
                    this._havejscriptversion = true;
                }
                return this._jscriptversion;
            }
        }

        public int MajorVersion
        {
            get
            {
                if (!this._havemajorversion)
                {
                    try
                    {
                        this._majorversion = int.Parse(this["majorversion"], CultureInfo.InvariantCulture);
                        this._havemajorversion = true;
                    }
                    catch (FormatException exception)
                    {
                        throw this.BuildParseError(exception, "majorversion");
                    }
                }
                return this._majorversion;
            }
        }

        public virtual int MaximumHrefLength
        {
            get
            {
                if (!this._haveMaximumHrefLength)
                {
                    this._maximumHrefLength = Convert.ToInt32(this["maximumHrefLength"], CultureInfo.InvariantCulture);
                    this._haveMaximumHrefLength = true;
                }
                return this._maximumHrefLength;
            }
        }

        public virtual int MaximumRenderedPageSize
        {
            get
            {
                if (!this._haveMaximumRenderedPageSize)
                {
                    this._maximumRenderedPageSize = Convert.ToInt32(this["maximumRenderedPageSize"], CultureInfo.InvariantCulture);
                    this._haveMaximumRenderedPageSize = true;
                }
                return this._maximumRenderedPageSize;
            }
        }

        public virtual int MaximumSoftkeyLabelLength
        {
            get
            {
                if (!this._haveMaximumSoftkeyLabelLength)
                {
                    this._maximumSoftkeyLabelLength = Convert.ToInt32(this["maximumSoftkeyLabelLength"], CultureInfo.InvariantCulture);
                    this._haveMaximumSoftkeyLabelLength = true;
                }
                return this._maximumSoftkeyLabelLength;
            }
        }

        public double MinorVersion
        {
            get
            {
                if (!this._haveminorversion)
                {
                    lock (_staticLock)
                    {
                        if (!this._haveminorversion)
                        {
                            try
                            {
                                this._minorversion = double.Parse(this["minorversion"], NumberStyles.Float, (IFormatProvider) NumberFormatInfo.InvariantInfo);
                                this._haveminorversion = true;
                            }
                            catch (FormatException exception)
                            {
                                string str = this["minorversion"];
                                int index = str.IndexOf('.');
                                if (index != -1)
                                {
                                    int length = str.IndexOf('.', index + 1);
                                    if (length != -1)
                                    {
                                        try
                                        {
                                            this._minorversion = double.Parse(str.Substring(0, length), NumberStyles.Float, (IFormatProvider) NumberFormatInfo.InvariantInfo);
                                            Thread.MemoryBarrier();
                                            this._haveminorversion = true;
                                        }
                                        catch (FormatException)
                                        {
                                        }
                                    }
                                }
                                if (!this._haveminorversion)
                                {
                                    throw this.BuildParseError(exception, "minorversion");
                                }
                            }
                        }
                    }
                }
                return this._minorversion;
            }
        }

        public string MinorVersionString
        {
            get
            {
                return this["minorversion"];
            }
        }

        public virtual string MobileDeviceManufacturer
        {
            get
            {
                if (!this._haveMobileDeviceManufacturer)
                {
                    this._mobileDeviceManufacturer = this["mobileDeviceManufacturer"];
                    this._haveMobileDeviceManufacturer = true;
                }
                return this._mobileDeviceManufacturer;
            }
        }

        public virtual string MobileDeviceModel
        {
            get
            {
                if (!this._haveMobileDeviceModel)
                {
                    this._mobileDeviceModel = this["mobileDeviceModel"];
                    this._haveMobileDeviceModel = true;
                }
                return this._mobileDeviceModel;
            }
        }

        public System.Version MSDomVersion
        {
            get
            {
                if (!this._havemsdomversion)
                {
                    this._msdomversion = new System.Version(this["msdomversion"]);
                    this._havemsdomversion = true;
                }
                return this._msdomversion;
            }
        }

        public virtual int NumberOfSoftkeys
        {
            get
            {
                if (!this._haveNumberOfSoftkeys)
                {
                    this._numberOfSoftkeys = Convert.ToInt32(this["numberOfSoftkeys"], CultureInfo.InvariantCulture);
                    this._haveNumberOfSoftkeys = true;
                }
                return this._numberOfSoftkeys;
            }
        }

        public string Platform
        {
            get
            {
                if (!this._haveplatform)
                {
                    this._platform = this["platform"];
                    this._haveplatform = true;
                }
                return this._platform;
            }
        }

        public virtual string PreferredImageMime
        {
            get
            {
                if (!this._havePreferredImageMime)
                {
                    this._preferredImageMime = this["preferredImageMime"];
                    this._havePreferredImageMime = true;
                }
                return this._preferredImageMime;
            }
        }

        public virtual string PreferredRenderingMime
        {
            get
            {
                if (!this._havePreferredRenderingMime)
                {
                    this._preferredRenderingMime = this["preferredRenderingMime"];
                    this._havePreferredRenderingMime = true;
                }
                return this._preferredRenderingMime;
            }
        }

        public virtual string PreferredRenderingType
        {
            get
            {
                if (!this._havePreferredRenderingType)
                {
                    this._preferredRenderingType = this["preferredRenderingType"];
                    this._havePreferredRenderingType = true;
                }
                return this._preferredRenderingType;
            }
        }

        public virtual string PreferredRequestEncoding
        {
            get
            {
                if (!this._havePreferredRequestEncoding)
                {
                    this._preferredRequestEncoding = this["preferredRequestEncoding"];
                    Thread.MemoryBarrier();
                    this._havePreferredRequestEncoding = true;
                }
                return this._preferredRequestEncoding;
            }
        }

        public virtual string PreferredResponseEncoding
        {
            get
            {
                if (!this._havePreferredResponseEncoding)
                {
                    this._preferredResponseEncoding = this["preferredResponseEncoding"];
                    this._havePreferredResponseEncoding = true;
                }
                return this._preferredResponseEncoding;
            }
        }

        public virtual bool RendersBreakBeforeWmlSelectAndInput
        {
            get
            {
                if (!this._haveRendersBreakBeforeWmlSelectAndInput)
                {
                    this._rendersBreakBeforeWmlSelectAndInput = this.CapsParseBoolDefault("rendersBreakBeforeWmlSelectAndInput", false);
                    this._haveRendersBreakBeforeWmlSelectAndInput = true;
                }
                return this._rendersBreakBeforeWmlSelectAndInput;
            }
        }

        public virtual bool RendersBreaksAfterHtmlLists
        {
            get
            {
                if (!this._haveRendersBreaksAfterHtmlLists)
                {
                    this._rendersBreaksAfterHtmlLists = this.CapsParseBoolDefault("rendersBreaksAfterHtmlLists", true);
                    this._haveRendersBreaksAfterHtmlLists = true;
                }
                return this._rendersBreaksAfterHtmlLists;
            }
        }

        public virtual bool RendersBreaksAfterWmlAnchor
        {
            get
            {
                if (!this._haveRendersBreaksAfterWmlAnchor)
                {
                    this._rendersBreaksAfterWmlAnchor = this.CapsParseBoolDefault("rendersBreaksAfterWmlAnchor", true);
                    this._haveRendersBreaksAfterWmlAnchor = true;
                }
                return this._rendersBreaksAfterWmlAnchor;
            }
        }

        public virtual bool RendersBreaksAfterWmlInput
        {
            get
            {
                if (!this._haveRendersBreaksAfterWmlInput)
                {
                    this._rendersBreaksAfterWmlInput = this.CapsParseBoolDefault("rendersBreaksAfterWmlInput", true);
                    this._haveRendersBreaksAfterWmlInput = true;
                }
                return this._rendersBreaksAfterWmlInput;
            }
        }

        public virtual bool RendersWmlDoAcceptsInline
        {
            get
            {
                if (!this._haveRendersWmlDoAcceptsInline)
                {
                    this._rendersWmlDoAcceptsInline = this.CapsParseBoolDefault("rendersWmlDoAcceptsInline", true);
                    this._haveRendersWmlDoAcceptsInline = true;
                }
                return this._rendersWmlDoAcceptsInline;
            }
        }

        public virtual bool RendersWmlSelectsAsMenuCards
        {
            get
            {
                if (!this._haveRendersWmlSelectsAsMenuCards)
                {
                    this._rendersWmlSelectsAsMenuCards = this.CapsParseBoolDefault("rendersWmlSelectsAsMenuCards", false);
                    this._haveRendersWmlSelectsAsMenuCards = true;
                }
                return this._rendersWmlSelectsAsMenuCards;
            }
        }

        public virtual string RequiredMetaTagNameValue
        {
            get
            {
                if (!this._haveRequiredMetaTagNameValue)
                {
                    string str = this["requiredMetaTagNameValue"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        this._requiredMetaTagNameValue = str;
                    }
                    else
                    {
                        this._requiredMetaTagNameValue = null;
                    }
                    this._haveRequiredMetaTagNameValue = true;
                }
                return this._requiredMetaTagNameValue;
            }
        }

        public virtual bool RequiresAttributeColonSubstitution
        {
            get
            {
                if (!this._haveRequiresAttributeColonSubstitution)
                {
                    this._requiresAttributeColonSubstitution = this.CapsParseBoolDefault("requiresAttributeColonSubstitution", false);
                    this._haveRequiresAttributeColonSubstitution = true;
                }
                return this._requiresAttributeColonSubstitution;
            }
        }

        public virtual bool RequiresContentTypeMetaTag
        {
            get
            {
                if (!this._haveRequiresContentTypeMetaTag)
                {
                    this._requiresContentTypeMetaTag = this.CapsParseBoolDefault("requiresContentTypeMetaTag", false);
                    this._haveRequiresContentTypeMetaTag = true;
                }
                return this._requiresContentTypeMetaTag;
            }
        }

        public bool RequiresControlStateInSession
        {
            get
            {
                if (!this._haverequiresControlStateInSession)
                {
                    if (this["requiresControlStateInSession"] != null)
                    {
                        this._requiresControlStateInSession = this.CapsParseBoolDefault("requiresControlStateInSession", false);
                    }
                    this._haverequiresControlStateInSession = true;
                }
                return this._requiresControlStateInSession;
            }
        }

        public virtual bool RequiresDBCSCharacter
        {
            get
            {
                if (!this._haveRequiresDBCSCharacter)
                {
                    this._requiresDBCSCharacter = this.CapsParseBoolDefault("requiresDBCSCharacter", false);
                    this._haveRequiresDBCSCharacter = true;
                }
                return this._requiresDBCSCharacter;
            }
        }

        public virtual bool RequiresHtmlAdaptiveErrorReporting
        {
            get
            {
                if (!this._haveRequiresHtmlAdaptiveErrorReporting)
                {
                    this._requiresHtmlAdaptiveErrorReporting = this.CapsParseBoolDefault("requiresHtmlAdaptiveErrorReporting", false);
                    this._haveRequiresHtmlAdaptiveErrorReporting = true;
                }
                return this._requiresHtmlAdaptiveErrorReporting;
            }
        }

        public virtual bool RequiresLeadingPageBreak
        {
            get
            {
                if (!this._haveRequiresLeadingPageBreak)
                {
                    this._requiresLeadingPageBreak = this.CapsParseBoolDefault("requiresLeadingPageBreak", false);
                    this._haveRequiresLeadingPageBreak = true;
                }
                return this._requiresLeadingPageBreak;
            }
        }

        public virtual bool RequiresNoBreakInFormatting
        {
            get
            {
                if (!this._haveRequiresNoBreakInFormatting)
                {
                    this._requiresNoBreakInFormatting = this.CapsParseBoolDefault("requiresNoBreakInFormatting", false);
                    this._haveRequiresNoBreakInFormatting = true;
                }
                return this._requiresNoBreakInFormatting;
            }
        }

        public virtual bool RequiresOutputOptimization
        {
            get
            {
                if (!this._haveRequiresOutputOptimization)
                {
                    this._requiresOutputOptimization = this.CapsParseBoolDefault("requiresOutputOptimization", false);
                    this._haveRequiresOutputOptimization = true;
                }
                return this._requiresOutputOptimization;
            }
        }

        public virtual bool RequiresPhoneNumbersAsPlainText
        {
            get
            {
                if (!this._haveRequiresPhoneNumbersAsPlainText)
                {
                    this._requiresPhoneNumbersAsPlainText = this.CapsParseBoolDefault("requiresPhoneNumbersAsPlainText", false);
                    this._haveRequiresPhoneNumbersAsPlainText = true;
                }
                return this._requiresPhoneNumbersAsPlainText;
            }
        }

        public virtual bool RequiresSpecialViewStateEncoding
        {
            get
            {
                if (!this._haveRequiresSpecialViewStateEncoding)
                {
                    this._requiresSpecialViewStateEncoding = this.CapsParseBoolDefault("requiresSpecialViewStateEncoding", false);
                    this._haveRequiresSpecialViewStateEncoding = true;
                }
                return this._requiresSpecialViewStateEncoding;
            }
        }

        public virtual bool RequiresUniqueFilePathSuffix
        {
            get
            {
                if (!this._haveRequiresUniqueFilePathSuffix)
                {
                    this._requiresUniqueFilePathSuffix = this.CapsParseBoolDefault("requiresUniqueFilePathSuffix", false);
                    this._haveRequiresUniqueFilePathSuffix = true;
                }
                return this._requiresUniqueFilePathSuffix;
            }
        }

        public virtual bool RequiresUniqueHtmlCheckboxNames
        {
            get
            {
                if (!this._haveRequiresUniqueHtmlCheckboxNames)
                {
                    this._requiresUniqueHtmlCheckboxNames = this.CapsParseBoolDefault("requiresUniqueHtmlCheckboxNames", false);
                    this._haveRequiresUniqueHtmlCheckboxNames = true;
                }
                return this._requiresUniqueHtmlCheckboxNames;
            }
        }

        public virtual bool RequiresUniqueHtmlInputNames
        {
            get
            {
                if (!this._haveRequiresUniqueHtmlInputNames)
                {
                    this._requiresUniqueHtmlInputNames = this.CapsParseBoolDefault("requiresUniqueHtmlInputNames", false);
                    this._haveRequiresUniqueHtmlInputNames = true;
                }
                return this._requiresUniqueHtmlInputNames;
            }
        }

        public virtual bool RequiresUrlEncodedPostfieldValues
        {
            get
            {
                if (!this._haveRequiresUrlEncodedPostfieldValues)
                {
                    this._requiresUrlEncodedPostfieldValues = this.CapsParseBoolDefault("requiresUrlEncodedPostfieldValues", true);
                    this._haveRequiresUrlEncodedPostfieldValues = true;
                }
                return this._requiresUrlEncodedPostfieldValues;
            }
        }

        public virtual int ScreenBitDepth
        {
            get
            {
                if (!this._haveScreenBitDepth)
                {
                    this._screenBitDepth = Convert.ToInt32(this["screenBitDepth"], CultureInfo.InvariantCulture);
                    this._haveScreenBitDepth = true;
                }
                return this._screenBitDepth;
            }
        }

        public virtual int ScreenCharactersHeight
        {
            get
            {
                if (!this._haveScreenCharactersHeight)
                {
                    if (this["screenCharactersHeight"] == null)
                    {
                        int num = 480;
                        int num2 = 12;
                        if ((this["screenPixelsHeight"] != null) && (this["characterHeight"] != null))
                        {
                            num = Convert.ToInt32(this["screenPixelsHeight"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["characterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["screenPixelsHeight"] != null)
                        {
                            num = Convert.ToInt32(this["screenPixelsHeight"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["defaultCharacterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["characterHeight"] != null)
                        {
                            num = Convert.ToInt32(this["defaultScreenPixelsHeight"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["characterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["defaultScreenCharactersHeight"] != null)
                        {
                            num = Convert.ToInt32(this["defaultScreenCharactersHeight"], CultureInfo.InvariantCulture);
                            num2 = 1;
                        }
                        this._screenCharactersHeight = num / num2;
                    }
                    else
                    {
                        this._screenCharactersHeight = Convert.ToInt32(this["screenCharactersHeight"], CultureInfo.InvariantCulture);
                    }
                    this._haveScreenCharactersHeight = true;
                }
                return this._screenCharactersHeight;
            }
        }

        public virtual int ScreenCharactersWidth
        {
            get
            {
                if (!this._haveScreenCharactersWidth)
                {
                    if (this["screenCharactersWidth"] == null)
                    {
                        int num = 640;
                        int num2 = 8;
                        if ((this["screenPixelsWidth"] != null) && (this["characterWidth"] != null))
                        {
                            num = Convert.ToInt32(this["screenPixelsWidth"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["characterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["screenPixelsWidth"] != null)
                        {
                            num = Convert.ToInt32(this["screenPixelsWidth"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["defaultCharacterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["characterWidth"] != null)
                        {
                            num = Convert.ToInt32(this["defaultScreenPixelsWidth"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["characterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["defaultScreenCharactersWidth"] != null)
                        {
                            num = Convert.ToInt32(this["defaultScreenCharactersWidth"], CultureInfo.InvariantCulture);
                            num2 = 1;
                        }
                        this._screenCharactersWidth = num / num2;
                    }
                    else
                    {
                        this._screenCharactersWidth = Convert.ToInt32(this["screenCharactersWidth"], CultureInfo.InvariantCulture);
                    }
                    this._haveScreenCharactersWidth = true;
                }
                return this._screenCharactersWidth;
            }
        }

        public virtual int ScreenPixelsHeight
        {
            get
            {
                if (!this._haveScreenPixelsHeight)
                {
                    if (this["screenPixelsHeight"] == null)
                    {
                        int num = 40;
                        int num2 = 12;
                        if ((this["screenCharactersHeight"] != null) && (this["characterHeight"] != null))
                        {
                            num = Convert.ToInt32(this["screenCharactersHeight"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["characterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["screenCharactersHeight"] != null)
                        {
                            num = Convert.ToInt32(this["screenCharactersHeight"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["defaultCharacterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["characterHeight"] != null)
                        {
                            num = Convert.ToInt32(this["defaultScreenCharactersHeight"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["characterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["defaultScreenPixelsHeight"] != null)
                        {
                            num = Convert.ToInt32(this["defaultScreenPixelsHeight"], CultureInfo.InvariantCulture);
                            num2 = 1;
                        }
                        this._screenPixelsHeight = num * num2;
                    }
                    else
                    {
                        this._screenPixelsHeight = Convert.ToInt32(this["screenPixelsHeight"], CultureInfo.InvariantCulture);
                    }
                    this._haveScreenPixelsHeight = true;
                }
                return this._screenPixelsHeight;
            }
        }

        public virtual int ScreenPixelsWidth
        {
            get
            {
                if (!this._haveScreenPixelsWidth)
                {
                    if (this["screenPixelsWidth"] == null)
                    {
                        int num = 80;
                        int num2 = 8;
                        if ((this["screenCharactersWidth"] != null) && (this["characterWidth"] != null))
                        {
                            num = Convert.ToInt32(this["screenCharactersWidth"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["characterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["screenCharactersWidth"] != null)
                        {
                            num = Convert.ToInt32(this["screenCharactersWidth"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["defaultCharacterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["characterWidth"] != null)
                        {
                            num = Convert.ToInt32(this["defaultScreenCharactersWidth"], CultureInfo.InvariantCulture);
                            num2 = Convert.ToInt32(this["characterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["defaultScreenPixelsWidth"] != null)
                        {
                            num = Convert.ToInt32(this["defaultScreenPixelsWidth"], CultureInfo.InvariantCulture);
                            num2 = 1;
                        }
                        this._screenPixelsWidth = num * num2;
                    }
                    else
                    {
                        this._screenPixelsWidth = Convert.ToInt32(this["screenPixelsWidth"], CultureInfo.InvariantCulture);
                    }
                    this._haveScreenPixelsWidth = true;
                }
                return this._screenPixelsWidth;
            }
        }

        public virtual bool SupportsAccesskeyAttribute
        {
            get
            {
                if (!this._haveSupportsAccesskeyAttribute)
                {
                    this._supportsAccesskeyAttribute = this.CapsParseBoolDefault("supportsAccesskeyAttribute", false);
                    this._haveSupportsAccesskeyAttribute = true;
                }
                return this._supportsAccesskeyAttribute;
            }
        }

        public virtual bool SupportsBodyColor
        {
            get
            {
                if (!this._haveSupportsBodyColor)
                {
                    this._supportsBodyColor = this.CapsParseBoolDefault("supportsBodyColor", false);
                    this._haveSupportsBodyColor = true;
                }
                return this._supportsBodyColor;
            }
        }

        public virtual bool SupportsBold
        {
            get
            {
                if (!this._haveSupportsBold)
                {
                    this._supportsBold = this.CapsParseBoolDefault("supportsBold", true);
                    this._haveSupportsBold = true;
                }
                return this._supportsBold;
            }
        }

        public virtual bool SupportsCacheControlMetaTag
        {
            get
            {
                if (!this._haveSupportsCacheControlMetaTag)
                {
                    this._supportsCacheControlMetaTag = this.CapsParseBoolDefault("supportsCacheControlMetaTag", true);
                    this._haveSupportsCacheControlMetaTag = true;
                }
                return this._supportsCacheControlMetaTag;
            }
        }

        public virtual bool SupportsCallback
        {
            get
            {
                if (!this._haveSupportsCallback)
                {
                    this._supportsCallback = this.CapsParseBoolDefault("supportsCallback", false);
                    this._haveSupportsCallback = true;
                }
                return this._supportsCallback;
            }
        }

        public virtual bool SupportsCss
        {
            get
            {
                if (!this._haveSupportsCss)
                {
                    this._supportsCss = this.CapsParseBoolDefault("supportsCss", false);
                    this._haveSupportsCss = true;
                }
                return this._supportsCss;
            }
        }

        public virtual bool SupportsDivAlign
        {
            get
            {
                if (!this._haveSupportsDivAlign)
                {
                    this._supportsDivAlign = this.CapsParseBoolDefault("supportsDivAlign", false);
                    this._haveSupportsDivAlign = true;
                }
                return this._supportsDivAlign;
            }
        }

        public virtual bool SupportsDivNoWrap
        {
            get
            {
                if (!this._haveSupportsDivNoWrap)
                {
                    this._supportsDivNoWrap = this.CapsParseBoolDefault("supportsDivNoWrap", false);
                    this._haveSupportsDivNoWrap = true;
                }
                return this._supportsDivNoWrap;
            }
        }

        public virtual bool SupportsEmptyStringInCookieValue
        {
            get
            {
                if (!this._haveSupportsEmptyStringInCookieValue)
                {
                    this._supportsEmptyStringInCookieValue = this.CapsParseBoolDefault("supportsEmptyStringInCookieValue", true);
                    this._haveSupportsEmptyStringInCookieValue = true;
                }
                return this._supportsEmptyStringInCookieValue;
            }
        }

        public virtual bool SupportsFontColor
        {
            get
            {
                if (!this._haveSupportsFontColor)
                {
                    this._supportsFontColor = this.CapsParseBoolDefault("supportsFontColor", false);
                    this._haveSupportsFontColor = true;
                }
                return this._supportsFontColor;
            }
        }

        public virtual bool SupportsFontName
        {
            get
            {
                if (!this._haveSupportsFontName)
                {
                    this._supportsFontName = this.CapsParseBoolDefault("supportsFontName", false);
                    this._haveSupportsFontName = true;
                }
                return this._supportsFontName;
            }
        }

        public virtual bool SupportsFontSize
        {
            get
            {
                if (!this._haveSupportsFontSize)
                {
                    this._supportsFontSize = this.CapsParseBoolDefault("supportsFontSize", false);
                    this._haveSupportsFontSize = true;
                }
                return this._supportsFontSize;
            }
        }

        public virtual bool SupportsImageSubmit
        {
            get
            {
                if (!this._haveSupportsImageSubmit)
                {
                    this._supportsImageSubmit = this.CapsParseBoolDefault("supportsImageSubmit", false);
                    this._haveSupportsImageSubmit = true;
                }
                return this._supportsImageSubmit;
            }
        }

        public virtual bool SupportsIModeSymbols
        {
            get
            {
                if (!this._haveSupportsIModeSymbols)
                {
                    this._supportsIModeSymbols = this.CapsParseBoolDefault("supportsIModeSymbols", false);
                    this._haveSupportsIModeSymbols = true;
                }
                return this._supportsIModeSymbols;
            }
        }

        public virtual bool SupportsInputIStyle
        {
            get
            {
                if (!this._haveSupportsInputIStyle)
                {
                    this._supportsInputIStyle = this.CapsParseBoolDefault("supportsInputIStyle", false);
                    this._haveSupportsInputIStyle = true;
                }
                return this._supportsInputIStyle;
            }
        }

        public virtual bool SupportsInputMode
        {
            get
            {
                if (!this._haveSupportsInputMode)
                {
                    this._supportsInputMode = this.CapsParseBoolDefault("supportsInputMode", false);
                    this._haveSupportsInputMode = true;
                }
                return this._supportsInputMode;
            }
        }

        public virtual bool SupportsItalic
        {
            get
            {
                if (!this._haveSupportsItalic)
                {
                    this._supportsItalic = this.CapsParseBoolDefault("supportsItalic", true);
                    this._haveSupportsItalic = true;
                }
                return this._supportsItalic;
            }
        }

        public virtual bool SupportsJPhoneMultiMediaAttributes
        {
            get
            {
                if (!this._haveSupportsJPhoneMultiMediaAttributes)
                {
                    this._supportsJPhoneMultiMediaAttributes = this.CapsParseBoolDefault("supportsJPhoneMultiMediaAttributes", false);
                    this._haveSupportsJPhoneMultiMediaAttributes = true;
                }
                return this._supportsJPhoneMultiMediaAttributes;
            }
        }

        public virtual bool SupportsJPhoneSymbols
        {
            get
            {
                if (!this._haveSupportsJPhoneSymbols)
                {
                    this._supportsJPhoneSymbols = this.CapsParseBoolDefault("supportsJPhoneSymbols", false);
                    this._haveSupportsJPhoneSymbols = true;
                }
                return this._supportsJPhoneSymbols;
            }
        }

        internal bool SupportsMaintainScrollPositionOnPostback
        {
            get
            {
                if (!this._haveSupportsMaintainScrollPositionOnPostback)
                {
                    this._supportsMaintainScrollPositionOnPostback = this.CapsParseBoolDefault("supportsMaintainScrollPositionOnPostback", false);
                    this._haveSupportsMaintainScrollPositionOnPostback = true;
                }
                return this._supportsMaintainScrollPositionOnPostback;
            }
        }

        public virtual bool SupportsQueryStringInFormAction
        {
            get
            {
                if (!this._haveSupportsQueryStringInFormAction)
                {
                    this._supportsQueryStringInFormAction = this.CapsParseBoolDefault("supportsQueryStringInFormAction", true);
                    this._haveSupportsQueryStringInFormAction = true;
                }
                return this._supportsQueryStringInFormAction;
            }
        }

        public virtual bool SupportsRedirectWithCookie
        {
            get
            {
                if (!this._haveSupportsRedirectWithCookie)
                {
                    this._supportsRedirectWithCookie = this.CapsParseBoolDefault("supportsRedirectWithCookie", true);
                    this._haveSupportsRedirectWithCookie = true;
                }
                return this._supportsRedirectWithCookie;
            }
        }

        public virtual bool SupportsSelectMultiple
        {
            get
            {
                if (!this._haveSupportsSelectMultiple)
                {
                    this._supportsSelectMultiple = this.CapsParseBoolDefault("supportsSelectMultiple", false);
                    this._haveSupportsSelectMultiple = true;
                }
                return this._supportsSelectMultiple;
            }
        }

        public virtual bool SupportsUncheck
        {
            get
            {
                if (!this._haveSupportsUncheck)
                {
                    this._supportsUncheck = this.CapsParseBoolDefault("supportsUncheck", true);
                    this._haveSupportsUncheck = true;
                }
                return this._supportsUncheck;
            }
        }

        public virtual bool SupportsXmlHttp
        {
            get
            {
                if (!this._haveSupportsXmlHttp)
                {
                    this._supportsXmlHttp = this.CapsParseBoolDefault("supportsXmlHttp", false);
                    this._haveSupportsXmlHttp = true;
                }
                return this._supportsXmlHttp;
            }
        }

        public bool Tables
        {
            get
            {
                if (!this._havetables)
                {
                    this._tables = this.CapsParseBool("tables");
                    this._havetables = true;
                }
                return this._tables;
            }
        }

        public System.Type TagWriter
        {
            get
            {
                try
                {
                    if (!this._havetagwriter)
                    {
                        string str = this["tagwriter"];
                        if (string.IsNullOrEmpty(str))
                        {
                            this._tagwriter = null;
                        }
                        else if (string.Compare(str, typeof(System.Web.UI.HtmlTextWriter).FullName, StringComparison.Ordinal) == 0)
                        {
                            this._tagwriter = typeof(System.Web.UI.HtmlTextWriter);
                        }
                        else
                        {
                            this._tagwriter = BuildManager.GetType(str, true);
                        }
                        this._havetagwriter = true;
                    }
                }
                catch (Exception exception)
                {
                    throw this.BuildParseError(exception, "tagwriter");
                }
                return this._tagwriter;
            }
        }

        public string Type
        {
            get
            {
                if (!this._havetype)
                {
                    this._type = this["type"];
                    this._havetype = true;
                }
                return this._type;
            }
        }

        public bool UseOptimizedCacheKey
        {
            get
            {
                return this._useOptimizedCacheKey;
            }
        }

        public bool VBScript
        {
            get
            {
                if (!this._havevbscript)
                {
                    this._vbscript = this.CapsParseBool("vbscript");
                    this._havevbscript = true;
                }
                return this._vbscript;
            }
        }

        public string Version
        {
            get
            {
                if (!this._haveversion)
                {
                    this._version = this["version"];
                    this._haveversion = true;
                }
                return this._version;
            }
        }

        public System.Version W3CDomVersion
        {
            get
            {
                if (!this._havew3cdomversion)
                {
                    this._w3cdomversion = new System.Version(this["w3cdomversion"]);
                    this._havew3cdomversion = true;
                }
                return this._w3cdomversion;
            }
        }

        public bool Win16
        {
            get
            {
                if (!this._havewin16)
                {
                    this._win16 = this.CapsParseBool("win16");
                    this._havewin16 = true;
                }
                return this._win16;
            }
        }

        public bool Win32
        {
            get
            {
                if (!this._havewin32)
                {
                    this._win32 = this.CapsParseBool("win32");
                    this._havewin32 = true;
                }
                return this._win32;
            }
        }
    }
}

