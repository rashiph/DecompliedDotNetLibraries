namespace System.Net.Mime
{
    using System;

    internal enum MimeMultiPartType
    {
        Alternative = 1,
        Mixed = 0,
        Parallel = 2,
        Related = 3,
        Unknown = -1
    }
}

