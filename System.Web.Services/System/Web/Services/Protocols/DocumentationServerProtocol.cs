namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Diagnostics;
    using System.Web.UI;

    internal sealed class DocumentationServerProtocol : ServerProtocol
    {
        private IHttpHandler handler;
        private const int MAX_PATH_SIZE = 0x400;
        private DocumentationServerType serverType;

        internal void Documentation()
        {
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true), SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        private IHttpHandler GetCompiledPageInstance(string virtualPath, string inputFile, HttpContext context)
        {
            return PageParser.GetCompiledPageInstance(virtualPath, inputFile, context);
        }

        internal override bool Initialize()
        {
            this.serverType = (DocumentationServerType) base.GetFromCache(typeof(DocumentationServerProtocol), base.Type);
            if (this.serverType == null)
            {
                lock (ServerProtocol.InternalSyncObject)
                {
                    this.serverType = (DocumentationServerType) base.GetFromCache(typeof(DocumentationServerProtocol), base.Type);
                    if (this.serverType == null)
                    {
                        string uri = Uri.EscapeUriString(base.Request.Url.ToString()).Replace("#", "%23");
                        this.serverType = new DocumentationServerType(base.Type, uri);
                        base.AddToCache(typeof(DocumentationServerProtocol), base.Type, this.serverType);
                    }
                }
            }
            WebServicesSection current = WebServicesSection.Current;
            if ((current.WsdlHelpGenerator.Href != null) && (current.WsdlHelpGenerator.Href.Length > 0))
            {
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "Initialize", new object[0]) : null;
                if (Tracing.On)
                {
                    Tracing.Enter("ASP.NET", caller, new TraceMethod(typeof(PageParser), "GetCompiledPageInstance", new object[] { current.WsdlHelpGenerator.HelpGeneratorVirtualPath, current.WsdlHelpGenerator.HelpGeneratorPath, base.Context }));
                }
                this.handler = this.GetCompiledPageInstance(current.WsdlHelpGenerator.HelpGeneratorVirtualPath, current.WsdlHelpGenerator.HelpGeneratorPath, base.Context);
                if (Tracing.On)
                {
                    Tracing.Exit("ASP.NET", caller);
                }
            }
            return true;
        }

        internal override object[] ReadParameters()
        {
            return new object[0];
        }

        internal override bool WriteException(Exception e, Stream outputStream)
        {
            return false;
        }

        internal override void WriteReturns(object[] returnValues, Stream outputStream)
        {
            try
            {
                if (this.handler != null)
                {
                    base.Context.Items.Add("wsdls", this.serverType.ServiceDescriptions);
                    base.Context.Items.Add("schemas", this.serverType.Schemas);
                    if (base.Context.Request.Url.IsLoopback || base.Context.Request.IsLocal)
                    {
                        base.Context.Items.Add("wsdlsWithPost", this.serverType.ServiceDescriptionsWithPost);
                        base.Context.Items.Add("schemasWithPost", this.serverType.SchemasWithPost);
                    }
                    base.Context.Items.Add("conformanceWarnings", WebServicesSection.Current.EnabledConformanceWarnings);
                    base.Response.ContentType = "text/html";
                    this.handler.ProcessRequest(base.Context);
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new InvalidOperationException(Res.GetString("HelpGeneratorInternalError"), exception);
            }
        }

        internal override bool IsOneWay
        {
            get
            {
                return false;
            }
        }

        internal override LogicalMethodInfo MethodInfo
        {
            get
            {
                return this.serverType.MethodInfo;
            }
        }

        internal override System.Web.Services.Protocols.ServerType ServerType
        {
            get
            {
                return this.serverType;
            }
        }
    }
}

