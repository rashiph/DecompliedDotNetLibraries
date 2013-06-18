namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class SoapServerMessage : SoapMessage
    {
        internal SoapExtension[] allExtensions;
        internal SoapExtension[] highPriConfigExtensions;
        internal SoapExtension[] otherExtensions;
        private SoapServerProtocol protocol;

        internal SoapServerMessage(SoapServerProtocol protocol)
        {
            this.protocol = protocol;
        }

        protected override void EnsureInStage()
        {
            base.EnsureStage(SoapMessageStage.AfterDeserialize);
        }

        protected override void EnsureOutStage()
        {
            base.EnsureStage(SoapMessageStage.BeforeSerialize);
        }

        public override string Action
        {
            get
            {
                return this.protocol.ServerMethod.action;
            }
        }

        public override LogicalMethodInfo MethodInfo
        {
            get
            {
                return this.protocol.MethodInfo;
            }
        }

        public override bool OneWay
        {
            get
            {
                return this.protocol.ServerMethod.oneWay;
            }
        }

        public object Server
        {
            get
            {
                base.EnsureStage(SoapMessageStage.AfterDeserialize | SoapMessageStage.BeforeSerialize);
                return this.protocol.Target;
            }
        }

        [ComVisible(false)]
        public override SoapProtocolVersion SoapVersion
        {
            get
            {
                return this.protocol.Version;
            }
        }

        public override string Url
        {
            get
            {
                return Uri.EscapeUriString(this.protocol.Request.Url.ToString()).Replace("#", "%23");
            }
        }
    }
}

