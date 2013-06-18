namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Description;
    using System.Xml.Schema;

    internal sealed class DiscoveryServerProtocol : ServerProtocol
    {
        private DiscoveryServerType serverType;

        internal void Discover()
        {
        }

        internal override bool Initialize()
        {
            this.serverType = (DiscoveryServerType) base.GetFromCache(typeof(DiscoveryServerProtocol), base.Type);
            if (this.serverType == null)
            {
                lock (ServerProtocol.InternalSyncObject)
                {
                    this.serverType = (DiscoveryServerType) base.GetFromCache(typeof(DiscoveryServerProtocol), base.Type);
                    if (this.serverType == null)
                    {
                        string uri = Uri.EscapeUriString(base.Request.Url.ToString()).Replace("#", "%23");
                        this.serverType = new DiscoveryServerType(base.Type, uri);
                        base.AddToCache(typeof(DiscoveryServerProtocol), base.Type, this.serverType);
                    }
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
            base.Response.Clear();
            base.Response.ClearHeaders();
            base.Response.ContentType = ContentType.Compose("text/plain", Encoding.UTF8);
            base.Response.StatusCode = 500;
            base.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription(base.Response.StatusCode);
            StreamWriter writer = new StreamWriter(outputStream, new UTF8Encoding(false));
            writer.WriteLine(base.GenerateFaultString(e, true));
            writer.Flush();
            return true;
        }

        internal override void WriteReturns(object[] returnValues, Stream outputStream)
        {
            string id = base.Request.QueryString["schema"];
            Encoding encoding = new UTF8Encoding(false);
            if (id != null)
            {
                XmlSchema schema = this.serverType.GetSchema(id);
                if (schema == null)
                {
                    throw new InvalidOperationException(Res.GetString("WebSchemaNotFound"));
                }
                base.Response.ContentType = ContentType.Compose("text/xml", encoding);
                schema.Write(new StreamWriter(outputStream, encoding));
            }
            else
            {
                id = base.Request.QueryString["wsdl"];
                if (id != null)
                {
                    ServiceDescription serviceDescription = this.serverType.GetServiceDescription(id);
                    if (serviceDescription == null)
                    {
                        throw new InvalidOperationException(Res.GetString("ServiceDescriptionWasNotFound0"));
                    }
                    base.Response.ContentType = ContentType.Compose("text/xml", encoding);
                    serviceDescription.Write(new StreamWriter(outputStream, encoding));
                }
                else
                {
                    string strA = base.Request.QueryString[null];
                    if ((strA != null) && (string.Compare(strA, "wsdl", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        base.Response.ContentType = ContentType.Compose("text/xml", encoding);
                        this.serverType.Description.Write(new StreamWriter(outputStream, encoding));
                    }
                    else
                    {
                        if ((strA == null) || (string.Compare(strA, "disco", StringComparison.OrdinalIgnoreCase) != 0))
                        {
                            throw new InvalidOperationException(Res.GetString("internalError0"));
                        }
                        base.Response.ContentType = ContentType.Compose("text/xml", encoding);
                        this.serverType.Disco.Write(new StreamWriter(outputStream, encoding));
                    }
                }
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

