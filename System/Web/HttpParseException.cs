namespace System.Web
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class HttpParseException : HttpException
    {
        private int _line;
        private ParserErrorCollection _parserErrors;
        private System.Web.VirtualPath _virtualPath;

        public HttpParseException()
        {
        }

        public HttpParseException(string message) : base(message)
        {
        }

        private HttpParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._virtualPath = (System.Web.VirtualPath) info.GetValue("_virtualPath", typeof(System.Web.VirtualPath));
            this._line = info.GetInt32("_line");
            this._parserErrors = (ParserErrorCollection) info.GetValue("_parserErrors", typeof(ParserErrorCollection));
        }

        public HttpParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public HttpParseException(string message, Exception innerException, string virtualPath, string sourceCode, int line) : this(message, innerException, System.Web.VirtualPath.CreateAllowNull(virtualPath), sourceCode, line)
        {
        }

        internal HttpParseException(string message, Exception innerException, System.Web.VirtualPath virtualPath, string sourceCode, int line) : base(message, innerException)
        {
            string str;
            this._virtualPath = virtualPath;
            this._line = line;
            if (innerException != null)
            {
                str = innerException.Message;
            }
            else
            {
                str = message;
            }
            base.SetFormatter(new ParseErrorFormatter(this, System.Web.VirtualPath.GetVirtualPathString(virtualPath), sourceCode, line, str));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_virtualPath", this._virtualPath);
            info.AddValue("_line", this._line);
            info.AddValue("_parserErrors", this._parserErrors);
        }

        public string FileName
        {
            get
            {
                string path = this._virtualPath.MapPathInternal();
                if (path == null)
                {
                    return null;
                }
                InternalSecurityPermissions.PathDiscovery(path).Demand();
                return path;
            }
        }

        public int Line
        {
            get
            {
                return this._line;
            }
        }

        public ParserErrorCollection ParserErrors
        {
            get
            {
                if (this._parserErrors == null)
                {
                    this._parserErrors = new ParserErrorCollection();
                    ParserError error = new ParserError(this.Message, this._virtualPath, this._line);
                    this._parserErrors.Add(error);
                }
                return this._parserErrors;
            }
        }

        public string VirtualPath
        {
            get
            {
                return System.Web.VirtualPath.GetVirtualPathString(this._virtualPath);
            }
        }

        internal System.Web.VirtualPath VirtualPathObject
        {
            get
            {
                return this._virtualPath;
            }
        }
    }
}

