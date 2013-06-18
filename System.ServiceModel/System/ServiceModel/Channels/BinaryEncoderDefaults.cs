namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal static class BinaryEncoderDefaults
    {
        internal const int MaxSessionSize = 0x800;

        internal static System.ServiceModel.Channels.BinaryVersion BinaryVersion
        {
            get
            {
                return System.ServiceModel.Channels.BinaryVersion.Version1;
            }
        }

        internal static System.ServiceModel.EnvelopeVersion EnvelopeVersion
        {
            get
            {
                return System.ServiceModel.EnvelopeVersion.Soap12;
            }
        }
    }
}

