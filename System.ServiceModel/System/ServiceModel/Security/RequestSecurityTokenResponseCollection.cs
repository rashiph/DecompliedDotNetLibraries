namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal sealed class RequestSecurityTokenResponseCollection : BodyWriter
    {
        private IEnumerable<RequestSecurityTokenResponse> rstrCollection;
        private SecurityStandardsManager standardsManager;

        public RequestSecurityTokenResponseCollection(IEnumerable<RequestSecurityTokenResponse> rstrCollection) : this(rstrCollection, SecurityStandardsManager.DefaultInstance)
        {
        }

        public RequestSecurityTokenResponseCollection(IEnumerable<RequestSecurityTokenResponse> rstrCollection, SecurityStandardsManager standardsManager) : base(true)
        {
            if (rstrCollection == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstrCollection");
            }
            int num = 0;
            using (IEnumerator<RequestSecurityTokenResponse> enumerator = rstrCollection.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "rstrCollection[{0}]", new object[] { num }));
                    }
                    num++;
                }
            }
            this.rstrCollection = rstrCollection;
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            this.WriteTo(writer);
        }

        public void WriteTo(XmlWriter writer)
        {
            this.standardsManager.TrustDriver.WriteRequestSecurityTokenResponseCollection(this, writer);
        }

        public IEnumerable<RequestSecurityTokenResponse> RstrCollection
        {
            get
            {
                return this.rstrCollection;
            }
        }
    }
}

