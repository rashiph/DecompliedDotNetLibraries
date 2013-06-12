namespace System.Diagnostics
{
    using System;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class TextWriterTraceListener : TraceListener
    {
        private string fileName;
        internal TextWriter writer;

        public TextWriterTraceListener()
        {
        }

        public TextWriterTraceListener(Stream stream) : this(stream, string.Empty)
        {
        }

        public TextWriterTraceListener(TextWriter writer) : this(writer, string.Empty)
        {
        }

        public TextWriterTraceListener(string fileName)
        {
            this.fileName = fileName;
        }

        public TextWriterTraceListener(Stream stream, string name) : base(name)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.writer = new StreamWriter(stream);
        }

        public TextWriterTraceListener(TextWriter writer, string name) : base(name)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            this.writer = writer;
        }

        public TextWriterTraceListener(string fileName, string name) : base(name)
        {
            this.fileName = fileName;
        }

        public override void Close()
        {
            if (this.writer != null)
            {
                try
                {
                    this.writer.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }
            this.writer = null;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.Close();
                }
                else
                {
                    if (this.writer != null)
                    {
                        try
                        {
                            this.writer.Close();
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                    this.writer = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal bool EnsureWriter()
        {
            bool flag = true;
            if (this.writer == null)
            {
                flag = false;
                if (this.fileName == null)
                {
                    return flag;
                }
                Encoding encodingWithFallback = GetEncodingWithFallback(new UTF8Encoding(false));
                string fullPath = Path.GetFullPath(this.fileName);
                string directoryName = Path.GetDirectoryName(fullPath);
                string fileName = Path.GetFileName(fullPath);
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        this.writer = new StreamWriter(fullPath, true, encodingWithFallback, 0x1000);
                        flag = true;
                        break;
                    }
                    catch (IOException)
                    {
                        fileName = Guid.NewGuid().ToString() + fileName;
                        fullPath = Path.Combine(directoryName, fileName);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                if (!flag)
                {
                    this.fileName = null;
                }
            }
            return flag;
        }

        public override void Flush()
        {
            if (this.EnsureWriter())
            {
                try
                {
                    this.writer.Flush();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        private static Encoding GetEncodingWithFallback(Encoding encoding)
        {
            Encoding encoding2 = (Encoding) encoding.Clone();
            encoding2.EncoderFallback = EncoderFallback.ReplacementFallback;
            encoding2.DecoderFallback = DecoderFallback.ReplacementFallback;
            return encoding2;
        }

        public override void Write(string message)
        {
            if (this.EnsureWriter())
            {
                if (base.NeedIndent)
                {
                    this.WriteIndent();
                }
                try
                {
                    this.writer.Write(message);
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        public override void WriteLine(string message)
        {
            if (this.EnsureWriter())
            {
                if (base.NeedIndent)
                {
                    this.WriteIndent();
                }
                try
                {
                    this.writer.WriteLine(message);
                    base.NeedIndent = true;
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        public TextWriter Writer
        {
            get
            {
                this.EnsureWriter();
                return this.writer;
            }
            set
            {
                this.writer = value;
            }
        }
    }
}

