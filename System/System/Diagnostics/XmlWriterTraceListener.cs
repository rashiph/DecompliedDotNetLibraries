namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class XmlWriterTraceListener : TextWriterTraceListener
    {
        private const string fixedHeader = "<E2ETraceEvent xmlns=\"http://schemas.microsoft.com/2004/06/E2ETraceEvent\"><System xmlns=\"http://schemas.microsoft.com/2004/06/windows/eventlog/system\">";
        private readonly string machineName;
        private StringBuilder strBldr;
        private XmlTextWriter xmlBlobWriter;

        public XmlWriterTraceListener(Stream stream) : base(stream)
        {
            this.machineName = Environment.MachineName;
        }

        public XmlWriterTraceListener(TextWriter writer) : base(writer)
        {
            this.machineName = Environment.MachineName;
        }

        public XmlWriterTraceListener(string filename) : base(filename)
        {
            this.machineName = Environment.MachineName;
        }

        public XmlWriterTraceListener(Stream stream, string name) : base(stream, name)
        {
            this.machineName = Environment.MachineName;
        }

        public XmlWriterTraceListener(TextWriter writer, string name) : base(writer, name)
        {
            this.machineName = Environment.MachineName;
        }

        public XmlWriterTraceListener(string filename, string name) : base(filename, name)
        {
            this.machineName = Environment.MachineName;
        }

        public override void Close()
        {
            base.Close();
            if (this.xmlBlobWriter != null)
            {
                this.xmlBlobWriter.Close();
            }
            this.xmlBlobWriter = null;
            this.strBldr = null;
        }

        public override void Fail(string message, string detailMessage)
        {
            StringBuilder builder = new StringBuilder(message);
            if (detailMessage != null)
            {
                builder.Append(" ");
                builder.Append(detailMessage);
            }
            this.TraceEvent(null, SR.GetString("TraceAsTraceSource"), TraceEventType.Error, 0, builder.ToString());
        }

        private void InternalWrite(string message)
        {
            if (base.EnsureWriter())
            {
                base.writer.Write(message);
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data))
            {
                this.WriteHeader(source, eventType, id, eventCache);
                this.InternalWrite("<TraceData>");
                if (data != null)
                {
                    this.InternalWrite("<DataItem>");
                    this.WriteData(data);
                    this.InternalWrite("</DataItem>");
                }
                this.InternalWrite("</TraceData>");
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                this.WriteHeader(source, eventType, id, eventCache);
                this.InternalWrite("<TraceData>");
                if (data != null)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        this.InternalWrite("<DataItem>");
                        if (data[i] != null)
                        {
                            this.WriteData(data[i]);
                        }
                        this.InternalWrite("</DataItem>");
                    }
                }
                this.InternalWrite("</TraceData>");
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, message))
            {
                this.WriteHeader(source, eventType, id, eventCache);
                this.WriteEscaped(message);
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, format, args))
            {
                string str;
                this.WriteHeader(source, eventType, id, eventCache);
                if (args != null)
                {
                    str = string.Format(CultureInfo.InvariantCulture, format, args);
                }
                else
                {
                    str = format;
                }
                this.WriteEscaped(str);
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            this.WriteHeader(source, TraceEventType.Transfer, id, eventCache, relatedActivityId);
            this.WriteEscaped(message);
            this.WriteFooter(eventCache);
        }

        public override void Write(string message)
        {
            this.WriteLine(message);
        }

        private void WriteData(object data)
        {
            XPathNavigator navigator = data as XPathNavigator;
            if (navigator == null)
            {
                this.WriteEscaped(data.ToString());
            }
            else
            {
                if (this.strBldr == null)
                {
                    this.strBldr = new StringBuilder();
                    this.xmlBlobWriter = new XmlTextWriter(new StringWriter(this.strBldr, CultureInfo.CurrentCulture));
                }
                else
                {
                    this.strBldr.Length = 0;
                }
                try
                {
                    navigator.MoveToRoot();
                    this.xmlBlobWriter.WriteNode(navigator, false);
                    this.InternalWrite(this.strBldr.ToString());
                }
                catch (Exception)
                {
                    this.InternalWrite(data.ToString());
                }
            }
        }

        private void WriteEndHeader(TraceEventCache eventCache)
        {
            this.InternalWrite("\" />");
            this.InternalWrite("<Execution ProcessName=\"");
            this.InternalWrite(TraceEventCache.GetProcessName());
            this.InternalWrite("\" ProcessID=\"");
            this.InternalWrite(((uint) TraceEventCache.GetProcessId()).ToString(CultureInfo.InvariantCulture));
            this.InternalWrite("\" ThreadID=\"");
            if (eventCache != null)
            {
                this.WriteEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                this.WriteEscaped(TraceEventCache.GetThreadId().ToString(CultureInfo.InvariantCulture));
            }
            this.InternalWrite("\" />");
            this.InternalWrite("<Channel/>");
            this.InternalWrite("<Computer>");
            this.InternalWrite(this.machineName);
            this.InternalWrite("</Computer>");
            this.InternalWrite("</System>");
            this.InternalWrite("<ApplicationData>");
        }

        private void WriteEscaped(string str)
        {
            if (str != null)
            {
                int startIndex = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    switch (str[i])
                    {
                        case '\n':
                            this.InternalWrite(str.Substring(startIndex, i - startIndex));
                            this.InternalWrite("&#xA;");
                            startIndex = i + 1;
                            break;

                        case '\r':
                            this.InternalWrite(str.Substring(startIndex, i - startIndex));
                            this.InternalWrite("&#xD;");
                            startIndex = i + 1;
                            break;

                        case '&':
                            this.InternalWrite(str.Substring(startIndex, i - startIndex));
                            this.InternalWrite("&amp;");
                            startIndex = i + 1;
                            break;

                        case '\'':
                            this.InternalWrite(str.Substring(startIndex, i - startIndex));
                            this.InternalWrite("&apos;");
                            startIndex = i + 1;
                            break;

                        case '"':
                            this.InternalWrite(str.Substring(startIndex, i - startIndex));
                            this.InternalWrite("&quot;");
                            startIndex = i + 1;
                            break;

                        case '<':
                            this.InternalWrite(str.Substring(startIndex, i - startIndex));
                            this.InternalWrite("&lt;");
                            startIndex = i + 1;
                            break;

                        case '>':
                            this.InternalWrite(str.Substring(startIndex, i - startIndex));
                            this.InternalWrite("&gt;");
                            startIndex = i + 1;
                            break;
                    }
                }
                this.InternalWrite(str.Substring(startIndex, str.Length - startIndex));
            }
        }

        private void WriteFooter(TraceEventCache eventCache)
        {
            bool flag = base.IsEnabled(TraceOptions.LogicalOperationStack);
            bool flag2 = base.IsEnabled(TraceOptions.Callstack);
            if ((eventCache != null) && (flag || flag2))
            {
                this.InternalWrite("<System.Diagnostics xmlns=\"http://schemas.microsoft.com/2004/08/System.Diagnostics\">");
                if (flag)
                {
                    this.InternalWrite("<LogicalOperationStack>");
                    Stack logicalOperationStack = eventCache.LogicalOperationStack;
                    if (logicalOperationStack != null)
                    {
                        foreach (object obj2 in logicalOperationStack)
                        {
                            this.InternalWrite("<LogicalOperation>");
                            this.WriteEscaped(obj2.ToString());
                            this.InternalWrite("</LogicalOperation>");
                        }
                    }
                    this.InternalWrite("</LogicalOperationStack>");
                }
                this.InternalWrite("<Timestamp>");
                this.InternalWrite(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
                this.InternalWrite("</Timestamp>");
                if (flag2)
                {
                    this.InternalWrite("<Callstack>");
                    this.WriteEscaped(eventCache.Callstack);
                    this.InternalWrite("</Callstack>");
                }
                this.InternalWrite("</System.Diagnostics>");
            }
            this.InternalWrite("</ApplicationData></E2ETraceEvent>");
        }

        private void WriteHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache)
        {
            this.WriteStartHeader(source, eventType, id, eventCache);
            this.WriteEndHeader(eventCache);
        }

        private void WriteHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache, Guid relatedActivityId)
        {
            this.WriteStartHeader(source, eventType, id, eventCache);
            this.InternalWrite("\" RelatedActivityID=\"");
            this.InternalWrite(relatedActivityId.ToString("B"));
            this.WriteEndHeader(eventCache);
        }

        public override void WriteLine(string message)
        {
            this.TraceEvent(null, SR.GetString("TraceAsTraceSource"), TraceEventType.Information, 0, message);
        }

        private void WriteStartHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache)
        {
            this.InternalWrite("<E2ETraceEvent xmlns=\"http://schemas.microsoft.com/2004/06/E2ETraceEvent\"><System xmlns=\"http://schemas.microsoft.com/2004/06/windows/eventlog/system\">");
            this.InternalWrite("<EventID>");
            this.InternalWrite(((uint) id).ToString(CultureInfo.InvariantCulture));
            this.InternalWrite("</EventID>");
            this.InternalWrite("<Type>3</Type>");
            this.InternalWrite("<SubType Name=\"");
            this.InternalWrite(eventType.ToString());
            this.InternalWrite("\">0</SubType>");
            this.InternalWrite("<Level>");
            int num = (int) eventType;
            if (num > 0xff)
            {
                num = 0xff;
            }
            if (num < 0)
            {
                num = 0;
            }
            this.InternalWrite(num.ToString(CultureInfo.InvariantCulture));
            this.InternalWrite("</Level>");
            this.InternalWrite("<TimeCreated SystemTime=\"");
            if (eventCache != null)
            {
                this.InternalWrite(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
            }
            else
            {
                this.InternalWrite(DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
            }
            this.InternalWrite("\" />");
            this.InternalWrite("<Source Name=\"");
            this.WriteEscaped(source);
            this.InternalWrite("\" />");
            this.InternalWrite("<Correlation ActivityID=\"");
            if (eventCache != null)
            {
                this.InternalWrite(eventCache.ActivityId.ToString("B"));
            }
            else
            {
                this.InternalWrite(Guid.Empty.ToString("B"));
            }
        }
    }
}

