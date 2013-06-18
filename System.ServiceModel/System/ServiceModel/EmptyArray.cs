namespace System.ServiceModel
{
    using System;

    internal class EmptyArray
    {
        private static object[] instance = new object[0];

        private EmptyArray()
        {
        }

        internal static object[] Allocate(int n)
        {
            if (n == 0)
            {
                return Instance;
            }
            return new object[n];
        }

        internal static object[] Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

