namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;

    [Serializable]
    internal enum InternalParseTypeE
    {
        Empty,
        SerializedStreamHeader,
        Object,
        Member,
        ObjectEnd,
        MemberEnd,
        Headers,
        HeadersEnd,
        SerializedStreamHeaderEnd,
        Envelope,
        EnvelopeEnd,
        Body,
        BodyEnd
    }
}

