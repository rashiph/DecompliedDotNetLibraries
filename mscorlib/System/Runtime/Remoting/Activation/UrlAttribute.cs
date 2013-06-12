namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Contexts;
    using System.Security;

    [Serializable, ComVisible(true), SecurityCritical]
    public sealed class UrlAttribute : ContextAttribute
    {
        private static string propertyName = "UrlAttribute";
        private string url;

        [SecurityCritical]
        public UrlAttribute(string callsiteURL) : base(propertyName)
        {
            if (callsiteURL == null)
            {
                throw new ArgumentNullException("callsiteURL");
            }
            this.url = callsiteURL;
        }

        [SecuritySafeCritical]
        public override bool Equals(object o)
        {
            return (((o is IContextProperty) && (o is UrlAttribute)) && ((UrlAttribute) o).UrlValue.Equals(this.url));
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            return this.url.GetHashCode();
        }

        [SecurityCritical, ComVisible(true)]
        public override void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
        {
        }

        [SecurityCritical, ComVisible(true)]
        public override bool IsContextOK(Context ctx, IConstructionCallMessage msg)
        {
            return false;
        }

        public string UrlValue
        {
            [SecurityCritical]
            get
            {
                return this.url;
            }
        }
    }
}

