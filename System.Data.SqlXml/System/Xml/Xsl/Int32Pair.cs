namespace System.Xml.Xsl
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Int32Pair
    {
        private int left;
        private int right;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Int32Pair(int left, int right)
        {
            this.left = left;
            this.right = right;
        }

        public int Left
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.left;
            }
        }
        public int Right
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.right;
            }
        }
        public override bool Equals(object other)
        {
            if (!(other is Int32Pair))
            {
                return false;
            }
            Int32Pair pair = (Int32Pair) other;
            return ((this.left == pair.left) && (this.right == pair.right));
        }

        public override int GetHashCode()
        {
            return (this.left.GetHashCode() ^ this.right.GetHashCode());
        }
    }
}

