namespace System.Security.Util
{
    using System;

    internal sealed class TokenizerShortBlock
    {
        internal short[] m_block = new short[0x10];
        internal TokenizerShortBlock m_next;
    }
}

