namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Configuration;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.RegularExpressions;
    using System.Web.Routing;
    using System.Web.SessionState;
    using System.Web.UI.Adapters;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.Util;
    using System.Xml;

    [DesignerSerializer("Microsoft.VisualStudio.Web.WebForms.WebFormCodeDomSerializer, Microsoft.VisualStudio.Web, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.TypeCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Designer("Microsoft.VisualStudio.Web.WebForms.WebFormDesigner, Microsoft.VisualStudio.Web, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IRootDesigner)), ToolboxItem(false), DefaultEvent("Load"), DesignerCategory("ASPXCodeBehind")]
    public class Page : TemplateControl, IHttpHandler
    {
        internal HttpApplicationState _application;
        private bool _aspCompatMode;
        private AspCompatApplicationStep _aspCompatStep;
        private PageAsyncInfo _asyncInfo;
        private bool _asyncMode;
        private PageAsyncTaskManager _asyncTaskManager;
        private TimeSpan _asyncTimeout;
        private bool _asyncTimeoutSet;
        private Control _autoPostBackControl;
        internal System.Web.Caching.Cache _cache;
        private bool _cachedRequestViewState;
        private ICallbackEventHandler _callbackControl;
        private ArrayList _changedPostDataConsumers;
        private string _clientQueryString;
        private ClientScriptManager _clientScriptManager;
        private string _clientState;
        private bool _clientSupportsJavaScript;
        private bool _clientSupportsJavaScriptChecked;
        private string _clientTarget;
        private bool _containsCrossPagePost;
        private bool _containsEncryptedViewState;
        private IDictionary _contentTemplateCollection;
        internal HttpContext _context;
        private ArrayList _controlsRequiringPostBack;
        private StringSet _controlStateLoadedControlIds;
        private Stack _dataBindingContext;
        private string _descriptionToBeSet;
        private bool _designMode;
        private bool _designModeChecked;
        private CultureInfo _dynamicCulture;
        private CultureInfo _dynamicUICulture;
        private ArrayList _enabledControls;
        private bool _enableEventValidation = true;
        private bool _enableViewStateMac;
        private System.Web.UI.ViewStateEncryptionMode _encryptionMode;
        internal string _errorPage;
        private Control _focusedControl;
        private string _focusedControlID;
        private bool _fOnFormRenderCalled;
        private HtmlForm _form;
        private bool _fPageLayoutChanged;
        private bool _fPostBackScriptRendered;
        private bool _fRequirePostBackScript;
        private bool _fRequireWebFormsScript;
        private bool _fWebFormsScriptRendered;
        private bool _haveIdSeparator;
        private HtmlHead _header;
        private char _idSeparator;
        private bool _inOnFormRender;
        private bool _isCallback;
        private bool _isCrossPagePostBack;
        private IDictionary _items;
        private string _keywordsToBeSet;
        private NameValueCollection _leftoverPostData;
        private bool _maintainScrollPosition;
        private MasterPage _master;
        private VirtualPath _masterPageFile;
        private int _maxPageStateFieldLength = DefaultMaxPageStateFieldLength;
        private bool _needToPersistViewState;
        private System.Web.UI.Adapters.PageAdapter _pageAdapter;
        private System.Web.Util.SimpleBitVector32 _pageFlags;
        private Stack _partialCachingControlStack;
        private System.Web.UI.PageStatePersister _persister;
        private RenderMethod _postFormRenderDelegate;
        private bool _preInitWorkComplete;
        private Page _previousPage;
        private VirtualPath _previousPagePath;
        private bool _profileTreeBuilt;
        internal HybridDictionary _registeredControlsRequiringClearChildControlState;
        internal ControlSet _registeredControlsRequiringControlState;
        private ArrayList _registeredControlsThatRequirePostBack;
        private IPostBackEventHandler _registeredControlThatRequireRaiseEvent;
        private string _relativeFilePath;
        internal HttpRequest _request;
        private NameValueCollection _requestValueCollection;
        private string _requestViewState;
        private bool _requireFocusScript;
        private bool _requireScrollScript;
        internal HttpResponse _response;
        private static Type _scriptManagerType;
        private int _scrollPositionX;
        private const string _scrollPositionXID = "__SCROLLPOSITIONX";
        private int _scrollPositionY;
        private const string _scrollPositionYID = "__SCROLLPOSITIONY";
        private HttpSessionState _session;
        private bool _sessionRetrieved;
        private SmartNavigationSupport _smartNavSupport;
        private PageTheme _styleSheet;
        private string _styleSheetName;
        private int _supportsStyleSheets;
        private PageTheme _theme;
        private string _themeName;
        private string _titleToBeSet;
        private int _transactionMode;
        private string _uniqueFilePathSuffix;
        private bool _validated;
        private string _validatorInvalidControl;
        private ValidatorCollection _validators;
        private bool _viewStateEncryptionRequested;
        private string _viewStateUserKey;
        private System.Web.Configuration.XhtmlConformanceMode _xhtmlConformanceMode;
        private bool _xhtmlConformanceModeSet;
        internal const bool BufferDefault = true;
        internal const string callbackID = "__CALLBACKID";
        internal const string callbackIndexID = "__CALLBACKINDEX";
        internal const string callbackLoadScriptID = "__CALLBACKLOADSCRIPT";
        internal const string callbackParameterID = "__CALLBACKPARAM";
        internal static readonly int DefaultAsyncTimeoutSeconds = 0x2d;
        internal static readonly int DefaultMaxPageStateFieldLength = -1;
        private const string EnabledControlArray = "__enabledControlArray";
        internal const bool EnableEventValidationDefault = true;
        internal const bool EnableViewStateMacDefault = true;
        internal const System.Web.UI.ViewStateEncryptionMode EncryptionModeDefault = System.Web.UI.ViewStateEncryptionMode.Auto;
        internal static readonly object EventInitComplete = new object();
        internal static readonly object EventLoadComplete = new object();
        internal static readonly object EventPreInit = new object();
        internal static readonly object EventPreLoad = new object();
        internal static readonly object EventPreRenderComplete = new object();
        internal static readonly object EventSaveStateComplete = new object();
        internal const string EventValidationPrefixID = "__EVENTVALIDATION";
        private static readonly Version FocusMinimumEcmaVersion = new Version("1.4");
        private static readonly Version FocusMinimumJScriptVersion = new Version("3.0");
        private const string HiddenClassName = "aspNetHidden";
        private const int isCrossPagePostRequest = 8;
        private const int isExportingWebPart = 2;
        private const int isExportingWebPartShared = 4;
        private const int isPartialRenderingSupported = 0x10;
        private const int isPartialRenderingSupportedSet = 0x20;
        private static readonly Version JavascriptMinimumVersion = new Version("1.0");
        private const string lastFocusID = "__LASTFOCUS";
        internal const bool MaintainScrollPositionOnPostBackDefault = false;
        private static readonly Version MSDomScrollMinimumVersion = new Version("4.0");
        private const string PageID = "__Page";
        private const string PageReEnableControlsScriptKey = "PageReEnableControlsScript";
        private const string PageRegisteredControlsThatRequirePostBackKey = "__ControlsRequirePostBackKey__";
        private const string PageScrollPositionScriptKey = "PageScrollPositionScript";
        private const string PageSubmitScriptKey = "PageSubmitScript";
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string postEventArgumentID = "__EVENTARGUMENT";
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string postEventSourceID = "__EVENTTARGET";
        internal const string previousPageID = "__PREVIOUSPAGE";
        private static StringSet s_systemPostFields = new StringSet();
        private static char[] s_varySeparator = new char[] { ';' };
        internal const bool SmartNavigationDefault = false;
        private const int styleSheetInitialized = 1;
        internal const string systemPostFieldPrefix = "__";
        private static readonly string UniqueFilePathSuffixID = "__ufps";
        internal const string ViewStateEncryptionID = "__VIEWSTATEENCRYPTED";
        internal const string ViewStateFieldCountID = "__VIEWSTATEFIELDCOUNT";
        internal const string ViewStateFieldPrefixID = "__VIEWSTATE";
        internal const string WebPartExportID = "__WEBPARTEXPORT";

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler InitComplete
        {
            add
            {
                base.Events.AddHandler(EventInitComplete, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventInitComplete, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler LoadComplete
        {
            add
            {
                base.Events.AddHandler(EventLoadComplete, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLoadComplete, value);
            }
        }

        public event EventHandler PreInit
        {
            add
            {
                base.Events.AddHandler(EventPreInit, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPreInit, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler PreLoad
        {
            add
            {
                base.Events.AddHandler(EventPreLoad, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPreLoad, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler PreRenderComplete
        {
            add
            {
                base.Events.AddHandler(EventPreRenderComplete, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPreRenderComplete, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler SaveStateComplete
        {
            add
            {
                base.Events.AddHandler(EventSaveStateComplete, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSaveStateComplete, value);
            }
        }

        static Page()
        {
            s_systemPostFields.Add("__EVENTTARGET");
            s_systemPostFields.Add("__EVENTARGUMENT");
            s_systemPostFields.Add("__VIEWSTATEFIELDCOUNT");
            s_systemPostFields.Add("__VIEWSTATE");
            s_systemPostFields.Add("__VIEWSTATEENCRYPTED");
            s_systemPostFields.Add("__PREVIOUSPAGE");
            s_systemPostFields.Add("__CALLBACKID");
            s_systemPostFields.Add("__CALLBACKPARAM");
            s_systemPostFields.Add("__LASTFOCUS");
            s_systemPostFields.Add(UniqueFilePathSuffixID);
            s_systemPostFields.Add(HttpResponse.RedirectQueryStringVariable);
            s_systemPostFields.Add("__EVENTVALIDATION");
        }

        public Page()
        {
            base._page = this;
            this._enableViewStateMac = true;
            this.ID = "__Page";
            this._supportsStyleSheets = -1;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected internal void AddContentTemplate(string templateName, ITemplate template)
        {
            if (this._contentTemplateCollection == null)
            {
                this._contentTemplateCollection = new Hashtable(11, StringComparer.OrdinalIgnoreCase);
            }
            try
            {
                this._contentTemplateCollection.Add(templateName, template);
            }
            catch (ArgumentException)
            {
                throw new HttpException(System.Web.SR.GetString("MasterPage_Multiple_content", new object[] { templateName }));
            }
        }

        public void AddOnPreRenderCompleteAsync(BeginEventHandler beginHandler, EndEventHandler endHandler)
        {
            this.AddOnPreRenderCompleteAsync(beginHandler, endHandler, null);
        }

        public void AddOnPreRenderCompleteAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            if (beginHandler == null)
            {
                throw new ArgumentNullException("beginHandler");
            }
            if (endHandler == null)
            {
                throw new ArgumentNullException("endHandler");
            }
            if (this._asyncInfo == null)
            {
                if (!(this is IHttpAsyncHandler))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Async_required"));
                }
                this._asyncInfo = new PageAsyncInfo(this);
            }
            if (this._asyncInfo.AsyncPointReached)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Async_addhandler_too_late"));
            }
            this._asyncInfo.AddHandler(beginHandler, endHandler, state);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected internal void AddWrappedFileDependencies(object virtualFileDependencies)
        {
            this.Response.AddVirtualPathDependencies((string[]) virtualFileDependencies);
        }

        internal void ApplyControlSkin(Control ctrl)
        {
            if (this._theme != null)
            {
                this._theme.ApplyControlSkin(ctrl);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal bool ApplyControlStyleSheet(Control ctrl)
        {
            if (this._styleSheet != null)
            {
                this._styleSheet.ApplyControlSkin(ctrl);
                return true;
            }
            return false;
        }

        private void ApplyMasterPage()
        {
            if (this.Master != null)
            {
                ArrayList appliedMasterFilePaths = new ArrayList();
                appliedMasterFilePaths.Add(this._masterPageFile.VirtualPathString.ToLower(CultureInfo.InvariantCulture));
                MasterPage.ApplyMasterRecursive(this.Master, appliedMasterFilePaths);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IAsyncResult AspCompatBeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            this.SetIntrinsics(context);
            this._aspCompatStep = new AspCompatApplicationStep(context, new AspCompatCallback(this.ProcessRequest));
            return this._aspCompatStep.BeginAspCompatExecution(cb, extraData);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void AspCompatEndProcessRequest(IAsyncResult result)
        {
            this._aspCompatStep.EndAspCompatExecution(result);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IAsyncResult AsyncPageBeginProcessRequest(HttpContext context, AsyncCallback callback, object extraData)
        {
            this.SetIntrinsics(context, true);
            if (this._asyncInfo == null)
            {
                this._asyncInfo = new PageAsyncInfo(this);
            }
            this._asyncInfo.AsyncResult = new HttpAsyncResult(callback, extraData);
            this._asyncInfo.CallerIsBlocking = callback == null;
            try
            {
                this._context.InvokeCancellableCallback(new WaitCallback(this.AsyncPageProcessRequestBeforeAsyncPointCancellableCallback), null);
            }
            catch (Exception exception)
            {
                if (this._context.SyncContext.PendingOperationsCount == 0)
                {
                    throw;
                }
                this._asyncInfo.SetError(exception);
            }
            if ((this._asyncTaskManager != null) && !this._asyncInfo.CallerIsBlocking)
            {
                this._asyncTaskManager.RegisterHandlersForPagePreRenderCompleteAsync();
            }
            this._asyncInfo.AsyncPointReached = true;
            this._context.SyncContext.Disable();
            this._asyncInfo.CallHandlers(true);
            return this._asyncInfo.AsyncResult;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void AsyncPageEndProcessRequest(IAsyncResult result)
        {
            if (this._asyncInfo != null)
            {
                this._asyncInfo.AsyncResult.End();
            }
        }

        private void AsyncPageProcessRequestBeforeAsyncPointCancellableCallback(object state)
        {
            this.ProcessRequest(true, false);
        }

        internal void BeginFormRender(HtmlTextWriter writer, string formUniqueID)
        {
            bool flag = this.RenderDivAroundHiddenInputs(writer);
            if (flag)
            {
                writer.WriteLine();
                if (this.RenderingCompatibility >= VersionUtil.Framework40)
                {
                    writer.Write("<div class=\"aspNetHidden\">");
                }
                else
                {
                    writer.Write("<div>");
                }
            }
            this.ClientScript.RenderHiddenFields(writer);
            this.RenderViewStateFields(writer);
            if (flag)
            {
                writer.WriteLine("</div>");
            }
            if (this.ClientSupportsJavaScript)
            {
                if (this.MaintainScrollPositionOnPostBack && !this._requireScrollScript)
                {
                    this.ClientScript.RegisterHiddenField("__SCROLLPOSITIONX", this._scrollPositionX.ToString(CultureInfo.InvariantCulture));
                    this.ClientScript.RegisterHiddenField("__SCROLLPOSITIONY", this._scrollPositionY.ToString(CultureInfo.InvariantCulture));
                    this.ClientScript.RegisterStartupScript(typeof(Page), "PageScrollPositionScript", "\r\ntheForm.oldSubmit = theForm.submit;\r\ntheForm.submit = WebForm_SaveScrollPositionSubmit;\r\n\r\ntheForm.oldOnSubmit = theForm.onsubmit;\r\ntheForm.onsubmit = WebForm_SaveScrollPositionOnSubmit;\r\n" + (this.IsPostBack ? "\r\ntheForm.oldOnLoad = window.onload;\r\nwindow.onload = WebForm_RestoreScrollPosition;\r\n" : string.Empty), true);
                    this.RegisterWebFormsScript();
                    this._requireScrollScript = true;
                }
                if ((this.ClientSupportsFocus && (this.Form != null)) && ((this.RenderFocusScript || (this.Form.DefaultFocus.Length > 0)) || (this.Form.DefaultButton.Length > 0)))
                {
                    int num;
                    string s = string.Empty;
                    if (this.FocusedControlID.Length > 0)
                    {
                        s = this.FocusedControlID;
                    }
                    else if (this.FocusedControl != null)
                    {
                        if (this.FocusedControl.Visible)
                        {
                            s = this.FocusedControl.ClientID;
                        }
                    }
                    else if (this.ValidatorInvalidControl.Length > 0)
                    {
                        s = this.ValidatorInvalidControl;
                    }
                    else if (this.LastFocusedControl.Length > 0)
                    {
                        s = this.LastFocusedControl;
                    }
                    else if (this.Form.DefaultFocus.Length > 0)
                    {
                        s = this.Form.DefaultFocus;
                    }
                    else if (this.Form.DefaultButton.Length > 0)
                    {
                        s = this.Form.DefaultButton;
                    }
                    if (((s.Length > 0) && !CrossSiteScriptingValidation.IsDangerousString(s, out num)) && CrossSiteScriptingValidation.IsValidJavascriptId(s))
                    {
                        this.ClientScript.RegisterClientScriptResource(typeof(HtmlForm), "Focus.js");
                        if (!this.ClientScript.IsClientScriptBlockRegistered(typeof(HtmlForm), "Focus"))
                        {
                            this.RegisterWebFormsScript();
                            this.ClientScript.RegisterStartupScript(typeof(HtmlForm), "Focus", "WebForm_AutoFocus('" + System.Web.UI.Util.QuoteJScriptString(s) + "');", true);
                        }
                        IScriptManager scriptManager = this.ScriptManager;
                        if (scriptManager != null)
                        {
                            scriptManager.SetFocusInternal(s);
                        }
                    }
                }
                if ((this.Form.SubmitDisabledControls && (this.EnabledControls.Count > 0)) && (this._request.Browser.W3CDomVersion.Major > 0))
                {
                    foreach (Control control in this.EnabledControls)
                    {
                        this.ClientScript.RegisterArrayDeclaration("__enabledControlArray", "'" + control.ClientID + "'");
                    }
                    this.ClientScript.RegisterOnSubmitStatement(typeof(Page), "PageReEnableControlsScript", "WebForm_ReEnableControls();");
                    this.RegisterWebFormsScript();
                }
                if (this._fRequirePostBackScript)
                {
                    this.RenderPostBackScript(writer, formUniqueID);
                }
                if (this._fRequireWebFormsScript)
                {
                    this.RenderWebFormsScript(writer);
                }
            }
            this.ClientScript.RenderClientScriptBlocks(writer);
        }

        private void BuildPageProfileTree(bool enableViewState)
        {
            if (!this._profileTreeBuilt)
            {
                this._profileTreeBuilt = true;
                base.BuildProfileTree("ROOT", enableViewState);
            }
        }

        private void CheckRemainingAsyncTasks(bool isThreadAbort)
        {
            if (this._asyncTaskManager != null)
            {
                this._asyncTaskManager.DisposeTimer();
                if (isThreadAbort)
                {
                    this._asyncTaskManager.CompleteAllTasksNow(true);
                }
                else if (!this._asyncTaskManager.FailedToStartTasks && this._asyncTaskManager.AnyTasksRemain)
                {
                    throw new HttpException(System.Web.SR.GetString("Registered_async_tasks_remain"));
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual HtmlTextWriter CreateHtmlTextWriter(TextWriter tw)
        {
            if (((this.Context != null) && (this.Context.Request != null)) && (this.Context.Request.Browser != null))
            {
                return this.Context.Request.Browser.CreateHtmlTextWriter(tw);
            }
            HtmlTextWriter writer = CreateHtmlTextWriterInternal(tw, this._request);
            if (writer == null)
            {
                writer = new HtmlTextWriter(tw);
            }
            return writer;
        }

        public static HtmlTextWriter CreateHtmlTextWriterFromType(TextWriter tw, Type writerType)
        {
            HtmlTextWriter writer;
            if (writerType == typeof(HtmlTextWriter))
            {
                return new HtmlTextWriter(tw);
            }
            if (writerType == typeof(Html32TextWriter))
            {
                return new Html32TextWriter(tw);
            }
            try
            {
                System.Web.UI.Util.CheckAssignableType(typeof(HtmlTextWriter), writerType);
                writer = (HtmlTextWriter) HttpRuntime.CreateNonPublicInstance(writerType, new object[] { tw });
            }
            catch
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_HtmlTextWriter", new object[] { writerType.FullName }));
            }
            return writer;
        }

        internal static HtmlTextWriter CreateHtmlTextWriterInternal(TextWriter tw, HttpRequest request)
        {
            if ((request != null) && (request.Browser != null))
            {
                return request.Browser.CreateHtmlTextWriterInternal(tw);
            }
            return new Html32TextWriter(tw);
        }

        internal IStateFormatter CreateStateFormatter()
        {
            return new ObjectStateFormatter(this, true);
        }

        private CultureInfo CultureFromUserLanguages(bool specific)
        {
            if (((this._context != null) && (this._context.Request != null)) && ((this._context.Request.UserLanguages != null) && (this._context.Request.UserLanguages[0] != null)))
            {
                try
                {
                    string name = this._context.Request.UserLanguages[0];
                    int index = name.IndexOf(';');
                    if (index != -1)
                    {
                        name = name.Substring(0, index);
                    }
                    if (specific)
                    {
                        return HttpServerUtility.CreateReadOnlySpecificCultureInfo(name);
                    }
                    return HttpServerUtility.CreateReadOnlyCultureInfo(name);
                }
                catch
                {
                }
            }
            return null;
        }

        internal ICollection DecomposeViewStateIntoChunks()
        {
            string clientState = this.ClientState;
            if (clientState == null)
            {
                return null;
            }
            if (this.MaxPageStateFieldLength <= 0)
            {
                ArrayList list = new ArrayList(1);
                list.Add(clientState);
                return list;
            }
            int num = this.ClientState.Length / this.MaxPageStateFieldLength;
            ArrayList list2 = new ArrayList(num + 1);
            int startIndex = 0;
            for (int i = 0; i < num; i++)
            {
                list2.Add(clientState.Substring(startIndex, this.MaxPageStateFieldLength));
                startIndex += this.MaxPageStateFieldLength;
            }
            if (startIndex < clientState.Length)
            {
                list2.Add(clientState.Substring(startIndex));
            }
            if (list2.Count == 0)
            {
                list2.Add(string.Empty);
            }
            return list2;
        }

        internal static string DecryptString(string s)
        {
            return DecryptStringWithIV(s, IVType.Hash);
        }

        internal static string DecryptStringWithIV(string s, IVType ivType)
        {
            if (s == null)
            {
                return null;
            }
            byte[] buf = HttpServerUtility.UrlTokenDecode(s);
            if (buf != null)
            {
                buf = MachineKeySection.EncryptOrDecryptData(false, buf, null, 0, buf.Length, false, false, ivType);
            }
            if (buf == null)
            {
                throw new HttpException(System.Web.SR.GetString("ViewState_InvalidViewState"));
            }
            return Encoding.UTF8.GetString(buf);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DesignerInitialize()
        {
            this.InitRecursive(null);
        }

        private bool DetermineIsExportingWebPart()
        {
            byte[] queryStringBytes = this.Request.QueryStringBytes;
            if ((queryStringBytes == null) || (queryStringBytes.Length < 0x1c))
            {
                return false;
            }
            if (((((queryStringBytes[0] != 0x5f) || (queryStringBytes[1] != 0x5f)) || ((queryStringBytes[2] != 0x57) || (queryStringBytes[3] != 0x45))) || (((queryStringBytes[4] != 0x42) || (queryStringBytes[5] != 80)) || ((queryStringBytes[6] != 0x41) || (queryStringBytes[7] != 0x52)))) || (((((queryStringBytes[8] != 0x54) || (queryStringBytes[9] != 0x45)) || ((queryStringBytes[10] != 0x58) || (queryStringBytes[11] != 80))) || (((queryStringBytes[12] != 0x4f) || (queryStringBytes[13] != 0x52)) || ((queryStringBytes[14] != 0x54) || (queryStringBytes[15] != 0x3d)))) || (((queryStringBytes[0x10] != 0x74) || (queryStringBytes[0x11] != 0x72)) || (((queryStringBytes[0x12] != 0x75) || (queryStringBytes[0x13] != 0x65)) || (queryStringBytes[20] != 0x26)))))
            {
                return false;
            }
            this._pageFlags.Set(2);
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual NameValueCollection DeterminePostBackMode()
        {
            if (this.Context.Request == null)
            {
                return null;
            }
            if (this.Context.PreventPostback)
            {
                return null;
            }
            NameValueCollection collectionBasedOnMethod = this.GetCollectionBasedOnMethod(false);
            if (collectionBasedOnMethod == null)
            {
                return null;
            }
            bool flag = false;
            string[] values = collectionBasedOnMethod.GetValues((string) null);
            if (values != null)
            {
                int length = values.Length;
                for (int i = 0; i < length; i++)
                {
                    if (values[i].StartsWith("__VIEWSTATE", StringComparison.Ordinal) || (values[i] == "__EVENTTARGET"))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (((collectionBasedOnMethod["__VIEWSTATE"] == null) && (collectionBasedOnMethod["__VIEWSTATEFIELDCOUNT"] == null)) && ((collectionBasedOnMethod["__EVENTTARGET"] == null) && !flag))
            {
                return null;
            }
            if (this.Request.QueryStringText.IndexOf(HttpResponse.RedirectQueryStringAssignment, StringComparison.Ordinal) != -1)
            {
                collectionBasedOnMethod = null;
            }
            return collectionBasedOnMethod;
        }

        internal static string EncryptString(string s)
        {
            return EncryptStringWithIV(s, IVType.Hash);
        }

        internal static string EncryptStringWithIV(string s, IVType ivType)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return HttpServerUtility.UrlTokenEncode(MachineKeySection.EncryptOrDecryptData(true, bytes, null, 0, bytes.Length, false, false, ivType));
        }

        internal void EndFormRender(HtmlTextWriter writer, string formUniqueID)
        {
            this.EndFormRenderArrayAndExpandoAttribute(writer, formUniqueID);
            this.EndFormRenderHiddenFields(writer, formUniqueID);
            this.EndFormRenderPostBackAndWebFormsScript(writer, formUniqueID);
        }

        internal void EndFormRenderArrayAndExpandoAttribute(HtmlTextWriter writer, string formUniqueID)
        {
            if (this.ClientSupportsJavaScript)
            {
                this.ClientScript.RenderArrayDeclares(writer);
                this.ClientScript.RenderExpandoAttribute(writer);
            }
        }

        internal void EndFormRenderHiddenFields(HtmlTextWriter writer, string formUniqueID)
        {
            if (this.RequiresViewStateEncryptionInternal)
            {
                this.ClientScript.RegisterHiddenField("__VIEWSTATEENCRYPTED", string.Empty);
            }
            if (this._containsCrossPagePost)
            {
                string hiddenFieldInitialValue = EncryptString(this.Request.CurrentExecutionFilePath);
                this.ClientScript.RegisterHiddenField("__PREVIOUSPAGE", hiddenFieldInitialValue);
            }
            if (this.EnableEventValidation)
            {
                this.ClientScript.SaveEventValidationField();
            }
            if (this.ClientScript.HasRegisteredHiddenFields)
            {
                bool flag = this.RenderDivAroundHiddenInputs(writer);
                if (flag)
                {
                    writer.WriteLine();
                    if (this.RenderingCompatibility >= VersionUtil.Framework40)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "aspNetHidden");
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                }
                this.ClientScript.RenderHiddenFields(writer);
                if (flag)
                {
                    writer.RenderEndTag();
                }
            }
        }

        internal void EndFormRenderPostBackAndWebFormsScript(HtmlTextWriter writer, string formUniqueID)
        {
            if (this.ClientSupportsJavaScript)
            {
                if (this._fRequirePostBackScript && !this._fPostBackScriptRendered)
                {
                    this.RenderPostBackScript(writer, formUniqueID);
                }
                if (this._fRequireWebFormsScript && !this._fWebFormsScriptRendered)
                {
                    this.RenderWebFormsScript(writer);
                }
            }
            this.ClientScript.RenderClientStartupScripts(writer);
        }

        public void ExecuteRegisteredAsyncTasks()
        {
            if ((this._asyncTaskManager != null) && !this._asyncTaskManager.TaskExecutionInProgress)
            {
                HttpAsyncResult result = this._asyncTaskManager.ExecuteTasks(null, null);
                if (result.Error != null)
                {
                    throw new HttpException(null, result.Error);
                }
            }
        }

        private void ExportWebPart(string exportedWebPartID)
        {
            WebPart webPart = null;
            WebPartManager currentWebPartManager = WebPartManager.GetCurrentWebPartManager(this);
            if (currentWebPartManager != null)
            {
                webPart = currentWebPartManager.WebParts[exportedWebPartID];
            }
            if (((webPart == null) || webPart.IsClosed) || (webPart is ProxyWebPart))
            {
                this.Response.Redirect(this.Request.RawUrl, false);
            }
            else
            {
                this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                this.Response.Expires = 0;
                this.Response.ContentType = "application/mswebpart";
                string displayTitle = webPart.DisplayTitle;
                if (string.IsNullOrEmpty(displayTitle))
                {
                    displayTitle = System.Web.SR.GetString("Part_Untitled");
                }
                NonWordRegex regex = new NonWordRegex();
                this.Response.AddHeader("content-disposition", "attachment; filename=" + regex.Replace(displayTitle, "") + ".WebPart");
                using (XmlTextWriter writer = new XmlTextWriter(this.Response.Output))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    currentWebPartManager.ExportWebPart(webPart, writer);
                    writer.WriteEndDocument();
                }
            }
        }

        public override Control FindControl(string id)
        {
            if (System.Web.Util.StringUtil.EqualsIgnoreCase(id, "__Page"))
            {
                return this;
            }
            return base.FindControl(id, 0);
        }

        protected override void FrameworkInitialize()
        {
            base.FrameworkInitialize();
            this.InitializeStyleSheet();
        }

        internal NameValueCollection GetCollectionBasedOnMethod(bool dontReturnNull)
        {
            if (this._request.HttpVerb == HttpVerb.POST)
            {
                if (!dontReturnNull && !this._request.HasForm)
                {
                    return null;
                }
                return this._request.Form;
            }
            if (!dontReturnNull && !this._request.HasQueryString)
            {
                return null;
            }
            return this._request.QueryString;
        }

        public object GetDataItem()
        {
            if ((this._dataBindingContext == null) || (this._dataBindingContext.Count == 0))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Page_MissingDataBindingContext"));
            }
            return this._dataBindingContext.Peek();
        }

        internal bool GetDesignModeInternal()
        {
            if (!this._designModeChecked)
            {
                this._designMode = (base.Site != null) ? base.Site.DesignMode : false;
                this._designModeChecked = true;
            }
            return this._designMode;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Obsolete("The recommended alternative is ClientScript.GetPostBackEventReference. http://go.microsoft.com/fwlink/?linkid=14202")]
        public string GetPostBackClientEvent(Control control, string argument)
        {
            return this.ClientScript.GetPostBackEventReference(control, argument);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Obsolete("The recommended alternative is ClientScript.GetPostBackClientHyperlink. http://go.microsoft.com/fwlink/?linkid=14202")]
        public string GetPostBackClientHyperlink(Control control, string argument)
        {
            return this.ClientScript.GetPostBackClientHyperlink(control, argument, false);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Obsolete("The recommended alternative is ClientScript.GetPostBackEventReference. http://go.microsoft.com/fwlink/?linkid=14202")]
        public string GetPostBackEventReference(Control control)
        {
            return this.ClientScript.GetPostBackEventReference(control, string.Empty);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Obsolete("The recommended alternative is ClientScript.GetPostBackEventReference. http://go.microsoft.com/fwlink/?linkid=14202")]
        public string GetPostBackEventReference(Control control, string argument)
        {
            return this.ClientScript.GetPostBackEventReference(control, argument);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual int GetTypeHashCode()
        {
            return 0;
        }

        internal override string GetUniqueIDPrefix()
        {
            if (this.Parent == null)
            {
                return string.Empty;
            }
            return base.GetUniqueIDPrefix();
        }

        public ValidatorCollection GetValidators(string validationGroup)
        {
            if (validationGroup == null)
            {
                validationGroup = string.Empty;
            }
            ValidatorCollection validators = new ValidatorCollection();
            if (this._validators != null)
            {
                for (int i = 0; i < this.Validators.Count; i++)
                {
                    BaseValidator validator = this.Validators[i] as BaseValidator;
                    if (validator != null)
                    {
                        if (string.Compare(validator.ValidationGroup, validationGroup, StringComparison.Ordinal) == 0)
                        {
                            validators.Add(validator);
                        }
                    }
                    else if (validationGroup.Length == 0)
                    {
                        validators.Add(this.Validators[i]);
                    }
                }
            }
            return validators;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected object GetWrappedFileDependencies(string[] virtualFileDependencies)
        {
            return virtualFileDependencies;
        }

        private bool HandleError(Exception e)
        {
            try
            {
                this.Context.TempError = e;
                this.OnError(EventArgs.Empty);
                if (this.Context.TempError == null)
                {
                    return true;
                }
            }
            finally
            {
                this.Context.TempError = null;
            }
            if (!string.IsNullOrEmpty(this._errorPage) && this.Context.IsCustomErrorEnabled)
            {
                this._response.RedirectToErrorPage(this._errorPage, CustomErrorsSection.GetSettings(this.Context).RedirectMode);
                return true;
            }
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_UNHANDLED);
            string postMessage = null;
            if (this.Context.TraceIsEnabled)
            {
                this.Trace.Warn(System.Web.SR.GetString("Unhandled_Err_Error"), null, e);
                if (this.Trace.PageOutput)
                {
                    StringWriter writer = new StringWriter();
                    HtmlTextWriter output = new HtmlTextWriter(writer);
                    this.BuildPageProfileTree(false);
                    this.Trace.EndRequest();
                    this.Trace.StopTracing();
                    this.Trace.StatusCode = 500;
                    this.Trace.Render(output);
                    postMessage = writer.ToString();
                }
            }
            if ((HttpException.GetErrorFormatter(e) == null) && !(e is SecurityException))
            {
                throw new HttpUnhandledException(null, postMessage, e);
            }
            return false;
        }

        protected virtual void InitializeCulture()
        {
        }

        internal void InitializeStyleSheet()
        {
            if (!this._pageFlags[1])
            {
                string styleSheetTheme = this.StyleSheetTheme;
                if (!string.IsNullOrEmpty(styleSheetTheme))
                {
                    BuildResultCompiledType themeBuildResultType = ThemeDirectoryCompiler.GetThemeBuildResultType(this.Context, styleSheetTheme);
                    if (themeBuildResultType == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Page_theme_not_found", new object[] { styleSheetTheme }));
                    }
                    this._styleSheet = (PageTheme) themeBuildResultType.CreateInstance();
                    this._styleSheet.Initialize(this, true);
                }
                this._pageFlags.Set(1);
            }
        }

        private void InitializeThemes()
        {
            string theme = this.Theme;
            if (!string.IsNullOrEmpty(theme))
            {
                BuildResultCompiledType themeBuildResultType = ThemeDirectoryCompiler.GetThemeBuildResultType(this.Context, theme);
                if (themeBuildResultType == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Page_theme_not_found", new object[] { theme }));
                }
                this._theme = (PageTheme) themeBuildResultType.CreateInstance();
                this._theme.Initialize(this, false);
            }
        }

        private void InitializeWriter(HtmlTextWriter writer)
        {
            Html32TextWriter writer2 = writer as Html32TextWriter;
            if ((writer2 != null) && (this.Request.Browser != null))
            {
                writer2.ShouldPerformDivTableSubstitution = this.Request.Browser.Tables;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected internal virtual void InitOutputCache(OutputCacheParameters cacheSettings)
        {
            if (!this._isCrossPagePostBack)
            {
                OutputCacheProfile profile = null;
                HttpCachePolicy cache = this.Response.Cache;
                OutputCacheLocation any = ~OutputCacheLocation.Any;
                int seconds = 0;
                string varyByContentEncoding = null;
                string varyByHeader = null;
                string varyByCustom = null;
                string varyByParam = null;
                string sqlDependency = null;
                string varyByControl = null;
                bool noStore = false;
                RuntimeConfig appConfig = RuntimeConfig.GetAppConfig();
                if (appConfig.OutputCache.EnableOutputCache)
                {
                    HttpCacheability @public;
                    if ((cacheSettings.CacheProfile != null) && (cacheSettings.CacheProfile.Length != 0))
                    {
                        profile = appConfig.OutputCacheSettings.OutputCacheProfiles[cacheSettings.CacheProfile];
                        if (profile == null)
                        {
                            throw new HttpException(System.Web.SR.GetString("CacheProfile_Not_Found", new object[] { cacheSettings.CacheProfile }));
                        }
                        if (!profile.Enabled)
                        {
                            return;
                        }
                    }
                    if (profile != null)
                    {
                        seconds = profile.Duration;
                        varyByContentEncoding = profile.VaryByContentEncoding;
                        varyByHeader = profile.VaryByHeader;
                        varyByCustom = profile.VaryByCustom;
                        varyByParam = profile.VaryByParam;
                        sqlDependency = profile.SqlDependency;
                        noStore = profile.NoStore;
                        varyByControl = profile.VaryByControl;
                        any = profile.Location;
                        if (string.IsNullOrEmpty(varyByContentEncoding))
                        {
                            varyByContentEncoding = null;
                        }
                        if (string.IsNullOrEmpty(varyByHeader))
                        {
                            varyByHeader = null;
                        }
                        if (string.IsNullOrEmpty(varyByCustom))
                        {
                            varyByCustom = null;
                        }
                        if (string.IsNullOrEmpty(varyByParam))
                        {
                            varyByParam = null;
                        }
                        if (string.IsNullOrEmpty(varyByControl))
                        {
                            varyByControl = null;
                        }
                        if (System.Web.Util.StringUtil.EqualsIgnoreCase(varyByParam, "none"))
                        {
                            varyByParam = null;
                        }
                        if (System.Web.Util.StringUtil.EqualsIgnoreCase(varyByControl, "none"))
                        {
                            varyByControl = null;
                        }
                    }
                    if (cacheSettings.IsParameterSet(OutputCacheParameter.Duration))
                    {
                        seconds = cacheSettings.Duration;
                    }
                    if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByContentEncoding))
                    {
                        varyByContentEncoding = cacheSettings.VaryByContentEncoding;
                    }
                    if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByHeader))
                    {
                        varyByHeader = cacheSettings.VaryByHeader;
                    }
                    if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByCustom))
                    {
                        varyByCustom = cacheSettings.VaryByCustom;
                    }
                    if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByControl))
                    {
                        varyByControl = cacheSettings.VaryByControl;
                    }
                    if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByParam))
                    {
                        varyByParam = cacheSettings.VaryByParam;
                    }
                    if (cacheSettings.IsParameterSet(OutputCacheParameter.SqlDependency))
                    {
                        sqlDependency = cacheSettings.SqlDependency;
                    }
                    if (cacheSettings.IsParameterSet(OutputCacheParameter.NoStore))
                    {
                        noStore = cacheSettings.NoStore;
                    }
                    if (cacheSettings.IsParameterSet(OutputCacheParameter.Location))
                    {
                        any = cacheSettings.Location;
                    }
                    if (any == ~OutputCacheLocation.Any)
                    {
                        any = OutputCacheLocation.Any;
                    }
                    if ((any != OutputCacheLocation.None) && ((profile == null) || profile.Enabled))
                    {
                        if (((profile == null) || (profile.Duration == -1)) && !cacheSettings.IsParameterSet(OutputCacheParameter.Duration))
                        {
                            throw new HttpException(System.Web.SR.GetString("Missing_output_cache_attr", new object[] { "duration" }));
                        }
                        if (((profile == null) || ((profile.VaryByParam == null) && (profile.VaryByControl == null))) && (!cacheSettings.IsParameterSet(OutputCacheParameter.VaryByParam) && !cacheSettings.IsParameterSet(OutputCacheParameter.VaryByControl)))
                        {
                            throw new HttpException(System.Web.SR.GetString("Missing_output_cache_attr", new object[] { "varyByParam" }));
                        }
                    }
                    if (noStore)
                    {
                        this.Response.Cache.SetNoStore();
                    }
                    switch (any)
                    {
                        case OutputCacheLocation.Any:
                            @public = HttpCacheability.Public;
                            break;

                        case OutputCacheLocation.Client:
                            @public = HttpCacheability.Private;
                            break;

                        case OutputCacheLocation.Downstream:
                            @public = HttpCacheability.Public;
                            cache.SetNoServerCaching();
                            break;

                        case OutputCacheLocation.Server:
                            @public = HttpCacheability.Server;
                            break;

                        case OutputCacheLocation.None:
                            @public = HttpCacheability.NoCache;
                            break;

                        case OutputCacheLocation.ServerAndClient:
                            @public = HttpCacheability.ServerAndPrivate;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("cacheSettings", System.Web.SR.GetString("Invalid_cache_settings_location"));
                    }
                    cache.SetCacheability(@public);
                    if (any != OutputCacheLocation.None)
                    {
                        cache.SetExpires(this.Context.Timestamp.AddSeconds((double) seconds));
                        cache.SetMaxAge(new TimeSpan(0, 0, seconds));
                        cache.SetValidUntilExpires(true);
                        cache.SetLastModified(this.Context.Timestamp);
                        if (any != OutputCacheLocation.Client)
                        {
                            if (varyByContentEncoding != null)
                            {
                                foreach (string str7 in varyByContentEncoding.Split(s_varySeparator))
                                {
                                    cache.VaryByContentEncodings[str7.Trim()] = true;
                                }
                            }
                            if (varyByHeader != null)
                            {
                                foreach (string str8 in varyByHeader.Split(s_varySeparator))
                                {
                                    cache.VaryByHeaders[str8.Trim()] = true;
                                }
                            }
                            if (this.PageAdapter != null)
                            {
                                StringCollection cacheVaryByHeaders = this.PageAdapter.CacheVaryByHeaders;
                                if (cacheVaryByHeaders != null)
                                {
                                    foreach (string str9 in cacheVaryByHeaders)
                                    {
                                        cache.VaryByHeaders[str9] = true;
                                    }
                                }
                            }
                            if (any != OutputCacheLocation.Downstream)
                            {
                                if (varyByCustom != null)
                                {
                                    cache.SetVaryByCustom(varyByCustom);
                                }
                                if ((string.IsNullOrEmpty(varyByParam) && string.IsNullOrEmpty(varyByControl)) && ((this.PageAdapter == null) || (this.PageAdapter.CacheVaryByParams == null)))
                                {
                                    cache.VaryByParams.IgnoreParams = true;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(varyByParam))
                                    {
                                        foreach (string str10 in varyByParam.Split(s_varySeparator))
                                        {
                                            cache.VaryByParams[str10.Trim()] = true;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(varyByControl))
                                    {
                                        foreach (string str11 in varyByControl.Split(s_varySeparator))
                                        {
                                            cache.VaryByParams[str11.Trim()] = true;
                                        }
                                    }
                                    if (this.PageAdapter != null)
                                    {
                                        IList cacheVaryByParams = this.PageAdapter.CacheVaryByParams;
                                        if (cacheVaryByParams != null)
                                        {
                                            foreach (string str12 in cacheVaryByParams)
                                            {
                                                cache.VaryByParams[str12] = true;
                                            }
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(sqlDependency))
                                {
                                    this.Response.AddCacheDependency(new CacheDependency[] { SqlCacheDependency.CreateOutputCacheDependency(sqlDependency) });
                                }
                            }
                        }
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void InitOutputCache(int duration, string varyByHeader, string varyByCustom, OutputCacheLocation location, string varyByParam)
        {
            this.InitOutputCache(duration, null, varyByHeader, varyByCustom, location, varyByParam);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void InitOutputCache(int duration, string varyByContentEncoding, string varyByHeader, string varyByCustom, OutputCacheLocation location, string varyByParam)
        {
            if (!this._isCrossPagePostBack)
            {
                OutputCacheParameters cacheSettings = new OutputCacheParameters {
                    Duration = duration,
                    VaryByContentEncoding = varyByContentEncoding,
                    VaryByHeader = varyByHeader,
                    VaryByCustom = varyByCustom,
                    Location = location,
                    VaryByParam = varyByParam
                };
                this.InitOutputCache(cacheSettings);
            }
        }

        [Obsolete("The recommended alternative is ClientScript.IsClientScriptBlockRegistered(string key). http://go.microsoft.com/fwlink/?linkid=14202")]
        public bool IsClientScriptBlockRegistered(string key)
        {
            return this.ClientScript.IsClientScriptBlockRegistered(typeof(Page), key);
        }

        [Obsolete("The recommended alternative is ClientScript.IsStartupScriptRegistered(string key). http://go.microsoft.com/fwlink/?linkid=14202")]
        public bool IsStartupScriptRegistered(string key)
        {
            return this.ClientScript.IsStartupScriptRegistered(typeof(Page), key);
        }

        internal static bool IsSystemPostField(string field)
        {
            return s_systemPostFields.Contains(field);
        }

        private void LoadAllState()
        {
            object obj2 = this.LoadPageStateFromPersistenceMedium();
            IDictionary first = null;
            Pair second = null;
            Pair pair2 = obj2 as Pair;
            if (obj2 != null)
            {
                first = pair2.First as IDictionary;
                second = pair2.Second as Pair;
            }
            if (first != null)
            {
                this._controlsRequiringPostBack = (ArrayList) first["__ControlsRequirePostBackKey__"];
                if (this._registeredControlsRequiringControlState != null)
                {
                    foreach (Control control in (IEnumerable) this._registeredControlsRequiringControlState)
                    {
                        control.LoadControlStateInternal(first[control.UniqueID]);
                    }
                }
            }
            if (second != null)
            {
                string s = (string) second.First;
                int num = int.Parse(s, NumberFormatInfo.InvariantInfo);
                this._fPageLayoutChanged = num != this.GetTypeHashCode();
                if (!this._fPageLayoutChanged)
                {
                    base.LoadViewStateRecursive(second.Second);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual object LoadPageStateFromPersistenceMedium()
        {
            System.Web.UI.PageStatePersister pageStatePersister = this.PageStatePersister;
            try
            {
                pageStatePersister.Load();
            }
            catch (HttpException exception)
            {
                if (this._pageFlags[8])
                {
                    return null;
                }
                exception.WebEventCode = 0xbba;
                throw;
            }
            return new Pair(pageStatePersister.ControlState, pageStatePersister.ViewState);
        }

        internal void LoadScrollPosition()
        {
            if ((this._previousPagePath == null) && (this._requestValueCollection != null))
            {
                string s = this._requestValueCollection["__SCROLLPOSITIONX"];
                if ((s != null) && !int.TryParse(s, out this._scrollPositionX))
                {
                    this._scrollPositionX = 0;
                }
                string str2 = this._requestValueCollection["__SCROLLPOSITIONY"];
                if ((str2 != null) && !int.TryParse(str2, out this._scrollPositionY))
                {
                    this._scrollPositionY = 0;
                }
            }
        }

        public string MapPath(string virtualPath)
        {
            return this._request.MapPath(VirtualPath.CreateAllowNull(virtualPath), base.TemplateControlVirtualDirectory, true);
        }

        internal void OnFormPostRender(HtmlTextWriter writer)
        {
            this._inOnFormRender = false;
            if (this._postFormRenderDelegate != null)
            {
                this._postFormRenderDelegate(writer, null);
            }
        }

        internal void OnFormRender()
        {
            if (this._fOnFormRenderCalled)
            {
                throw new HttpException(System.Web.SR.GetString("Multiple_forms_not_allowed"));
            }
            this._fOnFormRenderCalled = true;
            this._inOnFormRender = true;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this._theme != null)
            {
                this._theme.SetStyleSheet();
            }
            if (this._styleSheet != null)
            {
                this._styleSheet.SetStyleSheet();
            }
        }

        protected virtual void OnInitComplete(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventInitComplete];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLoadComplete(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventLoadComplete];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPreInit(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventPreInit];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPreLoad(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventPreLoad];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPreRenderComplete(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventPreRenderComplete];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSaveStateComplete(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventSaveStateComplete];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void PerformPreInit()
        {
            this.OnPreInit(EventArgs.Empty);
            this.InitializeThemes();
            this.ApplyMasterPage();
            this._preInitWorkComplete = true;
        }

        private void PerformPreRenderComplete()
        {
            this.OnPreRenderComplete(EventArgs.Empty);
        }

        internal void PopCachingControl()
        {
            this._partialCachingControlStack.Pop();
        }

        internal void PopDataBindingContext()
        {
            this._dataBindingContext.Pop();
        }

        private void PrepareCallback(string callbackControlID)
        {
            this.Response.Cache.SetNoStore();
            try
            {
                string eventArgument = this._requestValueCollection["__CALLBACKPARAM"];
                this._callbackControl = this.FindControl(callbackControlID) as ICallbackEventHandler;
                if (this._callbackControl == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Page_CallBackTargetInvalid", new object[] { callbackControlID }));
                }
                this._callbackControl.RaiseCallbackEvent(eventArgument);
            }
            catch (Exception exception)
            {
                this.Response.Clear();
                this.Response.Write('e');
                if (this.Context.IsCustomErrorEnabled)
                {
                    this.Response.Write(System.Web.SR.GetString("Page_CallBackError"));
                }
                else
                {
                    bool flag = !string.IsNullOrEmpty(this._requestValueCollection["__CALLBACKLOADSCRIPT"]);
                    this.Response.Write(flag ? System.Web.UI.Util.QuoteJScriptString(HttpUtility.HtmlEncode(exception.Message)) : HttpUtility.HtmlEncode(exception.Message));
                }
            }
        }

        private void ProcessPostData(NameValueCollection postData, bool fBeforeLoad)
        {
            if (this._changedPostDataConsumers == null)
            {
                this._changedPostDataConsumers = new ArrayList();
            }
            if (postData != null)
            {
                foreach (string str in postData)
                {
                    if ((str != null) && !IsSystemPostField(str))
                    {
                        Control control = this.FindControl(str);
                        if (control == null)
                        {
                            if (fBeforeLoad)
                            {
                                if (this._leftoverPostData == null)
                                {
                                    this._leftoverPostData = new NameValueCollection();
                                }
                                this._leftoverPostData.Add(str, null);
                            }
                        }
                        else
                        {
                            IPostBackDataHandler postBackDataHandler = control.PostBackDataHandler;
                            if (postBackDataHandler == null)
                            {
                                if (control.PostBackEventHandler != null)
                                {
                                    this.RegisterRequiresRaiseEvent(control.PostBackEventHandler);
                                }
                            }
                            else
                            {
                                if ((postBackDataHandler != null) && postBackDataHandler.LoadPostData(str, this._requestValueCollection))
                                {
                                    this._changedPostDataConsumers.Add(control);
                                }
                                if (this._controlsRequiringPostBack != null)
                                {
                                    this._controlsRequiringPostBack.Remove(str);
                                }
                            }
                        }
                    }
                }
            }
            ArrayList list = null;
            if (this._controlsRequiringPostBack != null)
            {
                foreach (string str2 in this._controlsRequiringPostBack)
                {
                    Control control2 = this.FindControl(str2);
                    if (control2 != null)
                    {
                        IPostBackDataHandler adapterInternal = control2.AdapterInternal as IPostBackDataHandler;
                        if (adapterInternal == null)
                        {
                            adapterInternal = control2 as IPostBackDataHandler;
                        }
                        if (adapterInternal == null)
                        {
                            throw new HttpException(System.Web.SR.GetString("Postback_ctrl_not_found", new object[] { str2 }));
                        }
                        if (adapterInternal.LoadPostData(str2, this._requestValueCollection))
                        {
                            this._changedPostDataConsumers.Add(control2);
                        }
                    }
                    else if (fBeforeLoad)
                    {
                        if (list == null)
                        {
                            list = new ArrayList();
                        }
                        list.Add(str2);
                    }
                }
                this._controlsRequiringPostBack = list;
            }
        }

        private void ProcessRequest()
        {
            Thread currentThread = Thread.CurrentThread;
            CultureInfo currentCulture = currentThread.CurrentCulture;
            CultureInfo currentUICulture = currentThread.CurrentUICulture;
            try
            {
                this.ProcessRequest(true, true);
            }
            finally
            {
                this.RestoreCultures(currentThread, currentCulture, currentUICulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ProcessRequest(HttpContext context)
        {
            if ((HttpRuntime.NamedPermissionSet != null) && !HttpRuntime.DisableProcessRequestInApplicationTrust)
            {
                if (!HttpRuntime.ProcessRequestInApplicationTrust)
                {
                    this.ProcessRequestWithAssert(context);
                    return;
                }
                if (base.NoCompile)
                {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
            }
            this.ProcessRequestWithNoAssert(context);
        }

        private void ProcessRequest(bool includeStagesBeforeAsyncPoint, bool includeStagesAfterAsyncPoint)
        {
            if (includeStagesBeforeAsyncPoint)
            {
                this.FrameworkInitialize();
                base.ControlState = ControlState.FrameworkInitialized;
            }
            bool flag = this.Context.WorkerRequest is IIS7WorkerRequest;
            try
            {
                try
                {
                    if (this.IsTransacted)
                    {
                        this.ProcessRequestTransacted();
                    }
                    else
                    {
                        this.ProcessRequestMain(includeStagesBeforeAsyncPoint, includeStagesAfterAsyncPoint);
                    }
                    if (includeStagesAfterAsyncPoint)
                    {
                        flag = false;
                        this.ProcessRequestEndTrace();
                    }
                }
                catch (ThreadAbortException)
                {
                    try
                    {
                        if (flag)
                        {
                            this.ProcessRequestEndTrace();
                        }
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    if (includeStagesAfterAsyncPoint)
                    {
                        this.ProcessRequestCleanup();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void ProcessRequestCleanup()
        {
            this._request = null;
            this._response = null;
            if (!this.IsCrossPagePostBack)
            {
                this.UnloadRecursive(true);
            }
            if (this.Context.TraceIsEnabled)
            {
                this.Trace.StopTracing();
            }
        }

        private void ProcessRequestEndTrace()
        {
            if (this.Context.TraceIsEnabled)
            {
                this.Trace.EndRequest();
                if ((this.Trace.PageOutput && !this.IsCallback) && ((this.ScriptManager == null) || !this.ScriptManager.IsInAsyncPostBack))
                {
                    this.Trace.Render(this.CreateHtmlTextWriter(this.Response.Output));
                    this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                }
            }
        }

        private void ProcessRequestMain()
        {
            this.ProcessRequestMain(true, true);
        }

        private void ProcessRequestMain(bool includeStagesBeforeAsyncPoint, bool includeStagesAfterAsyncPoint)
        {
            try
            {
                HttpContext context = this.Context;
                string str = null;
                if (includeStagesBeforeAsyncPoint)
                {
                    if (this.IsInAspCompatMode)
                    {
                        AspCompatApplicationStep.OnPageStartSessionObjects();
                    }
                    if (this.PageAdapter != null)
                    {
                        this._requestValueCollection = this.PageAdapter.DeterminePostBackMode();
                    }
                    else
                    {
                        this._requestValueCollection = this.DeterminePostBackMode();
                    }
                    string callbackControlID = string.Empty;
                    if (this.DetermineIsExportingWebPart())
                    {
                        if (!RuntimeConfig.GetAppConfig().WebParts.EnableExport)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("WebPartExportHandler_DisabledExportHandler"));
                        }
                        str = this.Request.QueryString["webPart"];
                        if (string.IsNullOrEmpty(str))
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("WebPartExportHandler_InvalidArgument"));
                        }
                        if (string.Equals(this.Request.QueryString["scope"], "shared", StringComparison.OrdinalIgnoreCase))
                        {
                            this._pageFlags.Set(4);
                        }
                        string str3 = this.Request.QueryString["query"];
                        if (str3 == null)
                        {
                            str3 = string.Empty;
                        }
                        this.Request.QueryStringText = str3;
                        context.Trace.IsEnabled = false;
                    }
                    if (this._requestValueCollection != null)
                    {
                        if (this._requestValueCollection["__VIEWSTATEENCRYPTED"] != null)
                        {
                            this.ContainsEncryptedViewState = true;
                        }
                        callbackControlID = this._requestValueCollection["__CALLBACKID"];
                        if ((callbackControlID != null) && (this._request.HttpVerb == HttpVerb.POST))
                        {
                            this._isCallback = true;
                        }
                        else if (!this.IsCrossPagePostBack)
                        {
                            VirtualPath path = null;
                            if (this._requestValueCollection["__PREVIOUSPAGE"] != null)
                            {
                                try
                                {
                                    path = VirtualPath.CreateNonRelativeAllowNull(DecryptString(this._requestValueCollection["__PREVIOUSPAGE"]));
                                }
                                catch
                                {
                                    this._pageFlags[8] = true;
                                }
                                if ((path != null) && (path != this.Request.CurrentExecutionFilePathObject))
                                {
                                    this._pageFlags[8] = true;
                                    this._previousPagePath = path;
                                }
                            }
                        }
                    }
                    if (this.MaintainScrollPositionOnPostBack)
                    {
                        this.LoadScrollPosition();
                    }
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "Begin PreInit");
                    }
                    if (EtwTrace.IsTraceEnabled(5, 4))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_INIT_ENTER, this._context.WorkerRequest);
                    }
                    this.PerformPreInit();
                    if (EtwTrace.IsTraceEnabled(5, 4))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_INIT_LEAVE, this._context.WorkerRequest);
                    }
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "End PreInit");
                    }
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "Begin Init");
                    }
                    if (EtwTrace.IsTraceEnabled(5, 4))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_INIT_ENTER, this._context.WorkerRequest);
                    }
                    this.InitRecursive(null);
                    if (EtwTrace.IsTraceEnabled(5, 4))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_INIT_LEAVE, this._context.WorkerRequest);
                    }
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "End Init");
                    }
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "Begin InitComplete");
                    }
                    this.OnInitComplete(EventArgs.Empty);
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "End InitComplete");
                    }
                    if (this.IsPostBack)
                    {
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "Begin LoadState");
                        }
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_VIEWSTATE_ENTER, this._context.WorkerRequest);
                        }
                        this.LoadAllState();
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_VIEWSTATE_LEAVE, this._context.WorkerRequest);
                        }
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End LoadState");
                            this.Trace.Write("aspx.page", "Begin ProcessPostData");
                        }
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_POSTDATA_ENTER, this._context.WorkerRequest);
                        }
                        this.ProcessPostData(this._requestValueCollection, true);
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_POSTDATA_LEAVE, this._context.WorkerRequest);
                        }
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End ProcessPostData");
                        }
                    }
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "Begin PreLoad");
                    }
                    this.OnPreLoad(EventArgs.Empty);
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "End PreLoad");
                    }
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "Begin Load");
                    }
                    if (EtwTrace.IsTraceEnabled(5, 4))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_ENTER, this._context.WorkerRequest);
                    }
                    this.LoadRecursive();
                    if (EtwTrace.IsTraceEnabled(5, 4))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_LEAVE, this._context.WorkerRequest);
                    }
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "End Load");
                    }
                    if (this.IsPostBack)
                    {
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "Begin ProcessPostData Second Try");
                        }
                        this.ProcessPostData(this._leftoverPostData, false);
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End ProcessPostData Second Try");
                            this.Trace.Write("aspx.page", "Begin Raise ChangedEvents");
                        }
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_POST_DATA_CHANGED_ENTER, this._context.WorkerRequest);
                        }
                        this.RaiseChangedEvents();
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_POST_DATA_CHANGED_LEAVE, this._context.WorkerRequest);
                        }
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End Raise ChangedEvents");
                            this.Trace.Write("aspx.page", "Begin Raise PostBackEvent");
                        }
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RAISE_POSTBACK_ENTER, this._context.WorkerRequest);
                        }
                        this.RaisePostBackEvent(this._requestValueCollection);
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RAISE_POSTBACK_LEAVE, this._context.WorkerRequest);
                        }
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End Raise PostBackEvent");
                        }
                    }
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "Begin LoadComplete");
                    }
                    this.OnLoadComplete(EventArgs.Empty);
                    if (context.TraceIsEnabled)
                    {
                        this.Trace.Write("aspx.page", "End LoadComplete");
                    }
                    if (this.IsPostBack && this.IsCallback)
                    {
                        this.PrepareCallback(callbackControlID);
                    }
                    else if (!this.IsCrossPagePostBack)
                    {
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "Begin PreRender");
                        }
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_RENDER_ENTER, this._context.WorkerRequest);
                        }
                        this.PreRenderRecursiveInternal();
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_RENDER_LEAVE, this._context.WorkerRequest);
                        }
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End PreRender");
                        }
                    }
                }
                if ((this._asyncInfo == null) || this._asyncInfo.CallerIsBlocking)
                {
                    this.ExecuteRegisteredAsyncTasks();
                }
                this._request.ValidateRawUrl();
                if (includeStagesAfterAsyncPoint)
                {
                    if (this.IsCallback)
                    {
                        this.RenderCallback();
                    }
                    else if (!this.IsCrossPagePostBack)
                    {
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "Begin PreRenderComplete");
                        }
                        this.PerformPreRenderComplete();
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End PreRenderComplete");
                        }
                        if (context.TraceIsEnabled)
                        {
                            this.BuildPageProfileTree(this.EnableViewState);
                            this.Trace.Write("aspx.page", "Begin SaveState");
                        }
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_SAVE_VIEWSTATE_ENTER, this._context.WorkerRequest);
                        }
                        this.SaveAllState();
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_SAVE_VIEWSTATE_LEAVE, this._context.WorkerRequest);
                        }
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End SaveState");
                            this.Trace.Write("aspx.page", "Begin SaveStateComplete");
                        }
                        this.OnSaveStateComplete(EventArgs.Empty);
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End SaveStateComplete");
                            this.Trace.Write("aspx.page", "Begin Render");
                        }
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RENDER_ENTER, this._context.WorkerRequest);
                        }
                        if (str != null)
                        {
                            this.ExportWebPart(str);
                        }
                        else
                        {
                            this.RenderControl(this.CreateHtmlTextWriter(this.Response.Output));
                        }
                        if (EtwTrace.IsTraceEnabled(5, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RENDER_LEAVE, this._context.WorkerRequest);
                        }
                        if (context.TraceIsEnabled)
                        {
                            this.Trace.Write("aspx.page", "End Render");
                        }
                        this.CheckRemainingAsyncTasks(false);
                    }
                }
            }
            catch (ThreadAbortException exception)
            {
                HttpApplication.CancelModuleException exceptionState = exception.ExceptionState as HttpApplication.CancelModuleException;
                if (((!includeStagesBeforeAsyncPoint || !includeStagesAfterAsyncPoint) || ((this._context.Handler != this) || (this._context.ApplicationInstance == null))) || ((exceptionState == null) || exceptionState.Timeout))
                {
                    this.CheckRemainingAsyncTasks(true);
                    throw;
                }
                this._context.ApplicationInstance.CompleteRequest();
                ThreadResetAbortWithAssert();
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_DURING_REQUEST);
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);
                if (!this.HandleError(exception3))
                {
                    throw;
                }
            }
        }

        private void ProcessRequestTransacted()
        {
            bool transactionAborted = false;
            TransactedCallback callback = new TransactedCallback(this.ProcessRequestMain);
            Transactions.InvokeTransacted(callback, (TransactionOption) this._transactionMode, ref transactionAborted);
            try
            {
                if (transactionAborted)
                {
                    this.OnAbortTransaction(EventArgs.Empty);
                    WebBaseEvent.RaiseSystemEvent(this, 0x7d2);
                }
                else
                {
                    this.OnCommitTransaction(EventArgs.Empty);
                    WebBaseEvent.RaiseSystemEvent(this, 0x7d1);
                }
                this._request.ValidateRawUrl();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception exception)
            {
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_DURING_REQUEST);
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);
                if (!this.HandleError(exception))
                {
                    throw;
                }
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void ProcessRequestWithAssert(HttpContext context)
        {
            this.ProcessRequestWithNoAssert(context);
        }

        private void ProcessRequestWithNoAssert(HttpContext context)
        {
            this.SetIntrinsics(context);
            this.ProcessRequest();
        }

        internal void PushCachingControl(BasePartialCachingControl c)
        {
            if (this._partialCachingControlStack == null)
            {
                this._partialCachingControlStack = new Stack();
            }
            this._partialCachingControlStack.Push(c);
        }

        internal void PushDataBindingContext(object dataItem)
        {
            if (this._dataBindingContext == null)
            {
                this._dataBindingContext = new Stack();
            }
            this._dataBindingContext.Push(dataItem);
        }

        internal void RaiseChangedEvents()
        {
            if (this._changedPostDataConsumers != null)
            {
                for (int i = 0; i < this._changedPostDataConsumers.Count; i++)
                {
                    Control control = (Control) this._changedPostDataConsumers[i];
                    if (control != null)
                    {
                        IPostBackDataHandler postBackDataHandler = control.PostBackDataHandler;
                        if (((control == null) || control.IsDescendentOf(this)) && ((control != null) && (control.PostBackDataHandler != null)))
                        {
                            postBackDataHandler.RaisePostDataChangedEvent();
                        }
                    }
                }
            }
        }

        private void RaisePostBackEvent(NameValueCollection postData)
        {
            if (this._registeredControlThatRequireRaiseEvent != null)
            {
                this.RaisePostBackEvent(this._registeredControlThatRequireRaiseEvent, null);
            }
            else
            {
                string str = postData["__EVENTTARGET"];
                bool flag = !string.IsNullOrEmpty(str);
                if (flag || (this.AutoPostBackControl != null))
                {
                    Control control = null;
                    if (flag)
                    {
                        control = this.FindControl(str);
                    }
                    if ((control != null) && (control.PostBackEventHandler != null))
                    {
                        string eventArgument = postData["__EVENTARGUMENT"];
                        this.RaisePostBackEvent(control.PostBackEventHandler, eventArgument);
                    }
                }
                else
                {
                    this.Validate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void RaisePostBackEvent(IPostBackEventHandler sourceControl, string eventArgument)
        {
            sourceControl.RaisePostBackEvent(eventArgument);
        }

        [Obsolete("The recommended alternative is ClientScript.RegisterArrayDeclaration(string arrayName, string arrayValue). http://go.microsoft.com/fwlink/?linkid=14202"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public void RegisterArrayDeclaration(string arrayName, string arrayValue)
        {
            this.ClientScript.RegisterArrayDeclaration(arrayName, arrayValue);
        }

        public void RegisterAsyncTask(PageAsyncTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }
            if (this._asyncTaskManager == null)
            {
                this._asyncTaskManager = new PageAsyncTaskManager(this);
            }
            this._asyncTaskManager.AddTask(task);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Obsolete("The recommended alternative is ClientScript.RegisterClientScriptBlock(Type type, string key, string script). http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual void RegisterClientScriptBlock(string key, string script)
        {
            this.ClientScript.RegisterClientScriptBlock(typeof(Page), key, script);
        }

        internal void RegisterEnabledControl(Control control)
        {
            this.EnabledControls.Add(control);
        }

        internal void RegisterFocusScript()
        {
            if (this.ClientSupportsFocus && !this._requireFocusScript)
            {
                this.ClientScript.RegisterHiddenField("__LASTFOCUS", string.Empty);
                this._requireFocusScript = true;
                if (this._partialCachingControlStack != null)
                {
                    foreach (BasePartialCachingControl control in this._partialCachingControlStack)
                    {
                        control.RegisterFocusScript();
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Obsolete("The recommended alternative is ClientScript.RegisterHiddenField(string hiddenFieldName, string hiddenFieldInitialValue). http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual void RegisterHiddenField(string hiddenFieldName, string hiddenFieldInitialValue)
        {
            this.ClientScript.RegisterHiddenField(hiddenFieldName, hiddenFieldInitialValue);
        }

        [Obsolete("The recommended alternative is ClientScript.RegisterOnSubmitStatement(Type type, string key, string script). http://go.microsoft.com/fwlink/?linkid=14202"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public void RegisterOnSubmitStatement(string key, string script)
        {
            this.ClientScript.RegisterOnSubmitStatement(typeof(Page), key, script);
        }

        internal void RegisterPostBackScript()
        {
            if (this.ClientSupportsJavaScript && !this._fPostBackScriptRendered)
            {
                if (!this._fRequirePostBackScript)
                {
                    this.ClientScript.RegisterHiddenField("__EVENTTARGET", string.Empty);
                    this.ClientScript.RegisterHiddenField("__EVENTARGUMENT", string.Empty);
                    this._fRequirePostBackScript = true;
                }
                if (this._partialCachingControlStack != null)
                {
                    foreach (BasePartialCachingControl control in this._partialCachingControlStack)
                    {
                        control.RegisterPostBackScript();
                    }
                }
            }
        }

        internal void RegisterRequiresClearChildControlState(Control control)
        {
            if (this._registeredControlsRequiringClearChildControlState == null)
            {
                this._registeredControlsRequiringClearChildControlState = new HybridDictionary();
                this._registeredControlsRequiringClearChildControlState.Add(control, true);
            }
            else if (this._registeredControlsRequiringClearChildControlState[control] == null)
            {
                this._registeredControlsRequiringClearChildControlState.Add(control, true);
            }
            IDictionary controlState = (IDictionary) this.PageStatePersister.ControlState;
            if (controlState != null)
            {
                List<string> list = new List<string>(controlState.Count);
                foreach (string str in controlState.Keys)
                {
                    Control control2 = this.FindControl(str);
                    if ((control2 != null) && control2.IsDescendentOf(control))
                    {
                        list.Add(str);
                    }
                }
                foreach (string str2 in list)
                {
                    controlState[str2] = null;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void RegisterRequiresControlState(Control control)
        {
            if (control == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("Page_ControlState_ControlCannotBeNull"));
            }
            if (control.ControlState == ControlState.PreRendered)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Page_MustCallBeforeAndDuringPreRender", new object[] { "RegisterRequiresControlState" }));
            }
            if (this._registeredControlsRequiringControlState == null)
            {
                this._registeredControlsRequiringControlState = new ControlSet();
            }
            if (!this._registeredControlsRequiringControlState.Contains(control))
            {
                this._registeredControlsRequiringControlState.Add(control);
                IDictionary controlState = (IDictionary) this.PageStatePersister.ControlState;
                if (controlState != null)
                {
                    string uniqueID = control.UniqueID;
                    if (!this.ControlStateLoadedControlIds.Contains(uniqueID))
                    {
                        control.LoadControlStateInternal(controlState[uniqueID]);
                        this.ControlStateLoadedControlIds.Add(uniqueID);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void RegisterRequiresPostBack(Control control)
        {
            if (!(control is IPostBackDataHandler) && !(control.AdapterInternal is IPostBackDataHandler))
            {
                throw new HttpException(System.Web.SR.GetString("Ctrl_not_data_handler"));
            }
            if (this._registeredControlsThatRequirePostBack == null)
            {
                this._registeredControlsThatRequirePostBack = new ArrayList();
            }
            this._registeredControlsThatRequirePostBack.Add(control.UniqueID);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual void RegisterRequiresRaiseEvent(IPostBackEventHandler control)
        {
            this._registeredControlThatRequireRaiseEvent = control;
        }

        public void RegisterRequiresViewStateEncryption()
        {
            if (base.ControlState >= ControlState.PreRendered)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Too_late_for_RegisterRequiresViewStateEncryption"));
            }
            this._viewStateEncryptionRequested = true;
        }

        [Obsolete("The recommended alternative is ClientScript.RegisterStartupScript(Type type, string key, string script). http://go.microsoft.com/fwlink/?linkid=14202"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual void RegisterStartupScript(string key, string script)
        {
            this.ClientScript.RegisterStartupScript(typeof(Page), key, script, false);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void RegisterViewStateHandler()
        {
            this._needToPersistViewState = true;
        }

        internal void RegisterWebFormsScript()
        {
            if (this.ClientSupportsJavaScript && !this._fWebFormsScriptRendered)
            {
                this.RegisterPostBackScript();
                this._fRequireWebFormsScript = true;
                if (this._partialCachingControlStack != null)
                {
                    foreach (BasePartialCachingControl control in this._partialCachingControlStack)
                    {
                        control.RegisterWebFormsScript();
                    }
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.InitializeWriter(writer);
            base.Render(writer);
        }

        private void RenderCallback()
        {
            bool flag = !string.IsNullOrEmpty(this._requestValueCollection["__CALLBACKLOADSCRIPT"]);
            try
            {
                string str = null;
                if (flag)
                {
                    str = this._requestValueCollection["__CALLBACKINDEX"];
                    if (string.IsNullOrEmpty(str))
                    {
                        throw new HttpException(System.Web.SR.GetString("Page_CallBackInvalid"));
                    }
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (!char.IsDigit(str, i))
                        {
                            throw new HttpException(System.Web.SR.GetString("Page_CallBackInvalid"));
                        }
                    }
                    this.Response.Write("<script>parent.__pendingCallbacks[");
                    this.Response.Write(str);
                    this.Response.Write("].xmlRequest.responseText=\"");
                }
                if (this._callbackControl != null)
                {
                    string callbackResult = this._callbackControl.GetCallbackResult();
                    if (this.EnableEventValidation)
                    {
                        string eventValidationFieldValue = this.ClientScript.GetEventValidationFieldValue();
                        this.Response.Write(eventValidationFieldValue.Length.ToString(CultureInfo.InvariantCulture));
                        this.Response.Write('|');
                        this.Response.Write(eventValidationFieldValue);
                    }
                    else
                    {
                        this.Response.Write('s');
                    }
                    this.Response.Write(flag ? System.Web.UI.Util.QuoteJScriptString(callbackResult) : callbackResult);
                }
                if (flag)
                {
                    this.Response.Write("\";parent.__pendingCallbacks[");
                    this.Response.Write(str);
                    this.Response.Write("].xmlRequest.readyState=4;parent.WebForm_CallbackComplete();</script>");
                }
            }
            catch (Exception exception)
            {
                this.Response.Clear();
                this.Response.Write('e');
                if (this.Context.IsCustomErrorEnabled)
                {
                    this.Response.Write(System.Web.SR.GetString("Page_CallBackError"));
                }
                else
                {
                    this.Response.Write(flag ? System.Web.UI.Util.QuoteJScriptString(HttpUtility.HtmlEncode(exception.Message)) : HttpUtility.HtmlEncode(exception.Message));
                }
            }
        }

        private bool RenderDivAroundHiddenInputs(HtmlTextWriter writer)
        {
            if (!writer.RenderDivAroundHiddenInputs)
            {
                return false;
            }
            if (base.EnableLegacyRendering)
            {
                return (this.RenderingCompatibility >= VersionUtil.Framework40);
            }
            return true;
        }

        private void RenderPostBackScript(HtmlTextWriter writer, string formUniqueID)
        {
            writer.Write(base.EnableLegacyRendering ? "\r\n<script type=\"text/javascript\">\r\n<!--\r\n" : "\r\n<script type=\"text/javascript\">\r\n//<![CDATA[\r\n");
            if (this.PageAdapter != null)
            {
                writer.Write("var theForm = ");
                writer.Write(this.PageAdapter.GetPostBackFormReference(formUniqueID));
                writer.WriteLine(";");
            }
            else
            {
                writer.Write("var theForm = document.forms['");
                writer.Write(formUniqueID);
                writer.WriteLine("'];");
                writer.Write("if (!theForm) {\r\n    theForm = document.");
                writer.Write(formUniqueID);
                writer.WriteLine(";\r\n}");
            }
            writer.WriteLine("function __doPostBack(eventTarget, eventArgument) {\r\n    if (!theForm.onsubmit || (theForm.onsubmit() != false)) {\r\n        theForm.__EVENTTARGET.value = eventTarget;\r\n        theForm.__EVENTARGUMENT.value = eventArgument;\r\n        theForm.submit();\r\n    }\r\n}");
            writer.WriteLine(base.EnableLegacyRendering ? "// -->\r\n</script>\r\n" : "//]]>\r\n</script>\r\n");
            this._fPostBackScriptRendered = true;
        }

        internal void RenderViewStateFields(HtmlTextWriter writer)
        {
            if (this.ClientState != null)
            {
                ICollection is2 = this.DecomposeViewStateIntoChunks();
                writer.WriteLine();
                if (is2.Count > 1)
                {
                    writer.Write("<input type=\"hidden\" name=\"");
                    writer.Write("__VIEWSTATEFIELDCOUNT");
                    writer.Write("\" id=\"");
                    writer.Write("__VIEWSTATEFIELDCOUNT");
                    writer.Write("\" value=\"");
                    writer.Write(is2.Count.ToString(CultureInfo.InvariantCulture));
                    writer.WriteLine("\" />");
                }
                int num = 0;
                foreach (string str in is2)
                {
                    writer.Write("<input type=\"hidden\" name=\"");
                    writer.Write("__VIEWSTATE");
                    string str2 = null;
                    if (num > 0)
                    {
                        str2 = num.ToString(CultureInfo.InvariantCulture);
                        writer.Write(str2);
                    }
                    writer.Write("\" id=\"");
                    writer.Write("__VIEWSTATE");
                    if (num > 0)
                    {
                        writer.Write(str2);
                    }
                    writer.Write("\" value=\"");
                    writer.Write(str);
                    writer.WriteLine("\" />");
                    num++;
                }
            }
            else
            {
                writer.Write("\r\n<input type=\"hidden\" name=\"");
                writer.Write("__VIEWSTATE");
                writer.Write("\" id=\"");
                writer.Write("__VIEWSTATE");
                writer.WriteLine("\" value=\"\" />");
            }
        }

        private void RenderWebFormsScript(HtmlTextWriter writer)
        {
            this.ClientScript.RenderWebFormsScript(writer);
            this._fWebFormsScriptRendered = true;
        }

        public bool RequiresControlState(Control control)
        {
            return ((this._registeredControlsRequiringControlState != null) && this._registeredControlsRequiringControlState.Contains(control));
        }

        internal void ResetOnFormRenderCalled()
        {
            this._fOnFormRenderCalled = false;
        }

        private void RestoreCultures(Thread currentThread, CultureInfo prevCulture, CultureInfo prevUICulture)
        {
            if ((prevCulture != currentThread.CurrentCulture) || (prevUICulture != currentThread.CurrentUICulture))
            {
                if (HttpRuntime.IsFullTrust)
                {
                    this.SetCulture(currentThread, prevCulture, prevUICulture);
                }
                else
                {
                    this.SetCultureWithAssert(currentThread, prevCulture, prevUICulture);
                }
            }
        }

        private void SaveAllState()
        {
            if (this._needToPersistViewState)
            {
                Pair state = new Pair();
                IDictionary dictionary = null;
                if ((this._registeredControlsRequiringControlState != null) && (this._registeredControlsRequiringControlState.Count > 0))
                {
                    dictionary = new HybridDictionary(this._registeredControlsRequiringControlState.Count + 1);
                    foreach (Control control in (IEnumerable) this._registeredControlsRequiringControlState)
                    {
                        object obj2 = control.SaveControlStateInternal();
                        if ((dictionary[control.UniqueID] == null) && (obj2 != null))
                        {
                            dictionary.Add(control.UniqueID, obj2);
                        }
                    }
                }
                if ((this._registeredControlsThatRequirePostBack != null) && (this._registeredControlsThatRequirePostBack.Count > 0))
                {
                    if (dictionary == null)
                    {
                        dictionary = new HybridDictionary();
                    }
                    dictionary.Add("__ControlsRequirePostBackKey__", this._registeredControlsThatRequirePostBack);
                }
                if ((dictionary != null) && (dictionary.Count > 0))
                {
                    state.First = dictionary;
                }
                ViewStateMode viewStateMode = this.ViewStateMode;
                if (viewStateMode == ViewStateMode.Inherit)
                {
                    viewStateMode = ViewStateMode.Enabled;
                }
                Pair pair2 = new Pair(this.GetTypeHashCode().ToString(NumberFormatInfo.InvariantInfo), base.SaveViewStateRecursive(viewStateMode));
                if (this.Context.TraceIsEnabled)
                {
                    int viewstateSize = 0;
                    if (pair2.Second is Pair)
                    {
                        viewstateSize = base.EstimateStateSize(((Pair) pair2.Second).First);
                    }
                    else if (pair2.Second is Triplet)
                    {
                        viewstateSize = base.EstimateStateSize(((Triplet) pair2.Second).First);
                    }
                    this.Trace.AddControlStateSize(this.UniqueID, viewstateSize, (dictionary == null) ? 0 : base.EstimateStateSize(dictionary[this.UniqueID]));
                }
                state.Second = pair2;
                this.SavePageStateToPersistenceMedium(state);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual void SavePageStateToPersistenceMedium(object state)
        {
            System.Web.UI.PageStatePersister pageStatePersister = this.PageStatePersister;
            if (state is Pair)
            {
                Pair pair = (Pair) state;
                pageStatePersister.ControlState = pair.First;
                pageStatePersister.ViewState = pair.Second;
            }
            else
            {
                pageStatePersister.ViewState = state;
            }
            pageStatePersister.Save();
        }

        private void SetCulture(Thread currentThread, CultureInfo currentCulture, CultureInfo currentUICulture)
        {
            currentThread.CurrentCulture = currentCulture;
            currentThread.CurrentUICulture = currentUICulture;
        }

        [SecurityPermission(SecurityAction.Assert, ControlThread=true)]
        private void SetCultureWithAssert(Thread currentThread, CultureInfo currentCulture, CultureInfo currentUICulture)
        {
            this.SetCulture(currentThread, currentCulture, currentUICulture);
        }

        public void SetFocus(string clientID)
        {
            if ((clientID == null) || (clientID.Trim().Length == 0))
            {
                throw new ArgumentNullException("clientID");
            }
            if (this.Form == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Form_Required_For_Focus"));
            }
            if (this.Form.ControlState == ControlState.PreRendered)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Page_MustCallBeforeAndDuringPreRender", new object[] { "SetFocus" }));
            }
            this._focusedControlID = clientID.Trim();
            this._focusedControl = null;
            this.RegisterFocusScript();
        }

        public void SetFocus(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (this.Form == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Form_Required_For_Focus"));
            }
            if (this.Form.ControlState == ControlState.PreRendered)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Page_MustCallBeforeAndDuringPreRender", new object[] { "SetFocus" }));
            }
            this._focusedControl = control;
            this._focusedControlID = null;
            this.RegisterFocusScript();
        }

        internal void SetForm(HtmlForm form)
        {
            this._form = form;
        }

        internal void SetHeader(HtmlHead header)
        {
            this._header = header;
            if (!string.IsNullOrEmpty(this._titleToBeSet))
            {
                if (this._header == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Page_Title_Requires_Head"));
                }
                this.Title = this._titleToBeSet;
                this._titleToBeSet = null;
            }
            if (!string.IsNullOrEmpty(this._descriptionToBeSet))
            {
                if (this._header == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Page_Description_Requires_Head"));
                }
                this.MetaDescription = this._descriptionToBeSet;
                this._descriptionToBeSet = null;
            }
            if (!string.IsNullOrEmpty(this._keywordsToBeSet))
            {
                if (this._header == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Page_Description_Requires_Head"));
                }
                this.MetaKeywords = this._keywordsToBeSet;
                this._keywordsToBeSet = null;
            }
        }

        private void SetIntrinsics(HttpContext context)
        {
            this.SetIntrinsics(context, false);
        }

        private void SetIntrinsics(HttpContext context, bool allowAsync)
        {
            this._context = context;
            this._request = context.Request;
            this._response = context.Response;
            this._application = context.Application;
            this._cache = context.Cache;
            if ((!allowAsync && (this._context != null)) && (this._context.ApplicationInstance != null))
            {
                this._context.SyncContext.Disable();
            }
            if (!string.IsNullOrEmpty(this._clientTarget))
            {
                this._request.ClientTarget = this._clientTarget;
            }
            HttpCapabilitiesBase browser = this._request.Browser;
            if (browser != null)
            {
                this._response.ContentType = browser.PreferredRenderingMime;
                string preferredResponseEncoding = browser.PreferredResponseEncoding;
                string preferredRequestEncoding = browser.PreferredRequestEncoding;
                if (!string.IsNullOrEmpty(preferredResponseEncoding))
                {
                    this._response.ContentEncoding = Encoding.GetEncoding(preferredResponseEncoding);
                }
                if (!string.IsNullOrEmpty(preferredRequestEncoding))
                {
                    this._request.ContentEncoding = Encoding.GetEncoding(preferredRequestEncoding);
                }
            }
            base.HookUpAutomaticHandlers();
        }

        internal void SetPostFormRenderDelegate(RenderMethod renderMethod)
        {
            this._postFormRenderDelegate = renderMethod;
        }

        internal void SetPreviousPage(Page previousPage)
        {
            this._previousPage = previousPage;
        }

        internal void SetValidatorInvalidControlFocus(string clientID)
        {
            if (string.IsNullOrEmpty(this._validatorInvalidControl))
            {
                this._validatorInvalidControl = clientID;
                this.RegisterFocusScript();
            }
        }

        internal bool ShouldLoadControlState(Control control)
        {
            if (this._registeredControlsRequiringClearChildControlState != null)
            {
                foreach (Control control2 in this._registeredControlsRequiringClearChildControlState.Keys)
                {
                    if ((control != control2) && control.IsDescendentOf(control2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [SecurityPermission(SecurityAction.Assert, ControlThread=true)]
        internal static void ThreadResetAbortWithAssert()
        {
            Thread.ResetAbort();
        }

        internal override void UnloadRecursive(bool dispose)
        {
            base.UnloadRecursive(dispose);
            if ((this._previousPage != null) && this._previousPage.IsCrossPagePostBack)
            {
                this._previousPage.UnloadRecursive(dispose);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void UnregisterRequiresControlState(Control control)
        {
            if (control == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("Page_ControlState_ControlCannotBeNull"));
            }
            if (this._registeredControlsRequiringControlState != null)
            {
                this._registeredControlsRequiringControlState.Remove(control);
            }
        }

        public virtual void Validate()
        {
            this._validated = true;
            if (this._validators != null)
            {
                for (int i = 0; i < this.Validators.Count; i++)
                {
                    this.Validators[i].Validate();
                }
            }
        }

        public virtual void Validate(string validationGroup)
        {
            this._validated = true;
            if (this._validators != null)
            {
                ValidatorCollection validators = this.GetValidators(validationGroup);
                if (string.IsNullOrEmpty(validationGroup) && (this._validators.Count == validators.Count))
                {
                    this.Validate();
                }
                else
                {
                    for (int i = 0; i < validators.Count; i++)
                    {
                        validators[i].Validate();
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual void VerifyRenderingInServerForm(Control control)
        {
            if ((this.Context != null) && !base.DesignMode)
            {
                if (control == null)
                {
                    throw new ArgumentNullException("control");
                }
                if (!this._inOnFormRender && !this.IsCallback)
                {
                    throw new HttpException(System.Web.SR.GetString("ControlRenderedOutsideServerForm", new object[] { control.ClientID, control.GetType().Name }));
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpApplicationState Application
        {
            get
            {
                return this._application;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected bool AspCompatMode
        {
            get
            {
                return this._aspCompatMode;
            }
            set
            {
                this._aspCompatMode = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected bool AsyncMode
        {
            get
            {
                return this._asyncMode;
            }
            set
            {
                this._asyncMode = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TimeSpan AsyncTimeout
        {
            get
            {
                if (!this._asyncTimeoutSet)
                {
                    if (this.Context != null)
                    {
                        PagesSection pages = RuntimeConfig.GetConfig(this.Context).Pages;
                        if (pages != null)
                        {
                            this.AsyncTimeout = pages.AsyncTimeout;
                        }
                    }
                    if (!this._asyncTimeoutSet)
                    {
                        this.AsyncTimeout = TimeSpan.FromSeconds((double) DefaultAsyncTimeoutSeconds);
                    }
                }
                return this._asyncTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Page_Illegal_AsyncTimeout"), "AsyncTimeout");
                }
                this._asyncTimeout = value;
                this._asyncTimeoutSet = true;
            }
        }

        public Control AutoPostBackControl
        {
            get
            {
                return this._autoPostBackControl;
            }
            set
            {
                this._autoPostBackControl = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool Buffer
        {
            get
            {
                return this.Response.BufferOutput;
            }
            set
            {
                this.Response.BufferOutput = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Web.Caching.Cache Cache
        {
            get
            {
                if (this._cache == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Cache_not_available"));
                }
                return this._cache;
            }
        }

        internal string ClientOnSubmitEvent
        {
            get
            {
                if (!this.ClientScript.HasSubmitStatements && (((this.Form == null) || !this.Form.SubmitDisabledControls) || (this.EnabledControls.Count <= 0)))
                {
                    return string.Empty;
                }
                return "javascript:return WebForm_OnSubmit();";
            }
        }

        public string ClientQueryString
        {
            get
            {
                if (this._clientQueryString == null)
                {
                    if ((this.RequestInternal != null) && this.Request.HasQueryString)
                    {
                        Hashtable excludeKeys = new Hashtable();
                        foreach (string str in (IEnumerable) s_systemPostFields)
                        {
                            excludeKeys.Add(str, true);
                        }
                        this._clientQueryString = ((HttpValueCollection) this.Request.QueryString).ToString(true, excludeKeys);
                    }
                    else
                    {
                        this._clientQueryString = string.Empty;
                    }
                }
                return this._clientQueryString;
            }
        }

        public ClientScriptManager ClientScript
        {
            get
            {
                if (this._clientScriptManager == null)
                {
                    this._clientScriptManager = new ClientScriptManager(this);
                }
                return this._clientScriptManager;
            }
        }

        internal string ClientState
        {
            get
            {
                return this._clientState;
            }
            set
            {
                this._clientState = value;
            }
        }

        internal bool ClientSupportsFocus
        {
            get
            {
                if (this._request == null)
                {
                    return false;
                }
                if (this._request.Browser.EcmaScriptVersion < FocusMinimumEcmaVersion)
                {
                    return (this._request.Browser.JScriptVersion >= FocusMinimumJScriptVersion);
                }
                return true;
            }
        }

        internal bool ClientSupportsJavaScript
        {
            get
            {
                if (!this._clientSupportsJavaScriptChecked)
                {
                    this._clientSupportsJavaScript = (this._request != null) && (this._request.Browser.EcmaScriptVersion >= JavascriptMinimumVersion);
                    this._clientSupportsJavaScriptChecked = true;
                }
                return this._clientSupportsJavaScript;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("Page_ClientTarget")]
        public string ClientTarget
        {
            get
            {
                if (this._clientTarget != null)
                {
                    return this._clientTarget;
                }
                return string.Empty;
            }
            set
            {
                this._clientTarget = value;
                if (this._request != null)
                {
                    this._request.ClientTarget = value;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CodePage
        {
            get
            {
                return this.Response.ContentEncoding.CodePage;
            }
            set
            {
                this.Response.ContentEncoding = Encoding.GetEncoding(value);
            }
        }

        internal bool ContainsCrossPagePost
        {
            get
            {
                return this._containsCrossPagePost;
            }
            set
            {
                this._containsCrossPagePost = value;
            }
        }

        internal bool ContainsEncryptedViewState
        {
            get
            {
                return this._containsEncryptedViewState;
            }
            set
            {
                this._containsEncryptedViewState = value;
            }
        }

        internal bool ContainsTheme
        {
            get
            {
                return (this._theme != null);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string ContentType
        {
            get
            {
                return this.Response.ContentType;
            }
            set
            {
                this.Response.ContentType = value;
            }
        }

        protected internal override HttpContext Context
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this._context == null)
                {
                    this._context = HttpContext.Current;
                }
                return this._context;
            }
        }

        private StringSet ControlStateLoadedControlIds
        {
            get
            {
                if (this._controlStateLoadedControlIds == null)
                {
                    this._controlStateLoadedControlIds = new StringSet();
                }
                return this._controlStateLoadedControlIds;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string Culture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.DisplayName;
            }
            set
            {
                CultureInfo info = null;
                if (System.Web.Util.StringUtil.EqualsIgnoreCase(value, HttpApplication.AutoCulture))
                {
                    CultureInfo info2 = this.CultureFromUserLanguages(true);
                    if (info2 != null)
                    {
                        info = info2;
                    }
                }
                else if (System.Web.Util.StringUtil.StringStartsWithIgnoreCase(value, HttpApplication.AutoCulture))
                {
                    CultureInfo info3 = this.CultureFromUserLanguages(true);
                    if (info3 != null)
                    {
                        info = info3;
                    }
                    else
                    {
                        try
                        {
                            info = HttpServerUtility.CreateReadOnlyCultureInfo(value.Substring(5));
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    info = HttpServerUtility.CreateReadOnlyCultureInfo(value);
                }
                if (info != null)
                {
                    Thread.CurrentThread.CurrentCulture = info;
                    this._dynamicCulture = info;
                }
            }
        }

        internal CultureInfo DynamicCulture
        {
            get
            {
                return this._dynamicCulture;
            }
        }

        internal CultureInfo DynamicUICulture
        {
            get
            {
                return this._dynamicUICulture;
            }
        }

        private ArrayList EnabledControls
        {
            get
            {
                if (this._enabledControls == null)
                {
                    this._enabledControls = new ArrayList();
                }
                return this._enabledControls;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DefaultValue(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool EnableEventValidation
        {
            get
            {
                return this._enableEventValidation;
            }
            set
            {
                if (base.ControlState > ControlState.FrameworkInitialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("PropertySetAfterFrameworkInitialize", new object[] { "EnableEventValidation" }));
                }
                this._enableEventValidation = value;
            }
        }

        [Browsable(false)]
        public override bool EnableViewState
        {
            get
            {
                return base.EnableViewState;
            }
            set
            {
                base.EnableViewState = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public bool EnableViewStateMac
        {
            get
            {
                return this._enableViewStateMac;
            }
            set
            {
                if (this._enableViewStateMac != value)
                {
                    this._enableViewStateMac = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("Page_ErrorPage"), DefaultValue(""), Browsable(false)]
        public string ErrorPage
        {
            get
            {
                return this._errorPage;
            }
            set
            {
                this._errorPage = value;
            }
        }

        [Obsolete("The recommended alternative is HttpResponse.AddFileDependencies. http://go.microsoft.com/fwlink/?linkid=14202"), EditorBrowsable(EditorBrowsableState.Never)]
        protected ArrayList FileDependencies
        {
            set
            {
                this.Response.AddFileDependencies(value);
            }
        }

        internal Control FocusedControl
        {
            get
            {
                return this._focusedControl;
            }
        }

        internal string FocusedControlID
        {
            get
            {
                if (this._focusedControlID == null)
                {
                    return string.Empty;
                }
                return this._focusedControlID;
            }
        }

        public HtmlForm Form
        {
            get
            {
                return this._form;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HtmlHead Header
        {
            get
            {
                return this._header;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                base.ID = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual char IdSeparator
        {
            get
            {
                if (!this._haveIdSeparator)
                {
                    if (base.AdapterInternal != null)
                    {
                        this._idSeparator = this.PageAdapter.IdSeparator;
                    }
                    else
                    {
                        this._idSeparator = base.IdSeparatorFromConfig;
                    }
                    this._haveIdSeparator = true;
                }
                return this._idSeparator;
            }
        }

        public bool IsAsync
        {
            get
            {
                return this._asyncMode;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsCallback
        {
            get
            {
                return this._isCallback;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsCrossPagePostBack
        {
            get
            {
                return this._isCrossPagePostBack;
            }
        }

        internal bool IsExportingWebPart
        {
            get
            {
                return this._pageFlags[2];
            }
        }

        internal bool IsExportingWebPartShared
        {
            get
            {
                return this._pageFlags[4];
            }
        }

        internal bool IsInAspCompatMode
        {
            get
            {
                return this._aspCompatMode;
            }
        }

        internal bool IsInOnFormRender
        {
            get
            {
                return this._inOnFormRender;
            }
        }

        internal bool IsPartialRenderingSupported
        {
            get
            {
                if (!this._pageFlags[0x20])
                {
                    Type scriptManagerType = this.ScriptManagerType;
                    if (scriptManagerType != null)
                    {
                        object obj2 = this.Page.Items[scriptManagerType];
                        if (obj2 != null)
                        {
                            PropertyInfo property = scriptManagerType.GetProperty("SupportsPartialRendering");
                            if (property != null)
                            {
                                object obj3 = property.GetValue(obj2, null);
                                this._pageFlags[0x10] = (bool) obj3;
                            }
                        }
                    }
                    this._pageFlags[0x20] = true;
                }
                return this._pageFlags[0x10];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsPostBack
        {
            get
            {
                if (this._requestValueCollection == null)
                {
                    return false;
                }
                if (this._isCrossPagePostBack)
                {
                    return true;
                }
                if (this._pageFlags[8])
                {
                    return false;
                }
                return (((this.Context.ServerExecuteDepth <= 0) || ((this.Context.Handler != null) && !(base.GetType() != this.Context.Handler.GetType()))) && !this._fPageLayoutChanged);
            }
        }

        public bool IsPostBackEventControlRegistered
        {
            get
            {
                return (this._registeredControlThatRequireRaiseEvent != null);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        internal bool IsTransacted
        {
            get
            {
                return (this._transactionMode != 0);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsValid
        {
            get
            {
                if (!this._validated)
                {
                    throw new HttpException(System.Web.SR.GetString("IsValid_Cant_Be_Called"));
                }
                if (this._validators != null)
                {
                    ValidatorCollection validators = this.Validators;
                    int count = validators.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (!validators[i].IsValid)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        [Browsable(false)]
        public IDictionary Items
        {
            get
            {
                if (this._items == null)
                {
                    this._items = new HybridDictionary();
                }
                return this._items;
            }
        }

        internal string LastFocusedControl
        {
            [AspNetHostingPermission(SecurityAction.Assert, Level=AspNetHostingPermissionLevel.Low)]
            get
            {
                if (this.RequestInternal != null)
                {
                    string str = this.Request["__LASTFOCUS"];
                    if (str != null)
                    {
                        return str;
                    }
                }
                return string.Empty;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public int LCID
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.LCID;
            }
            set
            {
                CultureInfo info = HttpServerUtility.CreateReadOnlyCultureInfo(value);
                Thread.CurrentThread.CurrentCulture = info;
                this._dynamicCulture = info;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool MaintainScrollPositionOnPostBack
        {
            get
            {
                if (((this.RequestInternal != null) && (this.RequestInternal.Browser != null)) && !this.RequestInternal.Browser.SupportsMaintainScrollPositionOnPostback)
                {
                    return false;
                }
                return this._maintainScrollPosition;
            }
            set
            {
                if (this._maintainScrollPosition != value)
                {
                    this._maintainScrollPosition = value;
                    if (this._maintainScrollPosition)
                    {
                        this.LoadScrollPosition();
                    }
                }
            }
        }

        [WebSysDescription("MasterPage_MasterPage"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MasterPage Master
        {
            get
            {
                if ((this._master == null) && !this._preInitWorkComplete)
                {
                    this._master = MasterPage.CreateMaster(this, this.Context, this._masterPageFile, this._contentTemplateCollection);
                }
                return this._master;
            }
        }

        [WebSysDescription("MasterPage_MasterPageFile"), WebCategory("Behavior"), DefaultValue("")]
        public virtual string MasterPageFile
        {
            get
            {
                return VirtualPath.GetVirtualPathString(this._masterPageFile);
            }
            set
            {
                if (this._preInitWorkComplete)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("PropertySetBeforePageEvent", new object[] { "MasterPageFile", "Page_PreInit" }));
                }
                if (value != VirtualPath.GetVirtualPathString(this._masterPageFile))
                {
                    this._masterPageFile = VirtualPath.CreateAllowNull(value);
                    if ((this._master != null) && this.Controls.Contains(this._master))
                    {
                        this.Controls.Remove(this._master);
                    }
                    this._master = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public int MaxPageStateFieldLength
        {
            get
            {
                return this._maxPageStateFieldLength;
            }
            set
            {
                if (base.ControlState > ControlState.FrameworkInitialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("PropertySetAfterFrameworkInitialize", new object[] { "MaxPageStateFieldLength" }));
                }
                if ((value == 0) || (value < -1))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Page_Illegal_MaxPageStateFieldLength"), "MaxPageStateFieldLength");
                }
                this._maxPageStateFieldLength = value;
            }
        }

        [Bindable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizable(true)]
        public string MetaDescription
        {
            get
            {
                if ((this.Page.Header == null) && (base.ControlState >= ControlState.ChildrenInitialized))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Page_Description_Requires_Head"));
                }
                if (this._descriptionToBeSet != null)
                {
                    return this._descriptionToBeSet;
                }
                return this.Page.Header.Description;
            }
            set
            {
                if (this.Page.Header == null)
                {
                    if (base.ControlState >= ControlState.ChildrenInitialized)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Page_Description_Requires_Head"));
                    }
                    this._descriptionToBeSet = value;
                }
                else
                {
                    this.Page.Header.Description = value;
                }
            }
        }

        [Localizable(true), Bindable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MetaKeywords
        {
            get
            {
                if ((this.Page.Header == null) && (base.ControlState >= ControlState.ChildrenInitialized))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Page_Keywords_Requires_Head"));
                }
                if (this._keywordsToBeSet != null)
                {
                    return this._keywordsToBeSet;
                }
                return this.Page.Header.Keywords;
            }
            set
            {
                if (this.Page.Header == null)
                {
                    if (base.ControlState >= ControlState.ChildrenInitialized)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Page_Keywords_Requires_Head"));
                    }
                    this._keywordsToBeSet = value;
                }
                else
                {
                    this.Page.Header.Keywords = value;
                }
            }
        }

        public System.Web.UI.Adapters.PageAdapter PageAdapter
        {
            get
            {
                if (this._pageAdapter == null)
                {
                    this.ResolveAdapter();
                    this._pageAdapter = (System.Web.UI.Adapters.PageAdapter) base.AdapterInternal;
                }
                return this._pageAdapter;
            }
        }

        protected virtual System.Web.UI.PageStatePersister PageStatePersister
        {
            get
            {
                if (this._persister == null)
                {
                    System.Web.UI.Adapters.PageAdapter pageAdapter = this.PageAdapter;
                    if (pageAdapter != null)
                    {
                        this._persister = pageAdapter.GetStatePersister();
                    }
                    if (this._persister == null)
                    {
                        this._persister = new HiddenFieldPageStatePersister(this);
                    }
                }
                return this._persister;
            }
        }

        internal Stack PartialCachingControlStack
        {
            get
            {
                return this._partialCachingControlStack;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Page PreviousPage
        {
            get
            {
                if ((this._previousPage == null) && (this._previousPagePath != null))
                {
                    if (!System.Web.UI.Util.IsUserAllowedToPath(this.Context, this._previousPagePath))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Previous_Page_Not_Authorized"));
                    }
                    ITypedWebObjectFactory vPathBuildResult = (ITypedWebObjectFactory) BuildManager.GetVPathBuildResult(this.Context, this._previousPagePath);
                    if (typeof(Page).IsAssignableFrom(vPathBuildResult.InstantiatedType))
                    {
                        this._previousPage = (Page) vPathBuildResult.CreateInstance();
                        this._previousPage._isCrossPagePostBack = true;
                        this.Server.Execute(this._previousPage, TextWriter.Null, true, false);
                    }
                }
                return this._previousPage;
            }
        }

        internal string RelativeFilePath
        {
            get
            {
                if (this._relativeFilePath == null)
                {
                    string currentExecutionFilePath = this.Context.Request.CurrentExecutionFilePath;
                    string filePath = this.Context.Request.FilePath;
                    if (filePath.Equals(currentExecutionFilePath))
                    {
                        int num = currentExecutionFilePath.LastIndexOf('/');
                        if (num >= 0)
                        {
                            currentExecutionFilePath = currentExecutionFilePath.Substring(num + 1);
                        }
                        this._relativeFilePath = currentExecutionFilePath;
                    }
                    else
                    {
                        this._relativeFilePath = this.Server.UrlDecode(System.Web.Util.UrlPath.MakeRelative(filePath, currentExecutionFilePath));
                    }
                }
                return this._relativeFilePath;
            }
        }

        internal bool RenderFocusScript
        {
            get
            {
                return this._requireFocusScript;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpRequest Request
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this._request == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Request_not_available"));
                }
                return this._request;
            }
        }

        internal HttpRequest RequestInternal
        {
            get
            {
                return this._request;
            }
        }

        internal NameValueCollection RequestValueCollection
        {
            get
            {
                return this._requestValueCollection;
            }
        }

        internal string RequestViewStateString
        {
            get
            {
                if (!this._cachedRequestViewState)
                {
                    StringBuilder builder = new StringBuilder();
                    try
                    {
                        if (this.RequestValueCollection != null)
                        {
                            string str = this.RequestValueCollection["__VIEWSTATEFIELDCOUNT"];
                            if ((this.MaxPageStateFieldLength == -1) || (str == null))
                            {
                                this._cachedRequestViewState = true;
                                this._requestViewState = this.RequestValueCollection["__VIEWSTATE"];
                                return this._requestViewState;
                            }
                            int num = Convert.ToInt32(str, CultureInfo.InvariantCulture);
                            if (num < 0)
                            {
                                throw new HttpException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                            }
                            for (int i = 0; i < num; i++)
                            {
                                string str2 = "__VIEWSTATE";
                                if (i > 0)
                                {
                                    str2 = str2 + i.ToString(CultureInfo.InvariantCulture);
                                }
                                string str3 = this.RequestValueCollection[str2];
                                if (str3 == null)
                                {
                                    throw new HttpException(System.Web.SR.GetString("ViewState_MissingViewStateField", new object[] { str2 }));
                                }
                                builder.Append(str3);
                            }
                        }
                        this._cachedRequestViewState = true;
                        this._requestViewState = builder.ToString();
                    }
                    catch (Exception exception)
                    {
                        ViewStateException.ThrowViewStateError(exception, builder.ToString());
                    }
                }
                return this._requestViewState;
            }
        }

        internal bool RequiresViewStateEncryptionInternal
        {
            get
            {
                return ((this.ViewStateEncryptionMode == System.Web.UI.ViewStateEncryptionMode.Always) || (this._viewStateEncryptionRequested && (this.ViewStateEncryptionMode == System.Web.UI.ViewStateEncryptionMode.Auto)));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpResponse Response
        {
            get
            {
                if (this._response == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Response_not_available"));
                }
                return this._response;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string ResponseEncoding
        {
            get
            {
                return this.Response.ContentEncoding.EncodingName;
            }
            set
            {
                this.Response.ContentEncoding = Encoding.GetEncoding(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Web.Routing.RouteData RouteData
        {
            get
            {
                if ((this.Context != null) && (this.Context.Request != null))
                {
                    return this.Context.Request.RequestContext.RouteData;
                }
                return null;
            }
        }

        internal IScriptManager ScriptManager
        {
            get
            {
                return (IScriptManager) this.Items[typeof(IScriptManager)];
            }
        }

        internal Type ScriptManagerType
        {
            get
            {
                if (_scriptManagerType == null)
                {
                    _scriptManagerType = BuildManager.GetType("System.Web.UI.ScriptManager", false);
                }
                return _scriptManagerType;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpServerUtility Server
        {
            get
            {
                return this.Context.Server;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual HttpSessionState Session
        {
            get
            {
                if (!this._sessionRetrieved)
                {
                    this._sessionRetrieved = true;
                    try
                    {
                        this._session = this.Context.Session;
                    }
                    catch
                    {
                    }
                }
                if (this._session == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Session_not_enabled"));
                }
                return this._session;
            }
        }

        [Obsolete("The recommended alternative is Page.SetFocus and Page.MaintainScrollPositionOnPostBack. http://go.microsoft.com/fwlink/?linkid=14202"), Browsable(false), Filterable(false)]
        public bool SmartNavigation
        {
            get
            {
                if (this._smartNavSupport == SmartNavigationSupport.NotDesiredOrSupported)
                {
                    return false;
                }
                if (this._smartNavSupport == SmartNavigationSupport.Desired)
                {
                    HttpContext current = HttpContext.Current;
                    if (current == null)
                    {
                        return false;
                    }
                    HttpBrowserCapabilities browser = current.Request.Browser;
                    if ((!string.Equals(browser.Browser, "ie", StringComparison.OrdinalIgnoreCase) || (browser.MajorVersion < 6)) || !browser.Win32)
                    {
                        this._smartNavSupport = SmartNavigationSupport.NotDesiredOrSupported;
                    }
                    else
                    {
                        this._smartNavSupport = SmartNavigationSupport.IE6OrNewer;
                    }
                }
                return (this._smartNavSupport != SmartNavigationSupport.NotDesiredOrSupported);
            }
            set
            {
                if (value)
                {
                    this._smartNavSupport = SmartNavigationSupport.Desired;
                }
                else
                {
                    this._smartNavSupport = SmartNavigationSupport.NotDesiredOrSupported;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Filterable(false)]
        public virtual string StyleSheetTheme
        {
            get
            {
                return this._styleSheetName;
            }
            set
            {
                if (this._pageFlags[1])
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SetStyleSheetThemeCannotBeSet"));
                }
                this._styleSheetName = value;
            }
        }

        internal bool SupportsStyleSheets
        {
            get
            {
                if (this._supportsStyleSheets != -1)
                {
                    return (this._supportsStyleSheets == 1);
                }
                if (((((this.Header != null) && (this.Header.StyleSheet != null)) && ((this.RequestInternal != null) && (this.Request.Browser != null))) && (((this.Request.Browser["preferredRenderingType"] != "xhtml-mp") && this.Request.Browser.SupportsCss) && !this.Page.IsCallback)) && ((this.ScriptManager == null) || !this.ScriptManager.IsInAsyncPostBack))
                {
                    this._supportsStyleSheets = 1;
                    return true;
                }
                this._supportsStyleSheets = 0;
                return false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual string Theme
        {
            get
            {
                return this._themeName;
            }
            set
            {
                if (this._preInitWorkComplete)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("PropertySetBeforePageEvent", new object[] { "Theme", "Page_PreInit" }));
                }
                if (!string.IsNullOrEmpty(value) && !System.Web.Util.FileUtil.IsValidDirectoryName(value))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Page_theme_invalid_name", new object[] { value }), "Theme");
                }
                this._themeName = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Bindable(true), Localizable(true)]
        public string Title
        {
            get
            {
                if ((this.Page.Header == null) && (base.ControlState >= ControlState.ChildrenInitialized))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Page_Title_Requires_Head"));
                }
                if (this._titleToBeSet != null)
                {
                    return this._titleToBeSet;
                }
                return this.Page.Header.Title;
            }
            set
            {
                if (this.Page.Header == null)
                {
                    if (base.ControlState >= ControlState.ChildrenInitialized)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Page_Title_Requires_Head"));
                    }
                    this._titleToBeSet = value;
                }
                else
                {
                    this.Page.Header.Title = value;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TraceContext Trace
        {
            get
            {
                return this.Context.Trace;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TraceEnabled
        {
            get
            {
                return this.Trace.IsEnabled;
            }
            set
            {
                this.Trace.IsEnabled = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public TraceMode TraceModeValue
        {
            get
            {
                return this.Trace.TraceMode;
            }
            set
            {
                this.Trace.TraceMode = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected int TransactionMode
        {
            get
            {
                return this._transactionMode;
            }
            set
            {
                this._transactionMode = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string UICulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.DisplayName;
            }
            set
            {
                CultureInfo info = null;
                if (System.Web.Util.StringUtil.EqualsIgnoreCase(value, HttpApplication.AutoCulture))
                {
                    CultureInfo info2 = this.CultureFromUserLanguages(false);
                    if (info2 != null)
                    {
                        info = info2;
                    }
                }
                else if (System.Web.Util.StringUtil.StringStartsWithIgnoreCase(value, HttpApplication.AutoCulture))
                {
                    CultureInfo info3 = this.CultureFromUserLanguages(false);
                    if (info3 != null)
                    {
                        info = info3;
                    }
                    else
                    {
                        try
                        {
                            info = HttpServerUtility.CreateReadOnlyCultureInfo(value.Substring(5));
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    info = HttpServerUtility.CreateReadOnlyCultureInfo(value);
                }
                if (info != null)
                {
                    Thread.CurrentThread.CurrentUICulture = info;
                    this._dynamicUICulture = info;
                }
            }
        }

        protected internal virtual string UniqueFilePathSuffix
        {
            get
            {
                if (this._uniqueFilePathSuffix == null)
                {
                    this._uniqueFilePathSuffix = UniqueFilePathSuffixID + "=" + ((DateTime.Now.Ticks % 0xf422fL)).ToString("D6", CultureInfo.InvariantCulture);
                    this._uniqueFilePathSuffix = this._uniqueFilePathSuffix.PadLeft(6, '0');
                }
                return this._uniqueFilePathSuffix;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IPrincipal User
        {
            get
            {
                return this.Context.User;
            }
        }

        internal string ValidatorInvalidControl
        {
            get
            {
                if (this._validatorInvalidControl == null)
                {
                    return string.Empty;
                }
                return this._validatorInvalidControl;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ValidatorCollection Validators
        {
            get
            {
                if (this._validators == null)
                {
                    this._validators = new ValidatorCollection();
                }
                return this._validators;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DefaultValue(0), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Web.UI.ViewStateEncryptionMode ViewStateEncryptionMode
        {
            get
            {
                return this._encryptionMode;
            }
            set
            {
                if (base.ControlState > ControlState.FrameworkInitialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("PropertySetAfterFrameworkInitialize", new object[] { "ViewStateEncryptionMode" }));
                }
                if ((value < System.Web.UI.ViewStateEncryptionMode.Auto) || (value > System.Web.UI.ViewStateEncryptionMode.Never))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._encryptionMode = value;
            }
        }

        [Browsable(false)]
        public string ViewStateUserKey
        {
            get
            {
                return this._viewStateUserKey;
            }
            set
            {
                if (base.ControlState >= ControlState.Initialized)
                {
                    throw new HttpException(System.Web.SR.GetString("Too_late_for_ViewStateUserKey"));
                }
                this._viewStateUserKey = value;
            }
        }

        [Browsable(false)]
        public override bool Visible
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        internal System.Web.Configuration.XhtmlConformanceMode XhtmlConformanceMode
        {
            get
            {
                if (!this._xhtmlConformanceModeSet)
                {
                    if (base.DesignMode)
                    {
                        this._xhtmlConformanceMode = System.Web.Configuration.XhtmlConformanceMode.Transitional;
                    }
                    else
                    {
                        this._xhtmlConformanceMode = base.GetXhtmlConformanceSection().Mode;
                    }
                    this._xhtmlConformanceModeSet = true;
                }
                return this._xhtmlConformanceMode;
            }
        }

        private class PageAsyncInfo
        {
            private HttpApplication _app;
            private bool _asyncPointReached;
            private HttpAsyncResult _asyncResult;
            private ArrayList _beginHandlers;
            private bool _callerIsBlocking;
            private WaitCallback _callHandlersCancellableCallback;
            private WaitCallback _callHandlersThreadpoolCallback;
            private bool _completed;
            private AsyncCallback _completionCallback;
            private int _currentHandler;
            private ArrayList _endHandlers;
            private Exception _error;
            private int _handlerCount;
            private Page _page;
            private ArrayList _stateObjects;
            private AspNetSynchronizationContext _syncContext;

            internal PageAsyncInfo(Page page)
            {
                this._page = page;
                this._app = page.Context.ApplicationInstance;
                this._syncContext = page.Context.SyncContext;
                this._completionCallback = new AsyncCallback(this.OnAsyncHandlerCompletion);
                this._callHandlersThreadpoolCallback = new WaitCallback(this.CallHandlersFromThreadpoolThread);
                this._callHandlersCancellableCallback = new WaitCallback(this.CallHandlersCancellableCallback);
            }

            internal void AddHandler(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
            {
                if (this._handlerCount == 0)
                {
                    this._beginHandlers = new ArrayList();
                    this._endHandlers = new ArrayList();
                    this._stateObjects = new ArrayList();
                }
                this._beginHandlers.Add(beginHandler);
                this._endHandlers.Add(endHandler);
                this._stateObjects.Add(state);
                this._handlerCount++;
            }

            internal void CallHandlers(bool onPageThread)
            {
                try
                {
                    this._page.Context.InvokeCancellableCallback(this._callHandlersCancellableCallback, onPageThread);
                }
                catch (Exception exception)
                {
                    this._error = exception;
                    this._completed = true;
                    this._asyncResult.Complete(onPageThread, null, this._error);
                    if ((!onPageThread && (exception is ThreadAbortException)) && (((ThreadAbortException) exception).ExceptionState is HttpApplication.CancelModuleException))
                    {
                        Page.ThreadResetAbortWithAssert();
                    }
                }
            }

            private void CallHandlersCancellableCallback(object state)
            {
                bool onPageThread = (bool) state;
                if (this.CallerIsBlocking || onPageThread)
                {
                    this.CallHandlersPossiblyUnderLock(onPageThread);
                }
                else
                {
                    lock (this._app)
                    {
                        this.CallHandlersPossiblyUnderLock(onPageThread);
                    }
                }
            }

            private void CallHandlersFromThreadpoolThread(object data)
            {
                this.CallHandlers(false);
            }

            private void CallHandlersPossiblyUnderLock(bool onPageThread)
            {
                HttpApplication.ThreadContext context = null;
                if (!onPageThread)
                {
                    context = this._app.OnThreadEnter();
                }
                try
                {
                    while ((this._currentHandler < this._handlerCount) && (this._error == null))
                    {
                        try
                        {
                            IAsyncResult ar = ((BeginEventHandler) this._beginHandlers[this._currentHandler])(this._page, EventArgs.Empty, this._completionCallback, this._stateObjects[this._currentHandler]);
                            if (ar == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Async_null_asyncresult"));
                            }
                            if (!ar.CompletedSynchronously)
                            {
                                return;
                            }
                            try
                            {
                                ((EndEventHandler) this._endHandlers[this._currentHandler])(ar);
                            }
                            finally
                            {
                                this._currentHandler++;
                            }
                            continue;
                        }
                        catch (Exception exception)
                        {
                            if (onPageThread && (this._syncContext.PendingOperationsCount == 0))
                            {
                                throw;
                            }
                            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_DURING_REQUEST);
                            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);
                            try
                            {
                                if (!this._page.HandleError(exception))
                                {
                                    this._error = exception;
                                }
                            }
                            catch (Exception exception2)
                            {
                                this._error = exception2;
                            }
                            continue;
                        }
                    }
                    if (this._syncContext.PendingOperationsCount > 0)
                    {
                        this._syncContext.SetLastCompletionWorkItem(this._callHandlersThreadpoolCallback);
                    }
                    else
                    {
                        if ((this._error == null) && (this._syncContext.Error != null))
                        {
                            try
                            {
                                if (!this._page.HandleError(this._syncContext.Error))
                                {
                                    this._error = this._syncContext.Error;
                                    this._syncContext.ClearError();
                                }
                            }
                            catch (Exception exception3)
                            {
                                this._error = exception3;
                            }
                        }
                        try
                        {
                            this._page.ProcessRequest(false, true);
                        }
                        catch (Exception exception4)
                        {
                            if (onPageThread)
                            {
                                throw;
                            }
                            this._error = exception4;
                        }
                        if (context != null)
                        {
                            try
                            {
                                context.Leave();
                            }
                            finally
                            {
                                context = null;
                            }
                        }
                        this._completed = true;
                        this._asyncResult.Complete(onPageThread, null, this._error);
                    }
                }
                finally
                {
                    if (context != null)
                    {
                        context.Leave();
                    }
                }
            }

            private void OnAsyncHandlerCompletion(IAsyncResult ar)
            {
                if (!ar.CompletedSynchronously)
                {
                    try
                    {
                        ((EndEventHandler) this._endHandlers[this._currentHandler])(ar);
                    }
                    catch (Exception exception)
                    {
                        this._error = exception;
                    }
                    if (!this._completed)
                    {
                        this._currentHandler++;
                        if (Thread.CurrentThread.IsThreadPoolThread)
                        {
                            this.CallHandlers(false);
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(this._callHandlersThreadpoolCallback);
                        }
                    }
                }
            }

            internal void SetError(Exception error)
            {
                this._error = error;
            }

            internal bool AsyncPointReached
            {
                get
                {
                    return this._asyncPointReached;
                }
                set
                {
                    this._asyncPointReached = value;
                }
            }

            internal HttpAsyncResult AsyncResult
            {
                get
                {
                    return this._asyncResult;
                }
                set
                {
                    this._asyncResult = value;
                }
            }

            internal bool CallerIsBlocking
            {
                get
                {
                    return this._callerIsBlocking;
                }
                set
                {
                    this._callerIsBlocking = value;
                }
            }
        }
    }
}

