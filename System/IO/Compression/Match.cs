namespace System.IO.Compression
{
    using System;

    internal class Match
    {
        private int len;
        private int pos;
        private MatchState state;
        private byte symbol;

        internal int Length
        {
            get
            {
                return this.len;
            }
            set
            {
                this.len = value;
            }
        }

        internal int Position
        {
            get
            {
                return this.pos;
            }
            set
            {
                this.pos = value;
            }
        }

        internal MatchState State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        internal byte Symbol
        {
            get
            {
                return this.symbol;
            }
            set
            {
                this.symbol = value;
            }
        }
    }
}

