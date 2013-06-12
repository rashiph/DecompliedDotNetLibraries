namespace System.Collections.Specialized
{
    using System;
    using System.Collections;

    internal class BackCompatibleStringComparer : IEqualityComparer
    {
        internal static IEqualityComparer Default = new BackCompatibleStringComparer();

        internal BackCompatibleStringComparer()
        {
        }

        public virtual int GetHashCode(object o)
        {
            string str = o as string;
            if (str == null)
            {
                return o.GetHashCode();
            }
            return GetHashCode(str);
        }

        public static unsafe int GetHashCode(string obj)
        {
            fixed (char* str = ((char*) obj))
            {
                int num2;
                char* chPtr = str;
                int num = 0x1505;
                for (char* chPtr2 = chPtr; (num2 = chPtr2[0]) != '\0'; chPtr2++)
                {
                    num = ((num << 5) + num) ^ num2;
                }
                return num;
            }
        }

        bool IEqualityComparer.Equals(object a, object b)
        {
            return object.Equals(a, b);
        }
    }
}

