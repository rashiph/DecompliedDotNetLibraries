namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    internal abstract class DependencyParser : BaseParser
    {
        private StringSet _circularReferenceChecker = new CaseInsensitiveStringSet();
        private PagesSection _pagesConfig;
        private VirtualPath _virtualPath;
        private StringSet _virtualPathDependencies;

        protected DependencyParser()
        {
        }

        protected void AddDependency(VirtualPath virtualPath)
        {
            virtualPath = base.ResolveVirtualPath(virtualPath);
            if (this._virtualPathDependencies == null)
            {
                this._virtualPathDependencies = new CaseInsensitiveStringSet();
            }
            this._virtualPathDependencies.Add(virtualPath.VirtualPathString);
        }

        internal ICollection GetVirtualPathDependencies()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            HttpRuntime.SetCurrentThreadCultureWithAssert(CultureInfo.InvariantCulture);
            try
            {
                try
                {
                    this.PrepareParse();
                    this.ParseFile();
                }
                finally
                {
                    HttpRuntime.SetCurrentThreadCultureWithAssert(currentCulture);
                }
            }
            catch
            {
                throw;
            }
            return this._virtualPathDependencies;
        }

        internal void Init(VirtualPath virtualPath)
        {
            base.CurrentVirtualPath = virtualPath;
            this._virtualPath = virtualPath;
            this._pagesConfig = MTConfigUtil.GetPagesConfig(virtualPath);
        }

        private void ParseFile()
        {
            this.ParseFile(null, this._virtualPath);
        }

        private void ParseFile(string physicalPath, VirtualPath virtualPath)
        {
            string o = (physicalPath != null) ? physicalPath : virtualPath.VirtualPathString;
            if (this._circularReferenceChecker.Contains(o))
            {
                throw new HttpException(System.Web.SR.GetString("Circular_include"));
            }
            this._circularReferenceChecker.Add(o);
            try
            {
                TextReader reader;
                if (physicalPath != null)
                {
                    using (reader = Util.ReaderFromFile(physicalPath, virtualPath))
                    {
                        this.ParseReader(reader);
                        return;
                    }
                }
                using (Stream stream = virtualPath.OpenFile())
                {
                    reader = Util.ReaderFromStream(stream, virtualPath);
                    this.ParseReader(reader);
                }
            }
            finally
            {
                this._circularReferenceChecker.Remove(o);
            }
        }

        private void ParseReader(TextReader input)
        {
            this.ParseString(input.ReadToEnd());
        }

        private void ParseString(string text)
        {
            Match match;
            int startat = 0;
        Label_0002:
            if ((match = BaseParser.textRegex.Match(text, startat)).Success)
            {
                startat = match.Index + match.Length;
            }
            if (startat != text.Length)
            {
                if ((match = BaseParser.directiveRegex.Match(text, startat)).Success)
                {
                    IDictionary attribs = CollectionsUtil.CreateCaseInsensitiveSortedList();
                    string directiveName = this.ProcessAttributes(match, attribs);
                    this.ProcessDirective(directiveName, attribs);
                    startat = match.Index + match.Length;
                }
                else if ((match = BaseParser.includeRegex.Match(text, startat)).Success)
                {
                    this.ProcessServerInclude(match);
                    startat = match.Index + match.Length;
                }
                else if ((match = BaseParser.commentRegex.Match(text, startat)).Success)
                {
                    startat = match.Index + match.Length;
                }
                else
                {
                    int num2 = text.IndexOf("<%@", startat, StringComparison.Ordinal);
                    if ((num2 == -1) || (num2 == startat))
                    {
                        return;
                    }
                    startat = num2;
                }
                if (startat != text.Length)
                {
                    goto Label_0002;
                }
            }
        }

        protected virtual void PrepareParse()
        {
        }

        private string ProcessAttributes(Match match, IDictionary attribs)
        {
            string str = null;
            CaptureCollection captures = match.Groups["attrname"].Captures;
            CaptureCollection captures2 = match.Groups["attrval"].Captures;
            CaptureCollection captures3 = match.Groups["equal"].Captures;
            for (int i = 0; i < captures.Count; i++)
            {
                string key = captures[i].ToString();
                string str3 = captures2[i].ToString();
                bool flag = captures3[i].ToString().Length > 0;
                if (((key != null) && !flag) && (str == null))
                {
                    str = key;
                }
                else
                {
                    try
                    {
                        if (attribs != null)
                        {
                            attribs.Add(key, str3);
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            return str;
        }

        internal virtual void ProcessDirective(string directiveName, IDictionary directive)
        {
            if ((directiveName == null) || StringUtil.EqualsIgnoreCase(directiveName, this.DefaultDirectiveName))
            {
                this.ProcessMainDirective(directive);
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "register"))
            {
                VirtualPath andRemoveVirtualPathAttribute = Util.GetAndRemoveVirtualPathAttribute(directive, "src");
                if (andRemoveVirtualPathAttribute != null)
                {
                    this.AddDependency(andRemoveVirtualPathAttribute);
                }
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "reference"))
            {
                VirtualPath virtualPath = Util.GetAndRemoveVirtualPathAttribute(directive, "virtualpath");
                if (virtualPath != null)
                {
                    this.AddDependency(virtualPath);
                }
                VirtualPath path3 = Util.GetAndRemoveVirtualPathAttribute(directive, "page");
                if (path3 != null)
                {
                    this.AddDependency(path3);
                }
                VirtualPath path4 = Util.GetAndRemoveVirtualPathAttribute(directive, "control");
                if (path4 != null)
                {
                    this.AddDependency(path4);
                }
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "assembly"))
            {
                VirtualPath path5 = Util.GetAndRemoveVirtualPathAttribute(directive, "src");
                if (path5 != null)
                {
                    this.AddDependency(path5);
                }
            }
        }

        private void ProcessMainDirective(IDictionary mainDirective)
        {
            foreach (DictionaryEntry entry in mainDirective)
            {
                string str2;
                string deviceName = Util.ParsePropertyDeviceFilter(((string) entry.Key).ToLower(CultureInfo.InvariantCulture), out str2);
                this.ProcessMainDirectiveAttribute(deviceName, str2, (string) entry.Value);
            }
        }

        internal virtual void ProcessMainDirectiveAttribute(string deviceName, string name, string value)
        {
            if (name == "src")
            {
                string nonEmptyAttribute = Util.GetNonEmptyAttribute(name, value);
                this.AddDependency(VirtualPath.Create(nonEmptyAttribute));
            }
        }

        private void ProcessServerInclude(Match match)
        {
            string str = match.Groups["pathtype"].Value;
            string str2 = match.Groups["filename"].Value;
            if (str2.Length != 0)
            {
                VirtualPath currentVirtualPath;
                string physicalPath = null;
                if (StringUtil.EqualsIgnoreCase(str, "file"))
                {
                    if (UrlPath.IsAbsolutePhysicalPath(str2))
                    {
                        physicalPath = str2;
                        currentVirtualPath = base.CurrentVirtualPath;
                    }
                    else
                    {
                        currentVirtualPath = base.ResolveVirtualPath(VirtualPath.Create(str2));
                    }
                }
                else if (StringUtil.EqualsIgnoreCase(str, "virtual"))
                {
                    currentVirtualPath = base.ResolveVirtualPath(VirtualPath.Create(str2));
                }
                else
                {
                    return;
                }
                VirtualPath path2 = this._virtualPath;
                try
                {
                    this._virtualPath = currentVirtualPath;
                    this.ParseFile(physicalPath, currentVirtualPath);
                }
                finally
                {
                    this._virtualPath = path2;
                }
            }
        }

        internal abstract string DefaultDirectiveName { get; }

        protected PagesSection PagesConfig
        {
            get
            {
                return this._pagesConfig;
            }
        }
    }
}

