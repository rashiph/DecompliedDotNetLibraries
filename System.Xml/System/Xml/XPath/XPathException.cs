namespace System.Xml.XPath
{
    using System;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml;

    [Serializable]
    public class XPathException : SystemException
    {
        private string[] args;
        private string message;
        private string res;

        public XPathException() : this(string.Empty, (Exception) null)
        {
        }

        public XPathException(string message) : this(message, (Exception) null)
        {
        }

        protected XPathException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.res = (string) info.GetValue("res", typeof(string));
            this.args = (string[]) info.GetValue("args", typeof(string[]));
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
                this.message = CreateMessage(this.res, this.args);
            }
            else
            {
                this.message = null;
            }
        }

        public XPathException(string message, Exception innerException) : this("Xml_UserException", new string[] { message }, innerException)
        {
        }

        private XPathException(string res, string[] args) : this(res, args, null)
        {
        }

        private XPathException(string res, string[] args, Exception inner) : base(CreateMessage(res, args), inner)
        {
            base.HResult = -2146231997;
            this.res = res;
            this.args = args;
        }

        internal static XPathException Create(string res)
        {
            return new XPathException(res, null);
        }

        internal static XPathException Create(string res, string arg)
        {
            return new XPathException(res, new string[] { arg });
        }

        internal static XPathException Create(string res, string arg, Exception innerException)
        {
            return new XPathException(res, new string[] { arg }, innerException);
        }

        internal static XPathException Create(string res, string arg, string arg2)
        {
            return new XPathException(res, new string[] { arg, arg2 });
        }

        private static string CreateMessage(string res, string[] args)
        {
            try
            {
                string str = Res.GetString(res, args);
                if (str == null)
                {
                    str = "UNKNOWN(" + res + ")";
                }
                return str;
            }
            catch (MissingManifestResourceException)
            {
                return ("UNKNOWN(" + res + ")");
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("res", this.res);
            info.AddValue("args", this.args);
            info.AddValue("version", "2.0");
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
    }
}

