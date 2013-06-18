namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Configuration;
    using System.Design;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class UserControlDesigner : ControlDesigner
    {
        private static IDictionary _antiRecursionDictionary = new HybridDictionary();
        private const string _dummyProtocolAndServer = "file://foo";
        private bool _userControlFound;
        private const string UserControlCacheKey = "__aspnetUserControlCache";

        private void EditUserControl()
        {
            IWebApplication service = (IWebApplication) base.Component.Site.GetService(typeof(IWebApplication));
            if (service != null)
            {
                IUserControlDesignerAccessor component = (IUserControlDesignerAccessor) base.Component;
                string[] strArray = component.TagName.Split(new char[] { ':' });
                string userControlPath = base.RootDesigner.ReferenceManager.GetUserControlPath(strArray[0], strArray[1]);
                if (!string.IsNullOrEmpty(userControlPath))
                {
                    userControlPath = this.MakeAppRelativePath(userControlPath);
                    IDocumentProjectItem projectItemFromUrl = service.GetProjectItemFromUrl(userControlPath) as IDocumentProjectItem;
                    if (projectItemFromUrl != null)
                    {
                        projectItemFromUrl.Open();
                    }
                }
            }
        }

        private string GenerateUserControlCacheKey(string userControlPath, IThemeResolutionService themeService)
        {
            string str = userControlPath;
            if (themeService != null)
            {
                ThemeProvider stylesheetThemeProvider = themeService.GetStylesheetThemeProvider();
                if ((stylesheetThemeProvider != null) && !string.IsNullOrEmpty(stylesheetThemeProvider.ThemeName))
                {
                    str = str + "|" + stylesheetThemeProvider.ThemeName;
                }
            }
            return str;
        }

        private string GenerateUserControlHashCode(string contents, IThemeResolutionService themeService)
        {
            string str = contents.GetHashCode().ToString(CultureInfo.InvariantCulture);
            if (themeService != null)
            {
                ThemeProvider stylesheetThemeProvider = themeService.GetStylesheetThemeProvider();
                if (stylesheetThemeProvider != null)
                {
                    str = str + "|" + stylesheetThemeProvider.ContentHashCode.ToString(CultureInfo.InvariantCulture);
                }
            }
            return str;
        }

        public override string GetDesignTimeHtml()
        {
            if (base.Component.Site != null)
            {
                IWebApplication application = (IWebApplication) base.Component.Site.GetService(typeof(IWebApplication));
                IDesignerHost host = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
                if (((application != null) && (host != null)) && (base.RootDesigner.ReferenceManager != null))
                {
                    IUserControlDesignerAccessor component = (IUserControlDesignerAccessor) base.Component;
                    string[] strArray = component.TagName.Split(new char[] { ':' });
                    string userControlPath = base.RootDesigner.ReferenceManager.GetUserControlPath(strArray[0], strArray[1]);
                    userControlPath = this.MakeAppRelativePath(userControlPath);
                    IThemeResolutionService themeService = (IThemeResolutionService) base.Component.Site.GetService(typeof(IThemeResolutionService));
                    string key = this.GenerateUserControlCacheKey(userControlPath, themeService);
                    if (!string.IsNullOrEmpty(userControlPath))
                    {
                        string b = null;
                        string second = string.Empty;
                        bool flag = false;
                        IDictionary dictionary = _antiRecursionDictionary;
                        IDictionaryService service = (IDictionaryService) application.GetService(typeof(IDictionaryService));
                        if (service != null)
                        {
                            dictionary = (IDictionary) service.GetValue("__aspnetUserControlCache");
                            if (dictionary == null)
                            {
                                dictionary = new HybridDictionary();
                                service.SetValue("__aspnetUserControlCache", dictionary);
                            }
                            Pair pair = (Pair) dictionary[key];
                            if (pair != null)
                            {
                                b = (string) pair.First;
                                second = (string) pair.Second;
                                flag = second.Contains("mvwres:");
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        IDocumentProjectItem projectItemFromUrl = application.GetProjectItemFromUrl(userControlPath) as IDocumentProjectItem;
                        if (projectItemFromUrl != null)
                        {
                            this._userControlFound = true;
                            string contents = new StreamReader(projectItemFromUrl.GetContents()).ReadToEnd();
                            string a = null;
                            if (!flag)
                            {
                                a = this.GenerateUserControlHashCode(contents, themeService);
                                flag = !string.Equals(a, b, StringComparison.OrdinalIgnoreCase) || contents.Contains(".ascx");
                            }
                            if (flag)
                            {
                                if (_antiRecursionDictionary.Contains(key))
                                {
                                    return base.CreateErrorDesignTimeHtml(System.Design.SR.GetString("UserControlDesigner_CyclicError"));
                                }
                                _antiRecursionDictionary[key] = base.CreateErrorDesignTimeHtml(System.Design.SR.GetString("UserControlDesigner_CyclicError"));
                                second = string.Empty;
                                Pair pair2 = new Pair();
                                if (a == null)
                                {
                                    a = this.GenerateUserControlHashCode(contents, themeService);
                                }
                                pair2.First = a;
                                pair2.Second = second;
                                dictionary[key] = pair2;
                                UserControl child = (UserControl) base.Component;
                                Page rootComponent = new Page();
                                try
                                {
                                    rootComponent.Controls.Add(child);
                                    IDesignerHost host2 = new UserControlDesignerHost(host, rootComponent, userControlPath);
                                    if (!string.IsNullOrEmpty(contents))
                                    {
                                        List<Triplet> userControlRegisterEntries = new List<Triplet>();
                                        Control[] controlArray = ControlSerializer.DeserializeControlsInternal(contents, host2, userControlRegisterEntries);
                                        foreach (Control control2 in controlArray)
                                        {
                                            if ((!(control2 is LiteralControl) && !(control2 is DesignerDataBoundLiteralControl)) && !(control2 is DataBoundLiteralControl))
                                            {
                                                if (string.IsNullOrEmpty(control2.ID))
                                                {
                                                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("UserControlDesigner_MissingID"), new object[] { control2.GetType().Name }));
                                                }
                                                host2.Container.Add(control2);
                                            }
                                            child.Controls.Add(control2);
                                        }
                                        foreach (Triplet triplet in userControlRegisterEntries)
                                        {
                                            string first = (string) triplet.First;
                                            Pair pair3 = (Pair) triplet.Second;
                                            Pair third = (Pair) triplet.Third;
                                            if (pair3 != null)
                                            {
                                                string tagName = (string) pair3.First;
                                                string src = (string) pair3.Second;
                                                ((UserControlDesignerHost) host2).RegisterUserControl(first, tagName, src);
                                            }
                                            else if (third != null)
                                            {
                                                string tagNamespace = (string) third.First;
                                                string assemblyName = (string) third.Second;
                                                ((UserControlDesignerHost) host2).RegisterTagNamespace(first, tagNamespace, assemblyName);
                                            }
                                        }
                                        StringBuilder builder = new StringBuilder();
                                        foreach (Control control3 in controlArray)
                                        {
                                            if (control3 is LiteralControl)
                                            {
                                                builder.Append(((LiteralControl) control3).Text);
                                            }
                                            else if (control3 is DesignerDataBoundLiteralControl)
                                            {
                                                builder.Append(((DesignerDataBoundLiteralControl) control3).Text);
                                            }
                                            else if (control3 is DataBoundLiteralControl)
                                            {
                                                builder.Append(((DataBoundLiteralControl) control3).Text);
                                            }
                                            else if (control3 is HtmlControl)
                                            {
                                                StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
                                                DesignTimeHtmlTextWriter writer2 = new DesignTimeHtmlTextWriter(writer);
                                                control3.RenderControl(writer2);
                                                builder.Append(writer.GetStringBuilder().ToString());
                                            }
                                            else
                                            {
                                                ViewRendering viewRendering = ((ControlDesigner) host2.GetDesigner(control3)).GetViewRendering();
                                                builder.Append(viewRendering.Content);
                                            }
                                        }
                                        second = builder.ToString();
                                    }
                                    pair2.Second = second;
                                }
                                catch
                                {
                                    dictionary.Remove(key);
                                    throw;
                                }
                                finally
                                {
                                    _antiRecursionDictionary.Remove(key);
                                    child.Controls.Clear();
                                    rootComponent.Controls.Remove(child);
                                }
                            }
                        }
                        else
                        {
                            second = base.CreateErrorDesignTimeHtml(System.Design.SR.GetString("UserControlDesigner_NotFound", new object[] { userControlPath }));
                        }
                        if (second.Trim().Length > 0)
                        {
                            return second;
                        }
                    }
                }
            }
            return base.CreatePlaceHolderDesignTimeHtml();
        }

        internal override string GetPersistInnerHtmlInternal()
        {
            if (base.Component.GetType() == typeof(UserControl))
            {
                return null;
            }
            return base.GetPersistInnerHtmlInternal();
        }

        private string MakeAppRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith("~", StringComparison.Ordinal))
            {
                return path;
            }
            string directoryName = Path.GetDirectoryName(base.RootDesigner.DocumentUrl);
            if (string.IsNullOrEmpty(directoryName))
            {
                directoryName = "~";
            }
            directoryName = directoryName.Replace('\\', '/').Replace("~", "file://foo");
            path = path.Replace('\\', '/');
            Uri uri = new Uri(directoryName + "/" + path);
            return uri.ToString().Replace("file://foo", "~");
        }

        private void Refresh()
        {
            this.UpdateDesignTimeHtml();
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new UserControlDesignerActionList(this));
                return lists;
            }
        }

        public override bool AllowResize
        {
            get
            {
                return false;
            }
        }

        internal override bool ShouldCodeSerializeInternal
        {
            get
            {
                if (base.Component.GetType() == typeof(UserControl))
                {
                    return false;
                }
                return base.ShouldCodeSerializeInternal;
            }
            set
            {
                base.ShouldCodeSerializeInternal = value;
            }
        }

        private sealed class DummyRootDesigner : WebFormsRootDesigner
        {
            private string _documentUrl;
            internal WebFormsRootDesigner _rootDesigner;
            private IList<UserControlDesigner.TagNamespaceRegisterEntry> _tagNamespaceRegisterEntries;
            private IDictionary<string, string> _userControlRegisterEntries;

            public DummyRootDesigner(WebFormsRootDesigner rootDesigner, IDictionary<string, string> userControlRegisterEntries, IList<UserControlDesigner.TagNamespaceRegisterEntry> tagNamespaceRegisterEntries, string documentUrl)
            {
                this._rootDesigner = rootDesigner;
                this._userControlRegisterEntries = userControlRegisterEntries;
                this._tagNamespaceRegisterEntries = tagNamespaceRegisterEntries;
                this._documentUrl = documentUrl;
            }

            public override void AddClientScriptToDocument(ClientScriptItem scriptItem)
            {
                throw new NotSupportedException();
            }

            public override string AddControlToDocument(Control newControl, Control referenceControl, ControlLocation location)
            {
                throw new NotSupportedException();
            }

            public override ClientScriptItemCollection GetClientScriptsInDocument()
            {
                throw new NotSupportedException();
            }

            protected internal override void GetControlViewAndTag(Control control, out IControlDesignerView view, out IControlDesignerTag tag)
            {
                view = null;
                tag = null;
            }

            public override void RemoveClientScriptFromDocument(string clientScriptId)
            {
                throw new NotSupportedException();
            }

            public override void RemoveControlFromDocument(Control control)
            {
                throw new NotSupportedException();
            }

            public override string DocumentUrl
            {
                get
                {
                    return this._documentUrl;
                }
            }

            public override bool IsDesignerViewLocked
            {
                get
                {
                    return true;
                }
            }

            public override bool IsLoading
            {
                get
                {
                    return this._rootDesigner.IsLoading;
                }
            }

            public override WebFormsReferenceManager ReferenceManager
            {
                get
                {
                    return new DummyWebFormsReferenceManager(this, this._rootDesigner.ReferenceManager, this._userControlRegisterEntries, this._tagNamespaceRegisterEntries);
                }
            }

            internal IWebApplication WebApplication
            {
                get
                {
                    if (this._rootDesigner != null)
                    {
                        return (IWebApplication) this._rootDesigner.GetService(typeof(IWebApplication));
                    }
                    return null;
                }
            }

            private sealed class DummyWebFormsReferenceManager : WebFormsReferenceManager
            {
                private WebFormsReferenceManager _baseReferenceManager;
                private IDictionary<string, string> _baseUserControlRegisterEntries;
                private UserControlDesigner.DummyRootDesigner _owner;
                private Collection<string> _registerDirectives;
                private IList<UserControlDesigner.TagNamespaceRegisterEntry> _tagNamespaceRegisterEntries;
                private static readonly string[] FrameworkTagPrefixAssemblySpecs = new string[] { GetAssemblySpec(new AssemblyName("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")), GetAssemblySpec(new AssemblyName("System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")), GetAssemblySpec(new AssemblyName("System.Web.DynamicData, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")), GetAssemblySpec(new AssemblyName("System.Web.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")) };

                public DummyWebFormsReferenceManager(UserControlDesigner.DummyRootDesigner owner, WebFormsReferenceManager baseReferenceManager, IDictionary<string, string> baseUserControlRegisterEntries, IList<UserControlDesigner.TagNamespaceRegisterEntry> tagNamespaceRegisterEntries)
                {
                    this._owner = owner;
                    this._baseReferenceManager = baseReferenceManager;
                    this._baseUserControlRegisterEntries = baseUserControlRegisterEntries;
                    this._tagNamespaceRegisterEntries = tagNamespaceRegisterEntries;
                }

                private static string EncodeHexString(byte[] sArray)
                {
                    string str = null;
                    if (sArray == null)
                    {
                        return str;
                    }
                    char[] chArray = new char[sArray.Length * 2];
                    int index = 0;
                    int num3 = 0;
                    while (index < sArray.Length)
                    {
                        int num = (sArray[index] & 240) >> 4;
                        chArray[num3++] = HexDigit(num);
                        num = sArray[index] & 15;
                        chArray[num3++] = HexDigit(num);
                        index++;
                    }
                    return new string(chArray);
                }

                private string GenerateRegisterDirective(string tagPrefixAndName, string src)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("<%@ Register");
                    if (!string.IsNullOrEmpty(tagPrefixAndName))
                    {
                        string[] strArray = tagPrefixAndName.Split(new char[] { ':' });
                        if (strArray.Length == 2)
                        {
                            builder.Append(" TagPrefix=\"");
                            builder.Append(strArray[0]);
                            builder.Append("\"");
                            builder.Append(" TagName=\"");
                            builder.Append(strArray[1]);
                            builder.Append("\"");
                        }
                    }
                    if (!string.IsNullOrEmpty(src))
                    {
                        builder.Append(" Src=\"");
                        builder.Append(src);
                        builder.Append("\"");
                    }
                    builder.Append("%>");
                    return builder.ToString();
                }

                private string GenerateRegisterDirective(string tagPrefix, string tagName, string ns, string assembly, string src)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("<%@ Register");
                    if ((tagPrefix != null) && (tagPrefix.Length > 0))
                    {
                        builder.Append(" TagPrefix=\"");
                        builder.Append(tagPrefix);
                        builder.Append("\"");
                    }
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        builder.Append(" TagName=\"");
                        builder.Append(tagName);
                        builder.Append("\"");
                    }
                    if (ns != null)
                    {
                        builder.Append(" Namespace=\"");
                        builder.Append(ns);
                        builder.Append("\"");
                    }
                    if (!string.IsNullOrEmpty(assembly))
                    {
                        builder.Append(" Assembly=\"");
                        builder.Append(assembly);
                        builder.Append("\"");
                    }
                    if (!string.IsNullOrEmpty(src))
                    {
                        builder.Append(" Src=\"");
                        builder.Append(src);
                        builder.Append("\"");
                    }
                    builder.Append("%>");
                    return builder.ToString();
                }

                private static string GetAssemblySpec(AssemblyName assemblyName)
                {
                    string str = ((assemblyName.CultureInfo == null) || string.IsNullOrEmpty(assemblyName.CultureInfo.Name)) ? "neutral" : assemblyName.CultureInfo.Name;
                    string str2 = assemblyName.Name ?? string.Empty;
                    string str3 = str2 + ", Culture=" + str;
                    string str4 = EncodeHexString(assemblyName.GetPublicKeyToken());
                    if (str4 != null)
                    {
                        str3 = str3 + ", PublicKeyToken=" + ((str4.Length == 0) ? "null" : str4);
                    }
                    return str3;
                }

                private bool GetNamespaceAndAssemblyFromType(Type objectType, out string ns, out string asmName)
                {
                    if (objectType != null)
                    {
                        Assembly assembly = objectType.Module.Assembly;
                        if (assembly.GlobalAssemblyCache)
                        {
                            asmName = assembly.FullName;
                        }
                        else
                        {
                            asmName = assembly.GetName().Name;
                        }
                        ns = objectType.Namespace;
                        if (ns == null)
                        {
                            ns = string.Empty;
                        }
                        ns = ns.TrimEnd(new char[] { '.' });
                        if (((ns != null) && (asmName != null)) && (asmName.Length > 0))
                        {
                            return true;
                        }
                    }
                    ns = null;
                    asmName = null;
                    return false;
                }

                public override ICollection GetRegisterDirectives()
                {
                    if (this._registerDirectives == null)
                    {
                        try
                        {
                            this._registerDirectives = new Collection<string>();
                            IWebApplication webApplication = this._owner.WebApplication;
                            if (webApplication != null)
                            {
                                System.Configuration.Configuration configuration = webApplication.OpenWebConfiguration(true);
                                if (configuration != null)
                                {
                                    PagesSection section = (PagesSection) configuration.GetSection("system.web/pages");
                                    if (section != null)
                                    {
                                        string filePath = configuration.FilePath;
                                        string physicalPath = webApplication.RootProjectItem.PhysicalPath;
                                        string baseURL = "~/" + filePath.Substring(physicalPath.Length, filePath.Length - physicalPath.Length);
                                        foreach (TagPrefixInfo info in section.Controls)
                                        {
                                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                            info.Source = ResolveFileUrl(baseURL, info.Source);
                                            foreach (PropertyInformation information2 in info.ElementInformation.Properties)
                                            {
                                                if (information2.Type == typeof(string))
                                                {
                                                    dictionary[information2.Name] = (information2.ValueOrigin != PropertyValueOrigin.Default) ? ((string) information2.Value) : null;
                                                }
                                            }
                                            this._registerDirectives.Add(this.GenerateRegisterDirective(dictionary["tagPrefix"], dictionary["tagName"], dictionary["namespace"], dictionary["assembly"], dictionary["src"]));
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if (this._baseUserControlRegisterEntries != null)
                        {
                            foreach (KeyValuePair<string, string> pair in this._baseUserControlRegisterEntries)
                            {
                                string item = this.GenerateRegisterDirective(pair.Key, pair.Value);
                                if (!this._registerDirectives.Contains(item))
                                {
                                    this._registerDirectives.Add(item);
                                }
                            }
                        }
                        if (this._tagNamespaceRegisterEntries != null)
                        {
                            foreach (UserControlDesigner.TagNamespaceRegisterEntry entry in this._tagNamespaceRegisterEntries)
                            {
                                string str5 = this.GenerateRegisterDirective(entry.TagPrefix, null, entry.TagNamespace, entry.AssemblyName, null);
                                if (!this._registerDirectives.Contains(str5))
                                {
                                    this._registerDirectives.Add(str5);
                                }
                            }
                        }
                    }
                    return this._registerDirectives;
                }

                public override string GetTagPrefix(Type objectType)
                {
                    string str;
                    string str2;
                    if (this.GetNamespaceAndAssemblyFromType(objectType, out str, out str2))
                    {
                        string tagPrefix = null;
                        string str4 = null;
                        if ((str != null) && (str2 != null))
                        {
                            string assemblySpec = GetAssemblySpec(objectType.Module.Assembly.GetName());
                            foreach (UserControlDesigner.TagNamespaceRegisterEntry entry in this._tagNamespaceRegisterEntries)
                            {
                                if (string.Equals(str, entry.TagNamespace, StringComparison.OrdinalIgnoreCase))
                                {
                                    string assemblyName = entry.AssemblyName;
                                    if (!string.IsNullOrEmpty(assemblyName))
                                    {
                                        if (string.Equals(str2, assemblyName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            tagPrefix = entry.TagPrefix;
                                        }
                                        else
                                        {
                                            string spec = null;
                                            try
                                            {
                                                spec = GetAssemblySpec(new AssemblyName(assemblyName));
                                            }
                                            catch
                                            {
                                            }
                                            if (((spec == null) || !IsFrameworkTagPrefixAssembly(spec)) || !string.Equals(spec, assemblySpec, StringComparison.OrdinalIgnoreCase))
                                            {
                                                continue;
                                            }
                                            tagPrefix = entry.TagPrefix;
                                        }
                                        break;
                                    }
                                    if (str4 == null)
                                    {
                                        str4 = entry.TagPrefix;
                                    }
                                }
                            }
                            if (tagPrefix != null)
                            {
                                return tagPrefix;
                            }
                            if (str4 != null)
                            {
                                return str4;
                            }
                            return string.Empty;
                        }
                    }
                    return this._baseReferenceManager.GetTagPrefix(objectType);
                }

                public override Type GetType(string tagPrefix, string tagName)
                {
                    return this._baseReferenceManager.GetType(tagPrefix, tagName);
                }

                public override string GetUserControlPath(string tagPrefix, string tagName)
                {
                    return this._owner._userControlRegisterEntries[tagPrefix + ":" + tagName];
                }

                private static char HexDigit(int num)
                {
                    return ((num < 10) ? ((char) (num + 0x30)) : ((char) (num + 0x57)));
                }

                private static bool IsAppRelativePath(string path)
                {
                    if ((path.Length < 2) || (path[0] != '~'))
                    {
                        return false;
                    }
                    if (path[1] != '/')
                    {
                        return (path[1] == '\\');
                    }
                    return true;
                }

                private static bool IsFrameworkTagPrefixAssembly(string spec)
                {
                    return FrameworkTagPrefixAssemblySpecs.Contains<string>(spec, StringComparer.OrdinalIgnoreCase);
                }

                private static bool IsRooted(string basepath)
                {
                    if ((((basepath != null) && (basepath.Length != 0)) && ((basepath[0] != '/') && (basepath[0] != '\\'))) && !Path.IsPathRooted(basepath))
                    {
                        return (basepath.IndexOf(Path.VolumeSeparatorChar) >= 0);
                    }
                    return true;
                }

                public override string RegisterTagPrefix(Type objectType)
                {
                    throw new NotSupportedException();
                }

                private static string ResolveFileUrl(string baseURL, string relativeFileUrl)
                {
                    if (!IsRooted(relativeFileUrl) && !IsAppRelativePath(relativeFileUrl))
                    {
                        string fileName = Path.GetFileName(baseURL);
                        int length = baseURL.LastIndexOf(fileName, StringComparison.Ordinal);
                        relativeFileUrl = Path.Combine(baseURL.Substring(0, length), relativeFileUrl);
                    }
                    return relativeFileUrl;
                }
            }
        }

        private sealed class DummySite : ISite, IServiceProvider
        {
            private IComponent _component;
            private IContainer _container;
            private IDesignerHost _designerHost;
            private string _name;

            public DummySite(IComponent component, UserControlDesigner.UserControlDesignerHost designerHost)
            {
                this._component = component;
                this._container = designerHost;
                this._designerHost = designerHost;
            }

            object IServiceProvider.GetService(Type type)
            {
                return this._designerHost.GetService(type);
            }

            IComponent ISite.Component
            {
                get
                {
                    return this._component;
                }
            }

            IContainer ISite.Container
            {
                get
                {
                    return this._container;
                }
            }

            bool ISite.DesignMode
            {
                get
                {
                    return true;
                }
            }

            string ISite.Name
            {
                get
                {
                    return this._name;
                }
                set
                {
                    this._name = value;
                }
            }
        }

        private sealed class TagNamespaceRegisterEntry
        {
            public string AssemblyName;
            public string TagNamespace;
            public string TagPrefix;

            public TagNamespaceRegisterEntry(string tagPrefix, string tagNamespace, string assemblyName)
            {
                this.TagPrefix = tagPrefix;
                this.TagNamespace = tagNamespace;
                this.AssemblyName = assemblyName;
            }
        }

        private class UserControlDesignerActionList : DesignerActionList
        {
            private UserControlDesigner _parent;

            public UserControlDesignerActionList(UserControlDesigner parent) : base(parent.Component)
            {
                this._parent = parent;
            }

            public void EditUserControl()
            {
                this._parent.EditUserControl();
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                if (this._parent._userControlFound)
                {
                    items.Add(new DesignerActionMethodItem(this, "EditUserControl", System.Design.SR.GetString("UserControlDesigner_EditUserControl"), string.Empty, string.Empty, true));
                }
                items.Add(new DesignerActionMethodItem(this, "Refresh", System.Design.SR.GetString("UserControlDesigner_Refresh"), string.Empty, string.Empty, true));
                return items;
            }

            public void Refresh()
            {
                this._parent.Refresh();
            }

            public override bool AutoShow
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }
        }

        private sealed class UserControlDesignerHost : IContainer, IDesignerHost, IServiceContainer, IServiceProvider, IDisposable, IUrlResolutionService
        {
            private Hashtable _componentTable;
            private Hashtable _designerTable;
            private bool _disposed;
            private IDesignerHost _host;
            private int _nameCounter;
            private IComponent _rootComponent;
            private IList<UserControlDesigner.TagNamespaceRegisterEntry> _tagNamespaceRegisterEntries;
            private string _userControlPath;
            private IDictionary<string, string> _userControlRegisterEntries;
            private const char appRelativeCharacter = '~';
            private const string dummyProtocolAndServer = "file://foo";

            event EventHandler IDesignerHost.Activated
            {
                add
                {
                }
                remove
                {
                }
            }

            event EventHandler IDesignerHost.Deactivated
            {
                add
                {
                }
                remove
                {
                }
            }

            event EventHandler IDesignerHost.LoadComplete
            {
                add
                {
                    this._host.LoadComplete += value;
                }
                remove
                {
                    this._host.LoadComplete -= value;
                }
            }

            event DesignerTransactionCloseEventHandler IDesignerHost.TransactionClosed
            {
                add
                {
                }
                remove
                {
                }
            }

            event DesignerTransactionCloseEventHandler IDesignerHost.TransactionClosing
            {
                add
                {
                }
                remove
                {
                }
            }

            event EventHandler IDesignerHost.TransactionOpened
            {
                add
                {
                }
                remove
                {
                }
            }

            event EventHandler IDesignerHost.TransactionOpening
            {
                add
                {
                }
                remove
                {
                }
            }

            public UserControlDesignerHost(IDesignerHost host, IComponent rootComponent, string userControlPath)
            {
                this._host = host;
                this._componentTable = new Hashtable();
                this._designerTable = new Hashtable();
                this._rootComponent = rootComponent;
                this._userControlPath = userControlPath;
                this._rootComponent.Site = new UserControlDesigner.DummySite(this._rootComponent, this);
            }

            public void ClearComponents()
            {
                for (int i = 0; i < this.DesignerTable.Count; i++)
                {
                    if (this.DesignerTable[i] != null)
                    {
                        IDesigner designer = (IDesigner) this.DesignerTable[i];
                        try
                        {
                            designer.Dispose();
                        }
                        catch
                        {
                        }
                    }
                }
                this.DesignerTable.Clear();
                for (int j = 0; j < this.ComponentTable.Count; j++)
                {
                    if (this.ComponentTable[j] != null)
                    {
                        IComponent component = (IComponent) this.ComponentTable[j];
                        ISite site = component.Site;
                        try
                        {
                            component.Dispose();
                        }
                        catch
                        {
                        }
                        if (component.Site != null)
                        {
                            ((IContainer) this).Remove(component);
                        }
                    }
                }
                this.ComponentTable.Clear();
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void Dispose(bool disposing)
            {
                if (!this._disposed && disposing)
                {
                    this.ClearComponents();
                    this._host = null;
                    this._componentTable = null;
                    this._designerTable = null;
                }
                this._disposed = true;
            }

            ~UserControlDesignerHost()
            {
                this.Dispose(false);
            }

            private IComponent[] GetComponents()
            {
                int count = this.ComponentTable.Count;
                IComponent[] componentArray = new IComponent[count];
                if (count != 0)
                {
                    int num2 = 0;
                    foreach (IComponent component in this.ComponentTable.Values)
                    {
                        componentArray[num2++] = component;
                    }
                }
                return componentArray;
            }

            private static bool IsAppRelativePath(string path)
            {
                if ((path.Length < 2) || (path[0] != '~'))
                {
                    return false;
                }
                if (path[1] != '/')
                {
                    return (path[1] == '\\');
                }
                return true;
            }

            private static bool IsRooted(string basepath)
            {
                if (((basepath != null) && (basepath.Length != 0)) && (basepath[0] != '/'))
                {
                    return (basepath[0] == '\\');
                }
                return true;
            }

            public void RegisterTagNamespace(string tagPrefix, string tagNamespace, string assemblyName)
            {
                if (this._tagNamespaceRegisterEntries == null)
                {
                    this._tagNamespaceRegisterEntries = new List<UserControlDesigner.TagNamespaceRegisterEntry>();
                }
                this._tagNamespaceRegisterEntries.Add(new UserControlDesigner.TagNamespaceRegisterEntry(tagPrefix, tagNamespace, assemblyName));
            }

            public void RegisterUserControl(string tagPrefix, string tagName, string src)
            {
                if (this._userControlRegisterEntries == null)
                {
                    this._userControlRegisterEntries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                this._userControlRegisterEntries[tagPrefix + ":" + tagName] = src;
            }

            void IDesignerHost.Activate()
            {
            }

            IComponent IDesignerHost.CreateComponent(Type componentType)
            {
                return null;
            }

            IComponent IDesignerHost.CreateComponent(Type componentType, string name)
            {
                return null;
            }

            DesignerTransaction IDesignerHost.CreateTransaction()
            {
                return this._host.CreateTransaction();
            }

            DesignerTransaction IDesignerHost.CreateTransaction(string description)
            {
                return this._host.CreateTransaction(description);
            }

            void IDesignerHost.DestroyComponent(IComponent component)
            {
                ((IContainer) this).Remove(component);
            }

            IDesigner IDesignerHost.GetDesigner(IComponent component)
            {
                if (component == this._host.RootComponent)
                {
                    return this._host.GetDesigner(component);
                }
                if (component == this._rootComponent)
                {
                    return new UserControlDesigner.DummyRootDesigner((WebFormsRootDesigner) this._host.GetDesigner(this._host.RootComponent), this._userControlRegisterEntries, this._tagNamespaceRegisterEntries, this._userControlPath);
                }
                return (IDesigner) this.DesignerTable[component];
            }

            Type IDesignerHost.GetType(string typeName)
            {
                return this._host.GetType(typeName);
            }

            void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
            {
            }

            void IServiceContainer.AddService(Type serviceType, object serviceInstance)
            {
            }

            void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
            {
            }

            void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
            {
            }

            void IServiceContainer.RemoveService(Type serviceType)
            {
            }

            void IServiceContainer.RemoveService(Type serviceType, bool promote)
            {
            }

            void IContainer.Add(IComponent component)
            {
                ((IContainer) this).Add(component, null);
            }

            void IContainer.Add(IComponent component, string name)
            {
                if (component == null)
                {
                    throw new ArgumentNullException("component");
                }
                if (component.Site == null)
                {
                    component.Site = new UserControlDesigner.DummySite(component, this);
                    if (component is Control)
                    {
                        component.Site.Name = ((Control) component).ID;
                    }
                    else
                    {
                        component.Site.Name = "Temp" + this._nameCounter++;
                    }
                }
                if (name == null)
                {
                    name = component.Site.Name;
                }
                if (this.ComponentTable[name] != null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("UserControlDesignerHost_ComponentAlreadyExists"), new object[] { name }));
                }
                this.ComponentTable[name] = component;
                IDesigner designer = TypeDescriptor.CreateDesigner(component, typeof(IDesigner));
                designer.Initialize(component);
                this.DesignerTable[component] = designer;
                if (component is Control)
                {
                    ((Control) component).Page = (Page) this._rootComponent;
                }
            }

            void IContainer.Remove(IComponent component)
            {
                if (component == null)
                {
                    throw new ArgumentNullException("component");
                }
                if (component.Site != null)
                {
                    string name = component.Site.Name;
                    if ((name != null) && (this.ComponentTable[name] == component))
                    {
                        if (this.DesignerTable != null)
                        {
                            IDesigner designer = (IDesigner) this.DesignerTable[component];
                            if (designer != null)
                            {
                                this.DesignerTable.Remove(component);
                                designer.Dispose();
                            }
                        }
                        this.ComponentTable.Remove(name);
                        component.Dispose();
                        component.Site = null;
                    }
                }
            }

            void IDisposable.Dispose()
            {
                this.Dispose();
            }

            object IServiceProvider.GetService(Type serviceType)
            {
                if ((!(serviceType == typeof(IDesignerHost)) && !(serviceType == typeof(IContainer))) && !(serviceType == typeof(IUrlResolutionService)))
                {
                    return this._host.GetService(serviceType);
                }
                return this;
            }

            string IUrlResolutionService.ResolveClientUrl(string relativeUrl)
            {
                if (relativeUrl == null)
                {
                    throw new ArgumentNullException("relativeUrl");
                }
                if (!IsRooted(relativeUrl) && !relativeUrl.Contains("mvwres:"))
                {
                    IUrlResolutionService service = (IUrlResolutionService) this._host.GetService(typeof(IUrlResolutionService));
                    if (service == null)
                    {
                        return relativeUrl;
                    }
                    if (IsAppRelativePath(relativeUrl))
                    {
                        relativeUrl = service.ResolveClientUrl(relativeUrl);
                        return relativeUrl;
                    }
                    string path = this._userControlPath;
                    if ((path == null) || (path.Length == 0))
                    {
                        return relativeUrl;
                    }
                    if (IsAppRelativePath(path))
                    {
                        Uri uri = new Uri(path.Replace("~", "file://foo"));
                        string[] segments = uri.Segments;
                        StringBuilder builder = new StringBuilder("~");
                        for (int i = 0; i < (segments.Length - 1); i++)
                        {
                            builder.Append(segments[i]);
                        }
                        relativeUrl = service.ResolveClientUrl(builder.ToString() + relativeUrl);
                        return relativeUrl;
                    }
                    string fileName = Path.GetFileName(path);
                    int length = path.LastIndexOf(fileName, StringComparison.Ordinal);
                    relativeUrl = Path.Combine(path.Substring(0, length), relativeUrl);
                }
                return relativeUrl;
            }

            private Hashtable ComponentTable
            {
                get
                {
                    return this._componentTable;
                }
            }

            private Hashtable DesignerTable
            {
                get
                {
                    return this._designerTable;
                }
            }

            IContainer IDesignerHost.Container
            {
                get
                {
                    return this;
                }
            }

            bool IDesignerHost.InTransaction
            {
                get
                {
                    return this._host.InTransaction;
                }
            }

            bool IDesignerHost.Loading
            {
                get
                {
                    return this._host.Loading;
                }
            }

            IComponent IDesignerHost.RootComponent
            {
                get
                {
                    return this._rootComponent;
                }
            }

            string IDesignerHost.RootComponentClassName
            {
                get
                {
                    return this._rootComponent.GetType().Name;
                }
            }

            string IDesignerHost.TransactionDescription
            {
                get
                {
                    return this._host.TransactionDescription;
                }
            }

            ComponentCollection IContainer.Components
            {
                get
                {
                    return new ComponentCollection(this.GetComponents());
                }
            }
        }
    }
}

