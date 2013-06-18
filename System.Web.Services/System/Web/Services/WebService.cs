namespace System.Web.Services
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Services.Protocols;
    using System.Web.SessionState;

    public class WebService : MarshalByValueComponent
    {
        private HttpContext context;
        internal static readonly string SoapVersionContextSlot = "WebServiceSoapVersion";

        internal void SetContext(HttpContext context)
        {
            this.context = context;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Description("The ASP.NET application object for the current request."), Browsable(false)]
        public HttpApplicationState Application
        {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
            get
            {
                return this.Context.Application;
            }
        }

        [Browsable(false), WebServicesDescription("WebServiceContext"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpContext Context
        {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
            get
            {
                if (this.context == null)
                {
                    this.context = HttpContext.Current;
                }
                if (this.context == null)
                {
                    throw new InvalidOperationException(Res.GetString("WebMissingHelpContext"));
                }
                return this.context;
            }
        }

        [WebServicesDescription("WebServiceServer"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpServerUtility Server
        {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
            get
            {
                return this.Context.Server;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription("WebServiceSession"), Browsable(false)]
        public HttpSessionState Session
        {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
            get
            {
                return this.Context.Session;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription("WebServiceSoapVersion"), ComVisible(false)]
        public SoapProtocolVersion SoapVersion
        {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
            get
            {
                object obj2 = this.Context.Items[SoapVersionContextSlot];
                if ((obj2 != null) && (obj2 is SoapProtocolVersion))
                {
                    return (SoapProtocolVersion) obj2;
                }
                return SoapProtocolVersion.Default;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription("WebServiceUser"), Browsable(false)]
        public IPrincipal User
        {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
            get
            {
                return this.Context.User;
            }
        }
    }
}

