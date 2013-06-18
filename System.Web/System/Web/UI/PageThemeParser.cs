namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Util;

    internal class PageThemeParser : BaseTemplateParser
    {
        private IList _cssFileList;
        private ControlBuilder _currentSkinBuilder;
        private bool _mainDirectiveProcessed;
        private IList _skinFileList;
        private VirtualPath _virtualDirPath;
        internal const string defaultDirectiveName = "skin";

        internal PageThemeParser(VirtualPath virtualDirPath, IList skinFileList, IList cssFileList)
        {
            this._virtualDirPath = virtualDirPath;
            this._skinFileList = skinFileList;
            this._cssFileList = cssFileList;
        }

        internal override RootBuilder CreateDefaultFileLevelBuilder()
        {
            return new FileLevelPageThemeBuilder();
        }

        internal override void ParseInternal()
        {
            if (this._skinFileList != null)
            {
                foreach (string str in this._skinFileList)
                {
                    base.ParseFile(null, str);
                }
            }
            base.AddSourceDependency(this._virtualDirPath);
        }

        internal override void ProcessDirective(string directiveName, IDictionary directive)
        {
            if (((directiveName == null) || (directiveName.Length == 0)) || StringUtil.EqualsIgnoreCase(directiveName, this.DefaultDirectiveName))
            {
                if (this._mainDirectiveProcessed)
                {
                    base.ProcessError(System.Web.SR.GetString("Only_one_directive_allowed", new object[] { this.DefaultDirectiveName }));
                }
                else
                {
                    this.ProcessMainDirective(directive);
                    this._mainDirectiveProcessed = true;
                }
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "register"))
            {
                base.ProcessDirective(directiveName, directive);
            }
            else
            {
                base.ProcessError(System.Web.SR.GetString("Unknown_directive", new object[] { directiveName }));
            }
        }

        internal override bool ProcessMainDirectiveAttribute(string deviceName, string name, string value, IDictionary parseData)
        {
            string str;
            if (((str = name) == null) || ((!(str == "classname") && !(str == "compilationmode")) && !(str == "inherits")))
            {
                return base.ProcessMainDirectiveAttribute(deviceName, name, value, parseData);
            }
            base.ProcessError(System.Web.SR.GetString("Attr_not_supported_in_directive", new object[] { name, this.DefaultDirectiveName }));
            return false;
        }

        internal ICollection CssFileList
        {
            get
            {
                return this._cssFileList;
            }
        }

        internal ControlBuilder CurrentSkinBuilder
        {
            get
            {
                return this._currentSkinBuilder;
            }
            set
            {
                this._currentSkinBuilder = value;
            }
        }

        internal override Type DefaultBaseType
        {
            get
            {
                return typeof(PageTheme);
            }
        }

        internal override string DefaultDirectiveName
        {
            get
            {
                return "skin";
            }
        }

        internal override bool IsCodeAllowed
        {
            get
            {
                return false;
            }
        }

        internal VirtualPath VirtualDirPath
        {
            get
            {
                return this._virtualDirPath;
            }
        }
    }
}

