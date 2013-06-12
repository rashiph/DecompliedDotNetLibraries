namespace System.Net
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class WebRequestPrefixElement
    {
        internal IWebRequestCreate creator;
        internal Type creatorType;
        public string Prefix;

        public WebRequestPrefixElement(string P, IWebRequestCreate C)
        {
            this.Prefix = P;
            this.Creator = C;
        }

        public WebRequestPrefixElement(string P, Type creatorType)
        {
            if (!typeof(IWebRequestCreate).IsAssignableFrom(creatorType))
            {
                throw new InvalidCastException(SR.GetString("net_invalid_cast", new object[] { creatorType.AssemblyQualifiedName, "IWebRequestCreate" }));
            }
            this.Prefix = P;
            this.creatorType = creatorType;
        }

        public IWebRequestCreate Creator
        {
            get
            {
                if ((this.creator == null) && (this.creatorType != null))
                {
                    lock (this)
                    {
                        if (this.creator == null)
                        {
                            this.creator = (IWebRequestCreate) Activator.CreateInstance(this.creatorType, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[0], CultureInfo.InvariantCulture);
                        }
                    }
                }
                return this.creator;
            }
            set
            {
                this.creator = value;
            }
        }
    }
}

