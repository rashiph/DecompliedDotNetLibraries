namespace System.ServiceModel.Channels
{
    using System;

    internal enum ClientFramingDecoderState
    {
        ReadingUpgradeRecord,
        ReadingUpgradeMode,
        UpgradeResponse,
        ReadingAckRecord,
        Start,
        ReadingFault,
        ReadingFaultString,
        Fault,
        ReadingEnvelopeRecord,
        ReadingEnvelopeSize,
        EnvelopeStart,
        ReadingEnvelopeBytes,
        EnvelopeEnd,
        ReadingEndRecord,
        End
    }
}

