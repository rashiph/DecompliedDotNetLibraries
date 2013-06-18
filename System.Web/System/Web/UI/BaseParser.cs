namespace System.Web.UI
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Web.RegularExpressions;

    public class BaseParser
    {
        private VirtualPath _baseVirtualDir;
        private VirtualPath _currentVirtualPath;
        private Regex _tagRegex;
        internal static readonly Regex aspCodeRegex = new AspCodeRegex();
        internal static readonly Regex aspEncodedExprRegex = new AspEncodedExprRegex();
        internal static readonly Regex aspExprRegex = new AspExprRegex();
        internal static readonly Regex commentRegex = new CommentRegex();
        internal static readonly Regex databindExprRegex = new DatabindExprRegex();
        internal static readonly Regex directiveRegex = new DirectiveRegex();
        internal static readonly Regex endtagRegex = new EndTagRegex();
        internal static readonly Regex gtRegex = new GTRegex();
        internal static readonly Regex includeRegex = new IncludeRegex();
        internal static readonly Regex ltRegex = new LTRegex();
        internal static readonly Regex runatServerRegex = new RunatServerRegex();
        internal static readonly Regex serverTagsRegex = new ServerTagsRegex();
        private static readonly Regex tagRegex35 = new TagRegex35();
        private static readonly Regex tagRegex40 = new System.Web.RegularExpressions.TagRegex();
        internal static readonly Regex textRegex = new TextRegex();

        private bool IsVersion40OrAbove()
        {
            if (HostingEnvironment.IsHosted)
            {
                return MultiTargetingUtil.IsTargetFramework40OrAbove;
            }
            return TargetFrameworkUtil.IsSupportedType(typeof(TagRegex35));
        }

        internal VirtualPath ResolveVirtualPath(VirtualPath virtualPath)
        {
            return VirtualPathProvider.CombineVirtualPathsInternal(this.CurrentVirtualPath, virtualPath);
        }

        internal VirtualPath BaseVirtualDir
        {
            get
            {
                return this._baseVirtualDir;
            }
        }

        internal VirtualPath CurrentVirtualPath
        {
            get
            {
                return this._currentVirtualPath;
            }
            set
            {
                this._currentVirtualPath = value;
                if (value != null)
                {
                    this._baseVirtualDir = value.Parent;
                }
            }
        }

        internal string CurrentVirtualPathString
        {
            get
            {
                return VirtualPath.GetVirtualPathString(this.CurrentVirtualPath);
            }
        }

        internal Regex TagRegex
        {
            get
            {
                if (this._tagRegex == null)
                {
                    this._tagRegex = this.IsVersion40OrAbove() ? tagRegex40 : tagRegex35;
                }
                return this._tagRegex;
            }
        }
    }
}

