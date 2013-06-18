namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Services.Diagnostics;
    using System.Xml.Serialization;

    internal class XmlReturnWriter : MimeReturnWriter
    {
        private XmlSerializer xmlSerializer;

        public override object GetInitializer(LogicalMethodInfo methodInfo)
        {
            return XmlReturn.GetInitializer(methodInfo);
        }

        public override object[] GetInitializers(LogicalMethodInfo[] methodInfos)
        {
            return XmlReturn.GetInitializers(methodInfos);
        }

        public override void Initialize(object o)
        {
            this.xmlSerializer = (XmlSerializer) o;
        }

        internal override void Write(HttpResponse response, Stream outputStream, object returnValue)
        {
            Encoding encoding = new UTF8Encoding(false);
            response.ContentType = ContentType.Compose("text/xml", encoding);
            StreamWriter writer = new StreamWriter(outputStream, encoding);
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "Write", new object[0]) : null;
            if (Tracing.On)
            {
                Tracing.Enter(Tracing.TraceId("TraceWriteResponse"), caller, new TraceMethod(this.xmlSerializer, "Serialize", new object[] { writer, returnValue }));
            }
            this.xmlSerializer.Serialize((TextWriter) writer, returnValue);
            if (Tracing.On)
            {
                Tracing.Exit(Tracing.TraceId("TraceWriteResponse"), caller);
            }
        }
    }
}

