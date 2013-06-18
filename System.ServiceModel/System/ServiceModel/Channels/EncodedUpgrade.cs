namespace System.ServiceModel.Channels
{
    using System;

    internal class EncodedUpgrade : EncodedFramingRecord
    {
        public EncodedUpgrade(string contentType) : base(FramingRecordType.UpgradeRequest, contentType)
        {
        }
    }
}

