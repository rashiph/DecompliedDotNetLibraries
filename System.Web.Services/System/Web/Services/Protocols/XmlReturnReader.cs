namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;
    using System.Xml.Serialization;

    public class XmlReturnReader : MimeReturnReader
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

        public override object Read(WebResponse response, Stream responseStream)
        {
            object obj3;
            try
            {
                if (response == null)
                {
                    throw new ArgumentNullException("response");
                }
                if (!ContentType.MatchesBase(response.ContentType, "text/xml"))
                {
                    throw new InvalidOperationException(Res.GetString("WebResultNotXml"));
                }
                Encoding encoding = RequestResponseUtils.GetEncoding(response.ContentType);
                StreamReader textReader = new StreamReader(responseStream, encoding, true);
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "Read", new object[0]) : null;
                if (Tracing.On)
                {
                    Tracing.Enter(Tracing.TraceId("TraceReadResponse"), caller, new TraceMethod(this.xmlSerializer, "Deserialize", new object[] { textReader }));
                }
                object obj2 = this.xmlSerializer.Deserialize(textReader);
                if (Tracing.On)
                {
                    Tracing.Exit(Tracing.TraceId("TraceReadResponse"), caller);
                }
                obj3 = obj2;
            }
            finally
            {
                response.Close();
            }
            return obj3;
        }
    }
}

