namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Text;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;
    using System.Xml.Serialization;

    public abstract class DiscoveryReference
    {
        private DiscoveryClientProtocol clientProtocol;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected DiscoveryReference()
        {
        }

        internal Exception AttemptResolve(string contentType, Stream stream)
        {
            try
            {
                this.Resolve(contentType, stream);
                return null;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, this, "AttemptResolve", exception);
                }
                return exception;
            }
        }

        public static string FilenameFromUrl(string url)
        {
            int num = url.LastIndexOf('/', url.Length - 1);
            if (num >= 0)
            {
                url = url.Substring(num + 1);
            }
            int index = url.IndexOf('.');
            if (index >= 0)
            {
                url = url.Substring(0, index);
            }
            int length = url.IndexOf('?');
            if (length >= 0)
            {
                url = url.Substring(0, length);
            }
            if ((url != null) && (url.Length != 0))
            {
                return MakeValidFilename(url);
            }
            return "item";
        }

        private static bool FindChar(char ch, char[] chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (ch == chars[i])
                {
                    return true;
                }
            }
            return false;
        }

        internal virtual void LoadExternals(Hashtable loadedExternals)
        {
        }

        internal static string MakeValidFilename(string filename)
        {
            if (filename == null)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(filename.Length);
            for (int i = 0; i < filename.Length; i++)
            {
                char ch = filename[i];
                if (!FindChar(ch, Path.InvalidPathChars))
                {
                    builder.Append(ch);
                }
            }
            string path = builder.ToString();
            if (path.Length == 0)
            {
                path = "item";
            }
            return Path.GetFileName(path);
        }

        public abstract object ReadDocument(Stream stream);
        public void Resolve()
        {
            if (this.ClientProtocol == null)
            {
                throw new InvalidOperationException(Res.GetString("WebResolveMissingClientProtocol"));
            }
            if ((this.ClientProtocol.Documents[this.Url] == null) && (this.ClientProtocol.InlinedSchemas[this.Url] == null))
            {
                string url = this.Url;
                string str2 = this.Url;
                string contentType = null;
                Stream stream = this.ClientProtocol.Download(ref url, ref contentType);
                if (this.ClientProtocol.Documents[url] != null)
                {
                    this.Url = url;
                }
                else
                {
                    try
                    {
                        this.Url = url;
                        this.Resolve(contentType, stream);
                    }
                    catch
                    {
                        this.Url = str2;
                        throw;
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
            }
        }

        protected internal abstract void Resolve(string contentType, Stream stream);
        internal static string UriToString(string baseUrl, string relUrl)
        {
            return new Uri(new Uri(baseUrl), relUrl).GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
        }

        public abstract void WriteDocument(object document, Stream stream);

        [XmlIgnore]
        public DiscoveryClientProtocol ClientProtocol
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.clientProtocol;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.clientProtocol = value;
            }
        }

        [XmlIgnore]
        public virtual string DefaultFilename
        {
            get
            {
                return FilenameFromUrl(this.Url);
            }
        }

        [XmlIgnore]
        public abstract string Url { get; set; }
    }
}

