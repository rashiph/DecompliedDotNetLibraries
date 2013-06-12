namespace System.Net.Mime
{
    using System;
    using System.Runtime.CompilerServices;

    internal class Base64WriteStateInfo : WriteStateInfoBase
    {
        internal Base64WriteStateInfo()
        {
        }

        internal Base64WriteStateInfo(int bufferSize, byte[] header, byte[] footer, int maxLineLength) : base(bufferSize, header, footer, maxLineLength)
        {
        }

        internal byte LastBits { get; set; }

        internal int Padding { get; set; }
    }
}

