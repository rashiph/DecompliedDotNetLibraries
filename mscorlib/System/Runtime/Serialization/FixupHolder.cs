namespace System.Runtime.Serialization
{
    using System;

    [Serializable]
    internal class FixupHolder
    {
        internal const int ArrayFixup = 1;
        internal const int DelayedFixup = 4;
        internal object m_fixupInfo;
        internal int m_fixupType;
        internal long m_id;
        internal const int MemberFixup = 2;

        internal FixupHolder(long id, object fixupInfo, int fixupType)
        {
            this.m_id = id;
            this.m_fixupInfo = fixupInfo;
            this.m_fixupType = fixupType;
        }
    }
}

