namespace System.Net.Cache
{
    using System;

    public class RequestCachePolicy
    {
        private RequestCacheLevel m_Level;

        public RequestCachePolicy() : this(RequestCacheLevel.Default)
        {
        }

        public RequestCachePolicy(RequestCacheLevel level)
        {
            if ((level < RequestCacheLevel.Default) || (level > RequestCacheLevel.NoCacheNoStore))
            {
                throw new ArgumentOutOfRangeException("level");
            }
            this.m_Level = level;
        }

        public override string ToString()
        {
            return ("Level:" + this.m_Level.ToString());
        }

        public RequestCacheLevel Level
        {
            get
            {
                return this.m_Level;
            }
        }
    }
}

