namespace System.IO
{
    using System;

    internal sealed class MdaHelper
    {
        private string allocatedCallstack;
        private StreamWriter streamWriter;

        internal MdaHelper(StreamWriter sw, string cs)
        {
            this.streamWriter = sw;
            this.allocatedCallstack = cs;
        }

        ~MdaHelper()
        {
            if (((this.streamWriter.charPos != 0) && (this.streamWriter.stream != null)) && (this.streamWriter.stream != Stream.Null))
            {
                string str = (this.streamWriter.stream is FileStream) ? ((FileStream) this.streamWriter.stream).NameInternal : "<unknown>";
                string allocatedCallstack = this.allocatedCallstack;
                if (allocatedCallstack == null)
                {
                    allocatedCallstack = Environment.GetResourceString("IO_StreamWriterBufferedDataLostCaptureAllocatedFromCallstackNotEnabled");
                }
                Mda.StreamWriterBufferedDataLost.ReportError(Environment.GetResourceString("IO_StreamWriterBufferedDataLost", new object[] { this.streamWriter.stream.GetType().FullName, str, allocatedCallstack }));
            }
        }
    }
}

