namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class ViaStringDecoder : StringDecoder
    {
        private Uri via;

        public ViaStringDecoder(int sizeQuota) : base(sizeQuota)
        {
        }

        protected override void OnComplete(string value)
        {
            try
            {
                this.via = new Uri(value);
                base.OnComplete(value);
            }
            catch (UriFormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataException(System.ServiceModel.SR.GetString("FramingViaNotUri", new object[] { value }), exception));
            }
        }

        protected override Exception OnSizeQuotaExceeded(int size)
        {
            Exception exception = new InvalidDataException(System.ServiceModel.SR.GetString("FramingViaTooLong", new object[] { size }));
            FramingEncodingString.AddFaultString(exception, "http://schemas.microsoft.com/ws/2006/05/framing/faults/ViaTooLong");
            return exception;
        }

        public Uri ValueAsUri
        {
            get
            {
                if (!base.IsValueDecoded)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.via;
            }
        }
    }
}

