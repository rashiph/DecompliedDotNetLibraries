namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public sealed class KeySizes
    {
        private int m_maxSize;
        private int m_minSize;
        private int m_skipSize;

        public KeySizes(int minSize, int maxSize, int skipSize)
        {
            this.m_minSize = minSize;
            this.m_maxSize = maxSize;
            this.m_skipSize = skipSize;
        }

        public int MaxSize
        {
            get
            {
                return this.m_maxSize;
            }
        }

        public int MinSize
        {
            get
            {
                return this.m_minSize;
            }
        }

        public int SkipSize
        {
            get
            {
                return this.m_skipSize;
            }
        }
    }
}

