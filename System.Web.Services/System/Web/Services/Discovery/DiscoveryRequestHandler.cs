namespace System.Web.Services.Discovery
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml;

    public sealed class DiscoveryRequestHandler : IHttpHandler
    {
        private static string GetDirPartOfPath(string str)
        {
            int length = str.LastIndexOf('/');
            if (length <= 0)
            {
                return "";
            }
            return str.Substring(0, length);
        }

        private static string GetFilePartOfPath(string str)
        {
            int num = str.LastIndexOf('/');
            if (num < 0)
            {
                return str;
            }
            if (num == (str.Length - 1))
            {
                return "";
            }
            return str.Substring(num + 1);
        }

        public void ProcessRequest(HttpContext context)
        {
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "ProcessRequest", new object[0]) : null;
            if (Tracing.On)
            {
                Tracing.Enter("IHttpHandler.ProcessRequest", caller, Tracing.Details(context.Request));
            }
            new PermissionSet(PermissionState.Unrestricted).Demand();
            string physicalPath = context.Request.PhysicalPath;
            bool traceVerbose = System.ComponentModel.CompModSwitches.DynamicDiscoverySearcher.TraceVerbose;
            if (File.Exists(physicalPath))
            {
                DynamicDiscoveryDocument document = null;
                FileStream input = null;
                try
                {
                    input = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
                    XmlTextReader reader = new XmlTextReader(input) {
                        WhitespaceHandling = WhitespaceHandling.Significant,
                        XmlResolver = null,
                        DtdProcessing = DtdProcessing.Prohibit
                    };
                    if (reader.IsStartElement("dynamicDiscovery", "urn:schemas-dynamicdiscovery:disco.2000-03-17"))
                    {
                        input.Position = 0L;
                        document = DynamicDiscoveryDocument.Load(input);
                    }
                }
                finally
                {
                    if (input != null)
                    {
                        input.Close();
                    }
                }
                if (document != null)
                {
                    DynamicDiscoSearcher searcher;
                    string[] excludedUrls = new string[document.ExcludePaths.Length];
                    string directoryName = Path.GetDirectoryName(physicalPath);
                    string fileName = Path.GetFileName(physicalPath);
                    for (int i = 0; i < excludedUrls.Length; i++)
                    {
                        excludedUrls[i] = document.ExcludePaths[i].Path;
                    }
                    Uri url = context.Request.Url;
                    string str = Uri.EscapeUriString(url.ToString()).Replace("#", "%23");
                    string dirPartOfPath = GetDirPartOfPath(str);
                    if ((GetDirPartOfPath(url.LocalPath).Length == 0) || System.ComponentModel.CompModSwitches.DynamicDiscoveryVirtualSearch.Enabled)
                    {
                        fileName = GetFilePartOfPath(str);
                        searcher = new DynamicVirtualDiscoSearcher(directoryName, excludedUrls, dirPartOfPath);
                    }
                    else
                    {
                        searcher = new DynamicPhysicalDiscoSearcher(directoryName, excludedUrls, dirPartOfPath);
                    }
                    bool flag2 = System.ComponentModel.CompModSwitches.DynamicDiscoverySearcher.TraceVerbose;
                    searcher.Search(fileName);
                    DiscoveryDocument discoveryDocument = searcher.DiscoveryDocument;
                    MemoryStream stream = new MemoryStream(0x400);
                    StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false));
                    discoveryDocument.Write(writer);
                    stream.Position = 0L;
                    byte[] buffer = new byte[(int) stream.Length];
                    int count = stream.Read(buffer, 0, buffer.Length);
                    context.Response.ContentType = ContentType.Compose("text/xml", Encoding.UTF8);
                    context.Response.OutputStream.Write(buffer, 0, count);
                }
                else
                {
                    context.Response.ContentType = "text/xml";
                    context.Response.WriteFile(physicalPath);
                }
                if (Tracing.On)
                {
                    Tracing.Exit("IHttpHandler.ProcessRequest", caller);
                }
            }
            else
            {
                if (Tracing.On)
                {
                    Tracing.Exit("IHttpHandler.ProcessRequest", caller);
                }
                throw new HttpException(0x194, System.Web.Services.Res.GetString("WebPathNotFound", new object[] { context.Request.Path }));
            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}

