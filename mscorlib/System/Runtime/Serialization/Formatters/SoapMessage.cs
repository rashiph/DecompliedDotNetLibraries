namespace System.Runtime.Serialization.Formatters
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;

    [Serializable, ComVisible(true)]
    public class SoapMessage : ISoapMessage
    {
        internal Header[] headers;
        internal string methodName;
        internal string[] paramNames;
        internal Type[] paramTypes;
        internal object[] paramValues;
        internal string xmlNameSpace;

        public Header[] Headers
        {
            get
            {
                return this.headers;
            }
            set
            {
                this.headers = value;
            }
        }

        public string MethodName
        {
            get
            {
                return this.methodName;
            }
            set
            {
                this.methodName = value;
            }
        }

        public string[] ParamNames
        {
            get
            {
                return this.paramNames;
            }
            set
            {
                this.paramNames = value;
            }
        }

        public Type[] ParamTypes
        {
            get
            {
                return this.paramTypes;
            }
            set
            {
                this.paramTypes = value;
            }
        }

        public object[] ParamValues
        {
            get
            {
                return this.paramValues;
            }
            set
            {
                this.paramValues = value;
            }
        }

        public string XmlNameSpace
        {
            get
            {
                return this.xmlNameSpace;
            }
            set
            {
                this.xmlNameSpace = value;
            }
        }
    }
}

