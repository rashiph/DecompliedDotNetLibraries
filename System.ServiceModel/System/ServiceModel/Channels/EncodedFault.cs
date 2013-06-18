namespace System.ServiceModel.Channels
{
    using System;

    internal class EncodedFault : EncodedFramingRecord
    {
        public EncodedFault(string fault) : base(FramingRecordType.Fault, fault)
        {
        }
    }
}

