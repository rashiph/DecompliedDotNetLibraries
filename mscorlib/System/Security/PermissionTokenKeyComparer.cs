namespace System.Security
{
    using System;
    using System.Collections;
    using System.Globalization;

    [Serializable]
    internal sealed class PermissionTokenKeyComparer : IEqualityComparer
    {
        private Comparer _caseSensitiveComparer = new Comparer(CultureInfo.InvariantCulture);
        private TextInfo _info = CultureInfo.InvariantCulture.TextInfo;

        [SecuritySafeCritical]
        public int Compare(object a, object b)
        {
            string strLeft = a as string;
            string strRight = b as string;
            if ((strLeft == null) || (strRight == null))
            {
                return this._caseSensitiveComparer.Compare(a, b);
            }
            int num = this._caseSensitiveComparer.Compare(a, b);
            if (num == 0)
            {
                return 0;
            }
            if (SecurityManager.IsSameType(strLeft, strRight))
            {
                return 0;
            }
            return num;
        }

        public bool Equals(object a, object b)
        {
            return ((a == b) || (((a != null) && (b != null)) && (this.Compare(a, b) == 0)));
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            string str = obj as string;
            if (str == null)
            {
                return obj.GetHashCode();
            }
            int index = str.IndexOf(',');
            if (index == -1)
            {
                index = str.Length;
            }
            int num2 = 0;
            for (int i = 0; i < index; i++)
            {
                num2 = ((num2 << 7) ^ str[i]) ^ (num2 >> 0x19);
            }
            return num2;
        }
    }
}

