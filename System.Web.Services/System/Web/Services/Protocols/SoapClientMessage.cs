namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public sealed class SoapClientMessage : SoapMessage
    {
        internal SoapExtension[] initializedExtensions;
        private SoapClientMethod method;
        private SoapHttpClientProtocol protocol;
        private string url;

        internal SoapClientMessage(SoapHttpClientProtocol protocol, SoapClientMethod method, string url)
        {
            this.method = method;
            this.protocol = protocol;
            this.url = url;
        }

        protected override void EnsureInStage()
        {
            base.EnsureStage(SoapMessageStage.BeforeSerialize);
        }

        protected override void EnsureOutStage()
        {
            base.EnsureStage(SoapMessageStage.AfterDeserialize);
        }

        public override string Action
        {
            get
            {
                return this.method.action;
            }
        }

        public SoapHttpClientProtocol Client
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocol;
            }
        }

        internal SoapClientMethod Method
        {
            get
            {
                return this.method;
            }
        }

        public override LogicalMethodInfo MethodInfo
        {
            get
            {
                return this.method.methodInfo;
            }
        }

        public override bool OneWay
        {
            get
            {
                return this.method.oneWay;
            }
        }

        [ComVisible(false)]
        public override SoapProtocolVersion SoapVersion
        {
            get
            {
                if (this.protocol.SoapVersion != SoapProtocolVersion.Default)
                {
                    return this.protocol.SoapVersion;
                }
                return SoapProtocolVersion.Soap11;
            }
        }

        public override string Url
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.url;
            }
        }
    }
}

