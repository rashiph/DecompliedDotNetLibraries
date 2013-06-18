namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Configuration;

    internal abstract class HttpServerProtocol : ServerProtocol
    {
        private bool hasInputPayload;
        private HttpServerMethod serverMethod;
        private HttpServerType serverType;

        protected HttpServerProtocol(bool hasInputPayload)
        {
            this.hasInputPayload = hasInputPayload;
        }

        internal static bool AreUrlParametersSupported(LogicalMethodInfo methodInfo)
        {
            if (methodInfo.OutParameters.Length > 0)
            {
                return false;
            }
            foreach (ParameterInfo info in methodInfo.InParameters)
            {
                Type parameterType = info.ParameterType;
                if (parameterType.IsArray)
                {
                    if (!ScalarFormatter.IsTypeSupported(parameterType.GetElementType()))
                    {
                        return false;
                    }
                }
                else if (!ScalarFormatter.IsTypeSupported(parameterType))
                {
                    return false;
                }
            }
            return true;
        }

        internal override bool Initialize()
        {
            string name = base.Request.PathInfo.Substring(1);
            this.serverType = (HttpServerType) base.GetFromCache(typeof(HttpServerProtocol), base.Type);
            if (this.serverType == null)
            {
                lock (ServerProtocol.InternalSyncObject)
                {
                    this.serverType = (HttpServerType) base.GetFromCache(typeof(HttpServerProtocol), base.Type);
                    if (this.serverType == null)
                    {
                        this.serverType = new HttpServerType(base.Type);
                        base.AddToCache(typeof(HttpServerProtocol), base.Type, this.serverType);
                    }
                }
            }
            this.serverMethod = this.serverType.GetMethod(name);
            if (this.serverMethod == null)
            {
                this.serverMethod = this.serverType.GetMethodIgnoreCase(name);
                if (this.serverMethod != null)
                {
                    throw new ArgumentException(Res.GetString("WebInvalidMethodNameCase", new object[] { name, this.serverMethod.name }), "methodName");
                }
                string str2 = Encoding.UTF8.GetString(Encoding.Default.GetBytes(name));
                this.serverMethod = this.serverType.GetMethod(str2);
                if (this.serverMethod == null)
                {
                    throw new InvalidOperationException(Res.GetString("WebInvalidMethodName", new object[] { name }));
                }
            }
            return true;
        }

        internal override object[] ReadParameters()
        {
            if (this.serverMethod.readerTypes == null)
            {
                return new object[0];
            }
            for (int i = 0; i < this.serverMethod.readerTypes.Length; i++)
            {
                MimeParameterReader reader;
                if (!this.hasInputPayload)
                {
                    if (!(this.serverMethod.readerTypes[i] != typeof(UrlParameterReader)))
                    {
                        goto Label_0061;
                    }
                    continue;
                }
                if (this.serverMethod.readerTypes[i] == typeof(UrlParameterReader))
                {
                    continue;
                }
            Label_0061:
                reader = (MimeParameterReader) MimeFormatter.CreateInstance(this.serverMethod.readerTypes[i], this.serverMethod.readerInitializers[i]);
                object[] objArray = reader.Read(base.Request);
                if (objArray != null)
                {
                    return objArray;
                }
            }
            if (!this.hasInputPayload)
            {
                throw new InvalidOperationException(Res.GetString("WebInvalidRequestFormat"));
            }
            throw new InvalidOperationException(Res.GetString("WebInvalidRequestFormatDetails", new object[] { base.Request.ContentType }));
        }

        internal override bool WriteException(Exception e, Stream outputStream)
        {
            base.Response.Clear();
            base.Response.ClearHeaders();
            base.Response.ContentType = ContentType.Compose("text/plain", Encoding.UTF8);
            ServerProtocol.SetHttpResponseStatusCode(base.Response, 500);
            base.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription(base.Response.StatusCode);
            StreamWriter writer = new StreamWriter(outputStream, new UTF8Encoding(false));
            if (WebServicesSection.Current.Diagnostics.SuppressReturningExceptions)
            {
                writer.WriteLine(Res.GetString("WebSuppressedExceptionMessage"));
            }
            else
            {
                writer.WriteLine(base.GenerateFaultString(e, true));
            }
            writer.Flush();
            return true;
        }

        internal override void WriteReturns(object[] returnValues, Stream outputStream)
        {
            if (this.serverMethod.writerType != null)
            {
                ((MimeReturnWriter) MimeFormatter.CreateInstance(this.serverMethod.writerType, this.serverMethod.writerInitializer)).Write(base.Response, outputStream, returnValues[0]);
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
                return this.serverMethod.methodInfo;
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

