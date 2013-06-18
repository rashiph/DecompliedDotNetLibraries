namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class MailDefinitionBodyFileNameEditor : UrlEditor
    {
        protected override string Caption
        {
            get
            {
                return System.Design.SR.GetString("MailDefinitionBodyFileNameEditor_DefaultCaption");
            }
        }

        protected override string Filter
        {
            get
            {
                return System.Design.SR.GetString("MailDefinitionBodyFileNameEditor_DefaultFilter");
            }
        }
    }
}

