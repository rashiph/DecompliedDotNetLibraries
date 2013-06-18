namespace System.Web.Services.Protocols
{
    using System;

    public enum SoapMessageStage
    {
        AfterDeserialize = 8,
        AfterSerialize = 2,
        BeforeDeserialize = 4,
        BeforeSerialize = 1
    }
}

