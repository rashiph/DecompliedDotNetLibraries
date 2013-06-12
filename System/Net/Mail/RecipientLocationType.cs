namespace System.Net.Mail
{
    using System;

    internal enum RecipientLocationType
    {
        Local,
        Unknown,
        NotLocal,
        WillForward,
        Ambiguous
    }
}

