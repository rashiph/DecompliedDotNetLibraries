namespace System.Web.UI.Design
{
    using System;
    using System.IO;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;

    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    internal class DesignTimeHtmlTextWriter : HtmlTextWriter
    {
        public DesignTimeHtmlTextWriter(TextWriter writer) : base(writer)
        {
        }

        public DesignTimeHtmlTextWriter(TextWriter writer, string tabString) : base(writer, tabString)
        {
        }

        public override void AddAttribute(HtmlTextWriterAttribute key, string value)
        {
            if (((key == HtmlTextWriterAttribute.Src) || (key == HtmlTextWriterAttribute.Href)) || (key == HtmlTextWriterAttribute.Background))
            {
                base.AddAttribute(key.ToString(), value, key);
            }
            else
            {
                base.AddAttribute(key, value);
            }
        }
    }
}

