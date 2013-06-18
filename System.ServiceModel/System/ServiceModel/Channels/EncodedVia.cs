namespace System.ServiceModel.Channels
{
    using System;

    internal class EncodedVia : EncodedFramingRecord
    {
        public EncodedVia(string via) : base(FramingRecordType.Via, via)
        {
        }
    }
}

