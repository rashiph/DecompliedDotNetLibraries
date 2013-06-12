namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Text;
    using System.Web;
    using System.Web.Handlers;
    using System.Web.Util;

    public sealed class ClientScriptManager
    {
        private const string _callbackFunctionName = "WebForm_DoCallback";
        private HybridDictionary _clientPostBackValidatedEventTable;
        private ArrayList _clientScriptBlocks;
        private ArrayList _clientStartupScripts;
        private bool _eventValidationFieldLoaded;
        private Page _owner;
        private const string _postBackFunctionName = "__doPostBack";
        private const string _postbackOptionsFunctionName = "WebForm_DoPostBackWithOptions";
        private IDictionary _registeredArrayDeclares;
        private ListDictionary _registeredClientScriptBlocks;
        private ListDictionary _registeredClientStartupScripts;
        private ListDictionary _registeredControlsWithExpandoAttributes;
        private IDictionary _registeredHiddenFields;
        private ListDictionary _registeredOnSubmitStatements;
        private Dictionary<Assembly, Dictionary<string, object>> _registeredResourcesToSuppress;
        internal static IScriptResourceMapping _scriptResourceMapping;
        private ArrayList _validEventReferences;
        internal const string ClientScriptEnd = "//]]>\r\n</script>\r\n";
        internal const string ClientScriptEndLegacy = "// -->\r\n</script>\r\n";
        internal const string ClientScriptStart = "\r\n<script type=\"text/javascript\">\r\n//<![CDATA[\r\n";
        internal const string ClientScriptStartLegacy = "\r\n<script type=\"text/javascript\">\r\n<!--\r\n";
        private const string IncludeScriptBegin = "\r\n<script src=\"";
        private const string IncludeScriptEnd = "\" type=\"text/javascript\"></script>";
        internal const string JscriptPrefix = "javascript:";
        private const string PageCallbackScriptKey = "PageCallbackScript";

        internal ClientScriptManager(Page owner)
        {
            this._owner = owner;
        }

        internal void ClearHiddenFields()
        {
            this._registeredHiddenFields = null;
        }

        private static int ComputeHashKey(string uniqueId, string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                return StringUtil.GetStringHashCode(uniqueId);
            }
            return (StringUtil.GetStringHashCode(uniqueId) ^ StringUtil.GetStringHashCode(argument));
        }

        internal static ScriptKey CreateScriptIncludeKey(Type type, string key, bool isResource)
        {
            return new ScriptKey(type, key, true, isResource);
        }

        internal static ScriptKey CreateScriptKey(Type type, string key)
        {
            return new ScriptKey(type, key);
        }

        private void EnsureEventValidationFieldLoaded()
        {
            if (!this._eventValidationFieldLoaded)
            {
                this._eventValidationFieldLoaded = true;
                string str = null;
                if (this._owner.RequestValueCollection != null)
                {
                    str = this._owner.RequestValueCollection["__EVENTVALIDATION"];
                }
                if (!string.IsNullOrEmpty(str))
                {
                    IStateFormatter formatter = this._owner.CreateStateFormatter();
                    ArrayList list = null;
                    try
                    {
                        list = formatter.Deserialize(str) as ArrayList;
                    }
                    catch (Exception exception)
                    {
                        ViewStateException.ThrowViewStateError(exception, str);
                    }
                    if ((list != null) && (list.Count >= 1))
                    {
                        int num = (int) list[0];
                        string requestViewStateString = this._owner.RequestViewStateString;
                        if (num != StringUtil.GetStringHashCode(requestViewStateString))
                        {
                            ViewStateException.ThrowViewStateError(null, str);
                        }
                        this._clientPostBackValidatedEventTable = new HybridDictionary(list.Count - 1, true);
                        for (int i = 1; i < list.Count; i++)
                        {
                            int num3 = (int) list[i];
                            this._clientPostBackValidatedEventTable[num3] = null;
                        }
                        if (this._owner.IsCallback)
                        {
                            this._validEventReferences = list;
                        }
                    }
                }
            }
        }

        public string GetCallbackEventReference(Control control, string argument, string clientCallback, string context)
        {
            return this.GetCallbackEventReference(control, argument, clientCallback, context, false);
        }

        public string GetCallbackEventReference(Control control, string argument, string clientCallback, string context, bool useAsync)
        {
            return this.GetCallbackEventReference(control, argument, clientCallback, context, null, useAsync);
        }

        public string GetCallbackEventReference(string target, string argument, string clientCallback, string context, string clientErrorCallback, bool useAsync)
        {
            this._owner.RegisterWebFormsScript();
            if ((this._owner.ClientSupportsJavaScript && (this._owner.RequestInternal != null)) && this._owner.RequestInternal.Browser.SupportsCallback)
            {
                this.RegisterStartupScript(typeof(Page), "PageCallbackScript", ((this._owner.RequestInternal != null) && string.Equals(this._owner.RequestInternal.Url.Scheme, "https", StringComparison.OrdinalIgnoreCase)) ? ("\r\nvar callBackFrameUrl='" + Util.QuoteJScriptString(this.GetWebResourceUrl(typeof(Page), "SmartNav.htm"), false) + "';\r\nWebForm_InitCallback();") : "\r\nWebForm_InitCallback();", true);
            }
            if (argument == null)
            {
                argument = "null";
            }
            else if (argument.Length == 0)
            {
                argument = "\"\"";
            }
            if (context == null)
            {
                context = "null";
            }
            else if (context.Length == 0)
            {
                context = "\"\"";
            }
            return ("WebForm_DoCallback(" + target + "," + argument + "," + clientCallback + "," + context + "," + ((clientErrorCallback == null) ? "null" : clientErrorCallback) + "," + (useAsync ? "true" : "false") + ")");
        }

        public string GetCallbackEventReference(Control control, string argument, string clientCallback, string context, string clientErrorCallback, bool useAsync)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (!(control is ICallbackEventHandler))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Page_CallBackTargetInvalid", new object[] { control.UniqueID }));
            }
            return this.GetCallbackEventReference("'" + control.UniqueID + "'", argument, clientCallback, context, clientErrorCallback, useAsync);
        }

        internal string GetEventValidationFieldValue()
        {
            if ((this._validEventReferences != null) && (this._validEventReferences.Count != 0))
            {
                return this._owner.CreateStateFormatter().Serialize(this._validEventReferences);
            }
            return string.Empty;
        }

        public string GetPostBackClientHyperlink(Control control, string argument)
        {
            return this.GetPostBackClientHyperlink(control, argument, true, false);
        }

        public string GetPostBackClientHyperlink(Control control, string argument, bool registerForEventValidation)
        {
            return this.GetPostBackClientHyperlink(control, argument, true, registerForEventValidation);
        }

        internal string GetPostBackClientHyperlink(Control control, string argument, bool escapePercent, bool registerForEventValidation)
        {
            return ("javascript:" + this.GetPostBackEventReference(control, argument, escapePercent, registerForEventValidation));
        }

        public string GetPostBackEventReference(PostBackOptions options)
        {
            return this.GetPostBackEventReference(options, false);
        }

        public string GetPostBackEventReference(Control control, string argument)
        {
            return this.GetPostBackEventReference(control, argument, false, false);
        }

        public string GetPostBackEventReference(PostBackOptions options, bool registerForEventValidation)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (registerForEventValidation)
            {
                this.RegisterForEventValidation(options);
            }
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            if (options.RequiresJavaScriptProtocol)
            {
                builder.Append("javascript:");
            }
            if (options.AutoPostBack)
            {
                builder.Append("setTimeout('");
            }
            if ((!options.PerformValidation && !options.TrackFocus) && (options.ClientSubmit && string.IsNullOrEmpty(options.ActionUrl)))
            {
                string postBackEventReference = this.GetPostBackEventReference(options.TargetControl, options.Argument);
                if (options.AutoPostBack)
                {
                    builder.Append(Util.QuoteJScriptString(postBackEventReference));
                    builder.Append("', 0)");
                }
                else
                {
                    builder.Append(postBackEventReference);
                }
                return builder.ToString();
            }
            builder.Append("WebForm_DoPostBackWithOptions");
            builder.Append("(new WebForm_PostBackOptions(\"");
            builder.Append(options.TargetControl.UniqueID);
            builder.Append("\", ");
            if (string.IsNullOrEmpty(options.Argument))
            {
                builder.Append("\"\", ");
            }
            else
            {
                builder.Append("\"");
                builder.Append(Util.QuoteJScriptString(options.Argument));
                builder.Append("\", ");
            }
            if (options.PerformValidation)
            {
                flag = true;
                builder.Append("true, ");
            }
            else
            {
                builder.Append("false, ");
            }
            if ((options.ValidationGroup != null) && (options.ValidationGroup.Length > 0))
            {
                flag = true;
                builder.Append("\"");
                builder.Append(options.ValidationGroup);
                builder.Append("\", ");
            }
            else
            {
                builder.Append("\"\", ");
            }
            if ((options.ActionUrl != null) && (options.ActionUrl.Length > 0))
            {
                flag = true;
                this._owner.ContainsCrossPagePost = true;
                builder.Append("\"");
                builder.Append(Util.QuoteJScriptString(options.ActionUrl));
                builder.Append("\", ");
            }
            else
            {
                builder.Append("\"\", ");
            }
            if (options.TrackFocus)
            {
                this._owner.RegisterFocusScript();
                flag = true;
                builder.Append("true, ");
            }
            else
            {
                builder.Append("false, ");
            }
            if (options.ClientSubmit)
            {
                flag = true;
                this._owner.RegisterPostBackScript();
                builder.Append("true))");
            }
            else
            {
                builder.Append("false))");
            }
            if (options.AutoPostBack)
            {
                builder.Append("', 0)");
            }
            string str2 = null;
            if (flag)
            {
                str2 = builder.ToString();
                this._owner.RegisterWebFormsScript();
            }
            return str2;
        }

        public string GetPostBackEventReference(Control control, string argument, bool registerForEventValidation)
        {
            return this.GetPostBackEventReference(control, argument, false, registerForEventValidation);
        }

        private string GetPostBackEventReference(Control control, string argument, bool forUrl, bool registerForEventValidation)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            this._owner.RegisterPostBackScript();
            string uniqueID = control.UniqueID;
            if (registerForEventValidation)
            {
                this.RegisterForEventValidation(uniqueID, argument);
            }
            if ((control.EnableLegacyRendering && this._owner.IsInOnFormRender) && ((uniqueID != null) && (uniqueID.IndexOf(':') >= 0)))
            {
                uniqueID = uniqueID.Replace(':', '$');
            }
            return (("__doPostBack('" + uniqueID + "','") + Util.QuoteJScriptString(argument, forUrl) + "')");
        }

        public string GetWebResourceUrl(Type type, string resourceName)
        {
            return GetWebResourceUrl(this._owner, type, resourceName, false, (this._owner == null) ? null : this._owner.ScriptManager);
        }

        internal static string GetWebResourceUrl(Page owner, Type type, string resourceName, bool htmlEncoded, IScriptManager scriptManager)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentNullException("resourceName");
            }
            if ((owner == null) || !owner.DesignMode)
            {
                return AssemblyResourceLoader.GetWebResourceUrl(type, resourceName, htmlEncoded, scriptManager);
            }
            ISite site = owner.Site;
            if (site != null)
            {
                IResourceUrlGenerator service = site.GetService(typeof(IResourceUrlGenerator)) as IResourceUrlGenerator;
                if (service != null)
                {
                    return service.GetResourceUrl(type, resourceName);
                }
            }
            return resourceName;
        }

        public bool IsClientScriptBlockRegistered(string key)
        {
            return this.IsClientScriptBlockRegistered(typeof(Page), key);
        }

        public bool IsClientScriptBlockRegistered(Type type, string key)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return ((this._registeredClientScriptBlocks != null) && this._registeredClientScriptBlocks.Contains(CreateScriptKey(type, key)));
        }

        public bool IsClientScriptIncludeRegistered(string key)
        {
            return this.IsClientScriptIncludeRegistered(typeof(Page), key);
        }

        public bool IsClientScriptIncludeRegistered(Type type, string key)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return ((this._registeredClientScriptBlocks != null) && this._registeredClientScriptBlocks.Contains(CreateScriptIncludeKey(type, key, false)));
        }

        public bool IsOnSubmitStatementRegistered(string key)
        {
            return this.IsOnSubmitStatementRegistered(typeof(Page), key);
        }

        public bool IsOnSubmitStatementRegistered(Type type, string key)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return ((this._registeredOnSubmitStatements != null) && this._registeredOnSubmitStatements.Contains(CreateScriptKey(type, key)));
        }

        public bool IsStartupScriptRegistered(string key)
        {
            return this.IsStartupScriptRegistered(typeof(Page), key);
        }

        public bool IsStartupScriptRegistered(Type type, string key)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return ((this._registeredClientStartupScripts != null) && this._registeredClientStartupScripts.Contains(CreateScriptKey(type, key)));
        }

        public void RegisterArrayDeclaration(string arrayName, string arrayValue)
        {
            if (arrayName == null)
            {
                throw new ArgumentNullException("arrayName");
            }
            if (this._registeredArrayDeclares == null)
            {
                this._registeredArrayDeclares = new ListDictionary();
            }
            if (!this._registeredArrayDeclares.Contains(arrayName))
            {
                this._registeredArrayDeclares[arrayName] = new ArrayList();
            }
            ((ArrayList) this._registeredArrayDeclares[arrayName]).Add(arrayValue);
            if (this._owner.PartialCachingControlStack != null)
            {
                foreach (BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    control.RegisterArrayDeclaration(arrayName, arrayValue);
                }
            }
        }

        internal void RegisterArrayDeclaration(Control control, string arrayName, string arrayValue)
        {
            IScriptManager scriptManager = this._owner.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
            {
                scriptManager.RegisterArrayDeclaration(control, arrayName, arrayValue);
            }
            else
            {
                this.RegisterArrayDeclaration(arrayName, arrayValue);
            }
        }

        public void RegisterClientScriptBlock(Type type, string key, string script)
        {
            this.RegisterClientScriptBlock(type, key, script, false);
        }

        public void RegisterClientScriptBlock(Type type, string key, string script, bool addScriptTags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (addScriptTags)
            {
                this.RegisterScriptBlock(CreateScriptKey(type, key), script, ClientAPIRegisterType.ClientScriptBlocksWithoutTags);
            }
            else
            {
                this.RegisterScriptBlock(CreateScriptKey(type, key), script, ClientAPIRegisterType.ClientScriptBlocks);
            }
        }

        internal void RegisterClientScriptBlock(Control control, Type type, string key, string script, bool addScriptTags)
        {
            IScriptManager scriptManager = this._owner.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
            {
                scriptManager.RegisterClientScriptBlock(control, type, key, script, addScriptTags);
            }
            else
            {
                this.RegisterClientScriptBlock(type, key, script, addScriptTags);
            }
        }

        public void RegisterClientScriptInclude(string key, string url)
        {
            this.RegisterClientScriptInclude(typeof(Page), key, url);
        }

        public void RegisterClientScriptInclude(Type type, string key, string url)
        {
            this.RegisterClientScriptInclude(type, key, url, false);
        }

        internal void RegisterClientScriptInclude(Type type, string key, string url, bool isResource)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(url))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("url");
            }
            string script = "\r\n<script src=\"" + HttpUtility.HtmlAttributeEncode(url) + "\" type=\"text/javascript\"></script>";
            this.RegisterScriptBlock(CreateScriptIncludeKey(type, key, isResource), script, ClientAPIRegisterType.ClientScriptBlocks);
        }

        internal void RegisterClientScriptInclude(Control control, Type type, string key, string url)
        {
            IScriptManager scriptManager = this._owner.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
            {
                scriptManager.RegisterClientScriptInclude(control, type, key, url);
            }
            else
            {
                this.RegisterClientScriptInclude(type, key, url);
            }
        }

        public void RegisterClientScriptResource(Type type, string resourceName)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.RegisterClientScriptInclude(type, resourceName, this.GetWebResourceUrl(type, resourceName), true);
        }

        internal void RegisterClientScriptResource(Control control, Type type, string resourceName)
        {
            IScriptManager scriptManager = this._owner.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
            {
                scriptManager.RegisterClientScriptResource(control, type, resourceName);
            }
            else
            {
                this.RegisterClientScriptResource(type, resourceName);
            }
        }

        internal void RegisterDefaultButtonScript(Control button, HtmlTextWriter writer, bool useAddAttribute)
        {
            this._owner.RegisterWebFormsScript();
            if (this._owner.EnableLegacyRendering)
            {
                if (useAddAttribute)
                {
                    writer.AddAttribute("language", "javascript", false);
                }
                else
                {
                    writer.WriteAttribute("language", "javascript", false);
                }
            }
            string str = "javascript:return WebForm_FireDefaultButton(event, '" + button.ClientID + "')";
            if (useAddAttribute)
            {
                writer.AddAttribute("onkeypress", str);
            }
            else
            {
                writer.WriteAttribute("onkeypress", str);
            }
        }

        public void RegisterExpandoAttribute(string controlId, string attributeName, string attributeValue)
        {
            this.RegisterExpandoAttribute(controlId, attributeName, attributeValue, true);
        }

        public void RegisterExpandoAttribute(string controlId, string attributeName, string attributeValue, bool encode)
        {
            StringUtil.CheckAndTrimString(controlId, "controlId");
            StringUtil.CheckAndTrimString(attributeName, "attributeName");
            ListDictionary dictionary = null;
            if (this._registeredControlsWithExpandoAttributes == null)
            {
                this._registeredControlsWithExpandoAttributes = new ListDictionary(StringComparer.Ordinal);
            }
            else
            {
                dictionary = (ListDictionary) this._registeredControlsWithExpandoAttributes[controlId];
            }
            if (dictionary == null)
            {
                dictionary = new ListDictionary(StringComparer.Ordinal);
                this._registeredControlsWithExpandoAttributes.Add(controlId, dictionary);
            }
            if (encode)
            {
                attributeValue = Util.QuoteJScriptString(attributeValue);
            }
            dictionary.Add(attributeName, attributeValue);
            if (this._owner.PartialCachingControlStack != null)
            {
                foreach (BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    control.RegisterExpandoAttribute(controlId, attributeName, attributeValue);
                }
            }
        }

        internal void RegisterExpandoAttribute(Control control, string controlId, string attributeName, string attributeValue, bool encode)
        {
            IScriptManager scriptManager = this._owner.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
            {
                scriptManager.RegisterExpandoAttribute(control, controlId, attributeName, attributeValue, encode);
            }
            else
            {
                this.RegisterExpandoAttribute(controlId, attributeName, attributeValue, encode);
            }
        }

        public void RegisterForEventValidation(string uniqueId)
        {
            this.RegisterForEventValidation(uniqueId, string.Empty);
        }

        public void RegisterForEventValidation(PostBackOptions options)
        {
            this.RegisterForEventValidation(options.TargetControl.UniqueID, options.Argument);
        }

        public void RegisterForEventValidation(string uniqueId, string argument)
        {
            if ((this._owner.EnableEventValidation && !this._owner.DesignMode) && !string.IsNullOrEmpty(uniqueId))
            {
                if ((this._owner.ControlState < ControlState.PreRendered) && !this._owner.IsCallback)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ClientScriptManager_RegisterForEventValidation_Too_Early"));
                }
                int num = ComputeHashKey(uniqueId, argument);
                string clientState = this._owner.ClientState;
                if (clientState == null)
                {
                    clientState = string.Empty;
                }
                if (this._validEventReferences == null)
                {
                    if (this._owner.IsCallback)
                    {
                        this.EnsureEventValidationFieldLoaded();
                        if (this._validEventReferences == null)
                        {
                            this._validEventReferences = new ArrayList();
                        }
                    }
                    else
                    {
                        this._validEventReferences = new ArrayList();
                        this._validEventReferences.Add(StringUtil.GetStringHashCode(clientState));
                    }
                }
                this._validEventReferences.Add(num);
                if (this._owner.PartialCachingControlStack != null)
                {
                    foreach (BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                    {
                        control.RegisterForEventValidation(uniqueId, argument);
                    }
                }
            }
        }

        public void RegisterHiddenField(string hiddenFieldName, string hiddenFieldInitialValue)
        {
            if (hiddenFieldName == null)
            {
                throw new ArgumentNullException("hiddenFieldName");
            }
            if (this._registeredHiddenFields == null)
            {
                this._registeredHiddenFields = new ListDictionary();
            }
            if (!this._registeredHiddenFields.Contains(hiddenFieldName))
            {
                this._registeredHiddenFields.Add(hiddenFieldName, hiddenFieldInitialValue);
            }
            if (this._owner.PartialCachingControlStack != null)
            {
                foreach (BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    control.RegisterHiddenField(hiddenFieldName, hiddenFieldInitialValue);
                }
            }
        }

        internal void RegisterHiddenField(Control control, string hiddenFieldName, string hiddenFieldValue)
        {
            IScriptManager scriptManager = this._owner.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
            {
                scriptManager.RegisterHiddenField(control, hiddenFieldName, hiddenFieldValue);
            }
            else
            {
                this.RegisterHiddenField(hiddenFieldName, hiddenFieldValue);
            }
        }

        public void RegisterOnSubmitStatement(Type type, string key, string script)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.RegisterOnSubmitStatementInternal(CreateScriptKey(type, key), script);
        }

        internal void RegisterOnSubmitStatement(Control control, Type type, string key, string script)
        {
            IScriptManager scriptManager = this._owner.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
            {
                scriptManager.RegisterOnSubmitStatement(control, type, key, script);
            }
            else
            {
                this.RegisterOnSubmitStatement(type, key, script);
            }
        }

        internal void RegisterOnSubmitStatementInternal(ScriptKey key, string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("script");
            }
            if (this._registeredOnSubmitStatements == null)
            {
                this._registeredOnSubmitStatements = new ListDictionary();
            }
            int index = script.Length - 1;
            while ((index >= 0) && char.IsWhiteSpace(script, index))
            {
                index--;
            }
            if ((index >= 0) && (script[index] != ';'))
            {
                script = script.Substring(0, index + 1) + ";" + script.Substring(index + 1);
            }
            if (!this._registeredOnSubmitStatements.Contains(key))
            {
                this._registeredOnSubmitStatements.Add(key, script);
            }
            if (this._owner.PartialCachingControlStack != null)
            {
                foreach (BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    control.RegisterOnSubmitStatement(key, script);
                }
            }
        }

        internal void RegisterScriptBlock(ScriptKey key, string script, ClientAPIRegisterType type)
        {
            switch (type)
            {
                case ClientAPIRegisterType.ClientScriptBlocks:
                    this.RegisterScriptBlock(key, script, ref this._registeredClientScriptBlocks, ref this._clientScriptBlocks, false);
                    break;

                case ClientAPIRegisterType.ClientScriptBlocksWithoutTags:
                    this.RegisterScriptBlock(key, script, ref this._registeredClientScriptBlocks, ref this._clientScriptBlocks, true);
                    break;

                case ClientAPIRegisterType.ClientStartupScripts:
                    this.RegisterScriptBlock(key, script, ref this._registeredClientStartupScripts, ref this._clientStartupScripts, false);
                    break;

                case ClientAPIRegisterType.ClientStartupScriptsWithoutTags:
                    this.RegisterScriptBlock(key, script, ref this._registeredClientStartupScripts, ref this._clientStartupScripts, true);
                    break;
            }
            if (this._owner.PartialCachingControlStack != null)
            {
                foreach (BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    control.RegisterScriptBlock(type, key, script);
                }
            }
        }

        private void RegisterScriptBlock(ScriptKey key, string script, ref ListDictionary scriptBlocks, ref ArrayList scriptList, bool needsScriptTags)
        {
            if (scriptBlocks == null)
            {
                scriptBlocks = new ListDictionary();
                scriptList = new ArrayList();
            }
            if (!scriptBlocks.Contains(key))
            {
                Tuple<ScriptKey, string, bool> tuple = new Tuple<ScriptKey, string, bool>(key, script, needsScriptTags);
                scriptBlocks.Add(key, null);
                scriptList.Add(tuple);
            }
        }

        public void RegisterStartupScript(Type type, string key, string script)
        {
            this.RegisterStartupScript(type, key, script, false);
        }

        public void RegisterStartupScript(Type type, string key, string script, bool addScriptTags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (addScriptTags)
            {
                this.RegisterScriptBlock(CreateScriptKey(type, key), script, ClientAPIRegisterType.ClientStartupScriptsWithoutTags);
            }
            else
            {
                this.RegisterScriptBlock(CreateScriptKey(type, key), script, ClientAPIRegisterType.ClientStartupScripts);
            }
        }

        internal void RegisterStartupScript(Control control, Type type, string key, string script, bool addScriptTags)
        {
            IScriptManager scriptManager = this._owner.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
            {
                scriptManager.RegisterStartupScript(control, type, key, script, addScriptTags);
            }
            else
            {
                this.RegisterStartupScript(type, key, script, addScriptTags);
            }
        }

        internal void RenderArrayDeclares(HtmlTextWriter writer)
        {
            if ((this._registeredArrayDeclares != null) && (this._registeredArrayDeclares.Count != 0))
            {
                writer.Write(this._owner.EnableLegacyRendering ? "\r\n<script type=\"text/javascript\">\r\n<!--\r\n" : "\r\n<script type=\"text/javascript\">\r\n//<![CDATA[\r\n");
                IDictionaryEnumerator enumerator = this._registeredArrayDeclares.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    writer.Write("var ");
                    writer.Write(enumerator.Key);
                    writer.Write(" =  new Array(");
                    IEnumerator enumerator2 = ((ArrayList) enumerator.Value).GetEnumerator();
                    bool flag = true;
                    while (enumerator2.MoveNext())
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            writer.Write(", ");
                        }
                        writer.Write(enumerator2.Current);
                    }
                    writer.WriteLine(");");
                }
                writer.Write(this._owner.EnableLegacyRendering ? "// -->\r\n</script>\r\n" : "//]]>\r\n</script>\r\n");
            }
        }

        internal void RenderClientScriptBlocks(HtmlTextWriter writer)
        {
            bool flag = false;
            if (this._clientScriptBlocks != null)
            {
                flag = this.RenderRegisteredScripts(writer, this._clientScriptBlocks, true);
            }
            if (!string.IsNullOrEmpty(this._owner.ClientOnSubmitEvent) && this._owner.ClientSupportsJavaScript)
            {
                if (!flag)
                {
                    writer.Write(this._owner.EnableLegacyRendering ? "\r\n<script type=\"text/javascript\">\r\n<!--\r\n" : "\r\n<script type=\"text/javascript\">\r\n//<![CDATA[\r\n");
                }
                writer.Write("function WebForm_OnSubmit() {\r\n");
                if (this._registeredOnSubmitStatements != null)
                {
                    foreach (string str in this._registeredOnSubmitStatements.Values)
                    {
                        writer.Write(str);
                    }
                }
                writer.WriteLine("\r\nreturn true;\r\n}");
                writer.Write(this._owner.EnableLegacyRendering ? "// -->\r\n</script>\r\n" : "//]]>\r\n</script>\r\n");
            }
            else if (flag)
            {
                writer.Write(this._owner.EnableLegacyRendering ? "// -->\r\n</script>\r\n" : "//]]>\r\n</script>\r\n");
            }
        }

        internal void RenderClientStartupScripts(HtmlTextWriter writer)
        {
            if ((this._clientStartupScripts != null) && this.RenderRegisteredScripts(writer, this._clientStartupScripts, false))
            {
                writer.Write(this._owner.EnableLegacyRendering ? "// -->\r\n</script>\r\n" : "//]]>\r\n</script>\r\n");
            }
        }

        internal void RenderExpandoAttribute(HtmlTextWriter writer)
        {
            if ((this._registeredControlsWithExpandoAttributes != null) && (this._registeredControlsWithExpandoAttributes.Count != 0))
            {
                writer.Write(this._owner.EnableLegacyRendering ? "\r\n<script type=\"text/javascript\">\r\n<!--\r\n" : "\r\n<script type=\"text/javascript\">\r\n//<![CDATA[\r\n");
                foreach (DictionaryEntry entry in this._registeredControlsWithExpandoAttributes)
                {
                    string key = (string) entry.Key;
                    writer.Write("var ");
                    writer.Write(key);
                    writer.Write(" = document.all ? document.all[\"");
                    writer.Write(key);
                    writer.Write("\"] : document.getElementById(\"");
                    writer.Write(key);
                    writer.WriteLine("\");");
                    ListDictionary dictionary = (ListDictionary) entry.Value;
                    foreach (DictionaryEntry entry2 in dictionary)
                    {
                        writer.Write(key);
                        writer.Write(".");
                        writer.Write(entry2.Key);
                        if (entry2.Value == null)
                        {
                            writer.WriteLine(" = null;");
                        }
                        else
                        {
                            writer.Write(" = \"");
                            writer.Write(entry2.Value);
                            writer.WriteLine("\";");
                        }
                    }
                }
                writer.Write(this._owner.EnableLegacyRendering ? "// -->\r\n</script>\r\n" : "//]]>\r\n</script>\r\n");
            }
        }

        internal void RenderHiddenFields(HtmlTextWriter writer)
        {
            if ((this._registeredHiddenFields != null) && (this._registeredHiddenFields.Count != 0))
            {
                foreach (DictionaryEntry entry in this._registeredHiddenFields)
                {
                    string key = (string) entry.Key;
                    if (key == null)
                    {
                        key = string.Empty;
                    }
                    writer.WriteLine();
                    writer.Write("<input type=\"hidden\" name=\"");
                    writer.Write(key);
                    writer.Write("\" id=\"");
                    writer.Write(key);
                    writer.Write("\" value=\"");
                    HttpUtility.HtmlEncode((string) entry.Value, writer);
                    writer.Write("\" />");
                }
                this.ClearHiddenFields();
            }
        }

        private bool RenderRegisteredScripts(HtmlTextWriter writer, ArrayList scripts, bool checkForScriptManagerRegistrations)
        {
            writer.WriteLine();
            bool flag = false;
            checkForScriptManagerRegistrations &= this._registeredResourcesToSuppress != null;
            foreach (Tuple<ScriptKey, string, bool> tuple in scripts)
            {
                if (checkForScriptManagerRegistrations)
                {
                    Dictionary<string, object> dictionary;
                    ScriptKey key = tuple.Item1;
                    if ((key.IsResource && this._registeredResourcesToSuppress.TryGetValue(key.Assembly, out dictionary)) && dictionary.ContainsKey(key.Key))
                    {
                        continue;
                    }
                }
                if (tuple.Item3)
                {
                    if (!flag)
                    {
                        writer.Write(this._owner.EnableLegacyRendering ? "\r\n<script type=\"text/javascript\">\r\n<!--\r\n" : "\r\n<script type=\"text/javascript\">\r\n//<![CDATA[\r\n");
                        flag = true;
                    }
                }
                else if (flag)
                {
                    writer.Write(this._owner.EnableLegacyRendering ? "// -->\r\n</script>\r\n" : "//]]>\r\n</script>\r\n");
                    flag = false;
                }
                writer.Write(tuple.Item2);
            }
            return flag;
        }

        internal void RenderWebFormsScript(HtmlTextWriter writer)
        {
            Dictionary<string, object> dictionary;
            if (((this._registeredResourcesToSuppress == null) || !this._registeredResourcesToSuppress.TryGetValue(AssemblyResourceLoader.GetAssemblyFromType(typeof(Page)), out dictionary)) || !dictionary.ContainsKey("WebForms.js"))
            {
                writer.Write("\r\n<script src=\"");
                writer.Write(GetWebResourceUrl(this._owner, typeof(Page), "WebForms.js", true, this._owner.ScriptManager));
                writer.WriteLine("\" type=\"text/javascript\"></script>");
            }
        }

        internal void SaveEventValidationField()
        {
            string eventValidationFieldValue = this.GetEventValidationFieldValue();
            if (!string.IsNullOrEmpty(eventValidationFieldValue))
            {
                this.RegisterHiddenField("__EVENTVALIDATION", eventValidationFieldValue);
            }
        }

        public void ValidateEvent(string uniqueId)
        {
            this.ValidateEvent(uniqueId, string.Empty);
        }

        public void ValidateEvent(string uniqueId, string argument)
        {
            if (this._owner.EnableEventValidation)
            {
                if (string.IsNullOrEmpty(uniqueId))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Parameter_NullOrEmpty", new object[] { "uniqueId" }), "uniqueId");
                }
                this.EnsureEventValidationFieldLoaded();
                if (this._clientPostBackValidatedEventTable == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ClientScriptManager_InvalidPostBackArgument"));
                }
                int key = ComputeHashKey(uniqueId, argument);
                if (!this._clientPostBackValidatedEventTable.Contains(key))
                {
                    throw new ArgumentException(System.Web.SR.GetString("ClientScriptManager_InvalidPostBackArgument"));
                }
            }
        }

        internal bool HasRegisteredHiddenFields
        {
            get
            {
                return ((this._registeredHiddenFields != null) && (this._registeredHiddenFields.Count > 0));
            }
        }

        internal bool HasSubmitStatements
        {
            get
            {
                return ((this._registeredOnSubmitStatements != null) && (this._registeredOnSubmitStatements.Count > 0));
            }
        }

        internal Dictionary<Assembly, Dictionary<string, object>> RegisteredResourcesToSuppress
        {
            get
            {
                if (this._registeredResourcesToSuppress == null)
                {
                    this._registeredResourcesToSuppress = new Dictionary<Assembly, Dictionary<string, object>>();
                }
                return this._registeredResourcesToSuppress;
            }
        }
    }
}

