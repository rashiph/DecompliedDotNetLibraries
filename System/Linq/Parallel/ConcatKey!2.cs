namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ConcatKey<TLeftKey, TRightKey>
    {
        private readonly TLeftKey m_leftKey;
        private readonly TRightKey m_rightKey;
        private readonly bool m_isLeft;
        private ConcatKey(TLeftKey leftKey, TRightKey rightKey, bool isLeft)
        {
            this.m_leftKey = leftKey;
            this.m_rightKey = rightKey;
            this.m_isLeft = isLeft;
        }

        internal static ConcatKey<TLeftKey, TRightKey> MakeLeft(TLeftKey leftKey)
        {
            return new ConcatKey<TLeftKey, TRightKey>(leftKey, default(TRightKey), true);
        }

        internal static ConcatKey<TLeftKey, TRightKey> MakeRight(TRightKey rightKey)
        {
            return new ConcatKey<TLeftKey, TRightKey>(default(TLeftKey), rightKey, false);
        }

        internal static IComparer<ConcatKey<TLeftKey, TRightKey>> MakeComparer(IComparer<TLeftKey> leftComparer, IComparer<TRightKey> rightComparer)
        {
            return new ConcatKeyComparer<TLeftKey, TRightKey>(leftComparer, rightComparer);
        }
        private class ConcatKeyComparer : IComparer<ConcatKey<TLeftKey, TRightKey>>
        {
            private IComparer<TLeftKey> m_leftComparer;
            private IComparer<TRightKey> m_rightComparer;

            internal ConcatKeyComparer(IComparer<TLeftKey> leftComparer, IComparer<TRightKey> rightComparer)
            {
                this.m_leftComparer = leftComparer;
                this.m_rightComparer = rightComparer;
            }

            public int Compare(ConcatKey<TLeftKey, TRightKey> x, ConcatKey<TLeftKey, TRightKey> y)
            {
                if (x.m_isLeft != y.m_isLeft)
                {
                    if (!x.m_isLeft)
                    {
                        return 1;
                    }
                    return -1;
                }
                if (x.m_isLeft)
                {
                    return this.m_leftComparer.Compare(x.m_leftKey, y.m_leftKey);
                }
                return this.m_rightComparer.Compare(x.m_rightKey, y.m_rightKey);
            }
        }
    }
}

