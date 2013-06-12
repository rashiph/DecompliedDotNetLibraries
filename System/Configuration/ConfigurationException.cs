namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    [Serializable]
    public class ConfigurationException : SystemException
    {
        private string _filename;
        private int _line;
        private const string HTTP_PREFIX = "http:";

        [Obsolete("This class is obsolete, to create a new exception create a System.Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException() : this(null, null, null, 0)
        {
        }

        [Obsolete("This class is obsolete, to create a new exception create a System.Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message) : this(message, null, null, 0)
        {
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Init(info.GetString("filename"), info.GetInt32("line"));
        }

        [Obsolete("This class is obsolete, to create a new exception create a System.Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, Exception inner) : this(message, inner, null, 0)
        {
        }

        [Obsolete("This class is obsolete, to create a new exception create a System.Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, XmlNode node) : this(message, null, GetUnsafeXmlNodeFilename(node), GetXmlNodeLineNumber(node))
        {
        }

        [Obsolete("This class is obsolete, to create a new exception create a System.Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, Exception inner, XmlNode node) : this(message, inner, GetUnsafeXmlNodeFilename(node), GetXmlNodeLineNumber(node))
        {
        }

        [Obsolete("This class is obsolete, to create a new exception create a System.Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, string filename, int line) : this(message, null, filename, line)
        {
        }

        [Obsolete("This class is obsolete, to create a new exception create a System.Configuration!System.Configuration.ConfigurationErrorsException")]
        public ConfigurationException(string message, Exception inner, string filename, int line) : base(message, inner)
        {
            this.Init(filename, line);
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        private static string FullPathWithAssert(string filename)
        {
            string fullPath = null;
            try
            {
                fullPath = Path.GetFullPath(filename);
            }
            catch
            {
            }
            return fullPath;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("filename", this._filename);
            info.AddValue("line", this._line);
        }

        private static string GetUnsafeXmlNodeFilename(XmlNode node)
        {
            IConfigErrorInfo info = node as IConfigErrorInfo;
            if (info != null)
            {
                return info.Filename;
            }
            return string.Empty;
        }

        [Obsolete("This class is obsolete, use System.Configuration!System.Configuration.ConfigurationErrorsException.GetFilename instead")]
        public static string GetXmlNodeFilename(XmlNode node)
        {
            return SafeFilename(GetUnsafeXmlNodeFilename(node));
        }

        [Obsolete("This class is obsolete, use System.Configuration!System.Configuration.ConfigurationErrorsException.GetLinenumber instead")]
        public static int GetXmlNodeLineNumber(XmlNode node)
        {
            IConfigErrorInfo info = node as IConfigErrorInfo;
            if (info != null)
            {
                return info.LineNumber;
            }
            return 0;
        }

        private void Init(string filename, int line)
        {
            base.HResult = -2146232062;
            this._filename = filename;
            this._line = line;
        }

        internal static string SafeFilename(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                if (filename.StartsWith("http:", StringComparison.OrdinalIgnoreCase))
                {
                    return filename;
                }
                try
                {
                    Path.GetFullPath(filename);
                }
                catch (SecurityException)
                {
                    try
                    {
                        filename = Path.GetFileName(FullPathWithAssert(filename));
                    }
                    catch
                    {
                        filename = null;
                    }
                }
                catch
                {
                    filename = null;
                }
            }
            return filename;
        }

        public virtual string BareMessage
        {
            get
            {
                return base.Message;
            }
        }

        public virtual string Filename
        {
            get
            {
                return SafeFilename(this._filename);
            }
        }

        public virtual int Line
        {
            get
            {
                return this._line;
            }
        }

        public override string Message
        {
            get
            {
                string filename = this.Filename;
                if (!string.IsNullOrEmpty(filename))
                {
                    if (this.Line != 0)
                    {
                        return (this.BareMessage + " (" + filename + " line " + this.Line.ToString(CultureInfo.InvariantCulture) + ")");
                    }
                    return (this.BareMessage + " (" + filename + ")");
                }
                if (this.Line != 0)
                {
                    return (this.BareMessage + " (line " + this.Line.ToString("G", CultureInfo.InvariantCulture) + ")");
                }
                return this.BareMessage;
            }
        }
    }
}

