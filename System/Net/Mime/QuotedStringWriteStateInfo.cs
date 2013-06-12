namespace System.Net.Mime
{
    using System;

    internal class QuotedStringWriteStateInfo : WriteStateInfoBase
    {
        internal QuotedStringWriteStateInfo(int buffersize, byte[] header, byte[] footer, int maxLineLength) : base(buffersize, header, footer, maxLineLength)
        {
        }
    }
}

