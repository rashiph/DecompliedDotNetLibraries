namespace System.Xml.Xsl
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml.Utils;

    [Serializable]
    public class XsltException : SystemException
    {
        private string[] args;
        private int lineNumber;
        private int linePosition;
        private string message;
        private string res;
        private string sourceUri;

        public XsltException() : this(string.Empty, null)
        {
        }

        public XsltException(string message) : this(message, null)
        {
        }

        protected XsltException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.res = (string) info.GetValue("res", typeof(string));
            this.args = (string[]) info.GetValue("args", typeof(string[]));
            this.sourceUri = (string) info.GetValue("sourceUri", typeof(string));
            this.lineNumber = (int) info.GetValue("lineNumber", typeof(int));
            this.linePosition = (int) info.GetValue("linePosition", typeof(int));
            string str = null;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SerializationEntry current = enumerator.Current;
                if (current.Name == "version")
                {
                    str = (string) current.Value;
                }
            }
            if (str == null)
            {
                this.message = CreateMessage(this.res, this.args, this.sourceUri, this.lineNumber, this.linePosition);
            }
            else
            {
                this.message = null;
            }
        }

        public XsltException(string message, Exception innerException) : this("Xml_UserException", new string[] { message }, null, 0, 0, innerException)
        {
        }

        internal XsltException(string res, string[] args, string sourceUri, int lineNumber, int linePosition, Exception inner) : base(CreateMessage(res, args, sourceUri, lineNumber, linePosition), inner)
        {
            base.HResult = -2146231998;
            this.res = res;
            this.sourceUri = sourceUri;
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
        }

        internal static XsltException Create(string res, params string[] args)
        {
            return new XsltException(res, args, null, 0, 0, null);
        }

        internal static XsltException Create(string res, string[] args, Exception inner)
        {
            return new XsltException(res, args, null, 0, 0, inner);
        }

        private static string CreateMessage(string res, string[] args, string sourceUri, int lineNumber, int linePosition)
        {
            try
            {
                string str = FormatMessage(res, args);
                if ((res != "Xslt_CompileError") && (lineNumber != 0))
                {
                    str = str + " " + FormatMessage("Xml_ErrorFilePosition", new string[] { sourceUri, lineNumber.ToString(CultureInfo.InvariantCulture), linePosition.ToString(CultureInfo.InvariantCulture) });
                }
                return str;
            }
            catch (MissingManifestResourceException)
            {
                return ("UNKNOWN(" + res + ")");
            }
        }

        private static string FormatMessage(string key, params string[] args)
        {
            string format = Res.GetString(key);
            if ((format != null) && (args != null))
            {
                format = string.Format(CultureInfo.InvariantCulture, format, args);
            }
            return format;
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("res", this.res);
            info.AddValue("args", this.args);
            info.AddValue("sourceUri", this.sourceUri);
            info.AddValue("lineNumber", this.lineNumber);
            info.AddValue("linePosition", this.linePosition);
            info.AddValue("version", "2.0");
        }

        public virtual int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
        }

        public virtual int LinePosition
        {
            get
            {
                return this.linePosition;
            }
        }

        public override string Message
        {
            get
            {
                if (this.message != null)
                {
                    return this.message;
                }
                return base.Message;
            }
        }

        public virtual string SourceUri
        {
            get
            {
                return this.sourceUri;
            }
        }
    }
}

