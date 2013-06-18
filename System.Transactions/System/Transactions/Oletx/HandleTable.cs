namespace System.Transactions.Oletx
{
    using System;
    using System.Collections.Generic;

    internal static class HandleTable
    {
        private static int currentHandle;
        private static Dictionary<int, object> handleTable = new Dictionary<int, object>(0x100);
        private static object syncRoot = new object();

        public static IntPtr AllocHandle(object target)
        {
            lock (syncRoot)
            {
                int key = FindAvailableHandle();
                handleTable.Add(key, target);
                return new IntPtr(key);
            }
        }

        private static int FindAvailableHandle()
        {
            int key = 0;
            do
            {
                key = (++currentHandle != 0) ? currentHandle : ++currentHandle;
            }
            while (handleTable.ContainsKey(key));
            return key;
        }

        public static object FindHandle(IntPtr handle)
        {
            lock (syncRoot)
            {
                object obj3;
                if (!handleTable.TryGetValue(handle.ToInt32(), out obj3))
                {
                    return null;
                }
                return obj3;
            }
        }

        public static bool FreeHandle(IntPtr handle)
        {
            lock (syncRoot)
            {
                return handleTable.Remove(handle.ToInt32());
            }
        }
    }
}

