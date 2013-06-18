namespace System.Web.UI
{
    using System;
    using System.Web;

    internal class FileLevelPageThemeBuilder : RootBuilder
    {
        public override void AppendLiteralString(string s)
        {
            if ((s != null) && !Util.IsWhiteSpaceString(s))
            {
                throw new HttpException(System.Web.SR.GetString("Literal_content_not_allowed", new object[] { System.Web.SR.GetString("Page_theme_skin_file"), s.Trim() }));
            }
            base.AppendLiteralString(s);
        }

        public override void AppendSubBuilder(ControlBuilder subBuilder)
        {
            Type controlType = subBuilder.ControlType;
            if (!typeof(Control).IsAssignableFrom(controlType))
            {
                throw new HttpException(System.Web.SR.GetString("Page_theme_only_controls_allowed", new object[] { (controlType == null) ? string.Empty : controlType.ToString() }));
            }
            if (base.InPageTheme && !ThemeableAttribute.IsTypeThemeable(subBuilder.ControlType))
            {
                throw new HttpParseException(System.Web.SR.GetString("Type_theme_disabled", new object[] { subBuilder.ControlType.FullName }), null, subBuilder.VirtualPath, null, subBuilder.Line);
            }
            base.AppendSubBuilder(subBuilder);
        }
    }
}

