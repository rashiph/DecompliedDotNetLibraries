namespace System.Security.Util
{
    using System;

    internal sealed class TokenizerStringBlock
    {
        internal string[] m_block = new string[0x10];
        internal TokenizerStringBlock m_next;
    }
}

