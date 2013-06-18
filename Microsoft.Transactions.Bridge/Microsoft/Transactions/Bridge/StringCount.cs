namespace Microsoft.Transactions.Bridge
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StringCount
    {
        public string Name;
        public int Count;
        private static StringCount nullCount;
        public StringCount(string name)
        {
            this.Name = name;
            this.Count = 1;
        }

        public static StringCount Null
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return nullCount;
            }
        }
        static StringCount()
        {
            nullCount = new StringCount(null);
        }
    }
}

