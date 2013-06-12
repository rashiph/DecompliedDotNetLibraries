namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.UI;

    internal class PageThemeBuildProvider : BaseTemplateBuildProvider
    {
        private ArrayList _cssFileList;
        private IList _skinFileList;
        private VirtualPath _virtualDirPath;

        internal PageThemeBuildProvider(VirtualPath virtualDirPath)
        {
            this._virtualDirPath = virtualDirPath;
            base.SetVirtualPath(virtualDirPath);
        }

        internal void AddCssFile(VirtualPath virtualPath)
        {
            if (this._cssFileList == null)
            {
                this._cssFileList = new ArrayList();
            }
            this._cssFileList.Add(virtualPath.AppRelativeVirtualPathString);
        }

        internal void AddSkinFile(VirtualPath virtualPath)
        {
            if (this._skinFileList == null)
            {
                this._skinFileList = new StringCollection();
            }
            this._skinFileList.Add(virtualPath.VirtualPathString);
        }

        internal override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser)
        {
            return new PageThemeCodeDomTreeGenerator((PageThemeParser) parser);
        }

        protected override TemplateParser CreateParser()
        {
            if (this._cssFileList != null)
            {
                this._cssFileList.Sort();
            }
            return new PageThemeParser(this._virtualDirPath, this._skinFileList, this._cssFileList);
        }

        internal virtual string AssemblyNamePrefix
        {
            get
            {
                return "App_Theme_";
            }
        }
    }
}

