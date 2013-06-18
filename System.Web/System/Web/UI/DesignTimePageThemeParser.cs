namespace System.Web.UI
{
    using System;
    using System.Text;

    internal class DesignTimePageThemeParser : PageThemeParser
    {
        private string _themePhysicalPath;

        internal DesignTimePageThemeParser(string virtualDirPath) : base(null, null, null)
        {
            this._themePhysicalPath = virtualDirPath;
        }

        internal override void ParseInternal()
        {
            if (base.Text != null)
            {
                base.ParseString(base.Text, base.CurrentVirtualPath, Encoding.UTF8);
            }
        }

        internal string ThemePhysicalPath
        {
            get
            {
                return this._themePhysicalPath;
            }
        }
    }
}

