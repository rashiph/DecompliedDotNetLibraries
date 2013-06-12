namespace System.Security.Util
{
    using System;

    internal sealed class TokenizerStream
    {
        private int m_countTokens = 0;
        private TokenizerStringBlock m_currentStrings;
        private TokenizerShortBlock m_currentTokens;
        private TokenizerStringBlock m_headStrings = new TokenizerStringBlock();
        private TokenizerShortBlock m_headTokens = new TokenizerShortBlock();
        private int m_indexStrings;
        private int m_indexTokens;
        private TokenizerShortBlock m_lastTokens;

        internal TokenizerStream()
        {
            this.Reset();
        }

        internal void AddString(string str)
        {
            if (this.m_currentStrings.m_block.Length <= this.m_indexStrings)
            {
                this.m_currentStrings.m_next = new TokenizerStringBlock();
                this.m_currentStrings = this.m_currentStrings.m_next;
                this.m_indexStrings = 0;
            }
            this.m_currentStrings.m_block[this.m_indexStrings++] = str;
        }

        internal void AddToken(short token)
        {
            if (this.m_currentTokens.m_block.Length <= this.m_indexTokens)
            {
                this.m_currentTokens.m_next = new TokenizerShortBlock();
                this.m_currentTokens = this.m_currentTokens.m_next;
                this.m_indexTokens = 0;
            }
            this.m_countTokens++;
            this.m_currentTokens.m_block[this.m_indexTokens++] = token;
        }

        internal short GetNextFullToken()
        {
            if (this.m_currentTokens.m_block.Length <= this.m_indexTokens)
            {
                this.m_lastTokens = this.m_currentTokens;
                this.m_currentTokens = this.m_currentTokens.m_next;
                this.m_indexTokens = 0;
            }
            return this.m_currentTokens.m_block[this.m_indexTokens++];
        }

        internal string GetNextString()
        {
            if (this.m_currentStrings.m_block.Length <= this.m_indexStrings)
            {
                this.m_currentStrings = this.m_currentStrings.m_next;
                this.m_indexStrings = 0;
            }
            return this.m_currentStrings.m_block[this.m_indexStrings++];
        }

        internal short GetNextToken()
        {
            return (short) (this.GetNextFullToken() & 0xff);
        }

        internal int GetTokenCount()
        {
            return this.m_countTokens;
        }

        internal void GoToPosition(int position)
        {
            this.Reset();
            for (int i = 0; i < position; i++)
            {
                if (this.GetNextToken() == 3)
                {
                    this.ThrowAwayNextString();
                }
            }
        }

        internal void Reset()
        {
            this.m_lastTokens = null;
            this.m_currentTokens = this.m_headTokens;
            this.m_currentStrings = this.m_headStrings;
            this.m_indexTokens = 0;
            this.m_indexStrings = 0;
        }

        internal void TagLastToken(short tag)
        {
            if (this.m_indexTokens == 0)
            {
                this.m_lastTokens.m_block[this.m_lastTokens.m_block.Length - 1] = (short) (((ushort) this.m_lastTokens.m_block[this.m_lastTokens.m_block.Length - 1]) | ((ushort) tag));
            }
            else
            {
                this.m_currentTokens.m_block[this.m_indexTokens - 1] = (short) (((ushort) this.m_currentTokens.m_block[this.m_indexTokens - 1]) | ((ushort) tag));
            }
        }

        internal void ThrowAwayNextString()
        {
            this.GetNextString();
        }
    }
}

