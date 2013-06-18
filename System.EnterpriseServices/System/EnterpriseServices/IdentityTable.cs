namespace System.EnterpriseServices
{
    using System;
    using System.Collections;

    internal static class IdentityTable
    {
        private static Hashtable _table = new Hashtable();

        public static void AddObject(IntPtr key, object val)
        {
            lock (_table)
            {
                WeakReference reference = _table[key] as WeakReference;
                if (reference == null)
                {
                    reference = new WeakReference(val, false);
                    _table.Add(key, reference);
                }
                else if (reference.Target == null)
                {
                    reference.Target = val;
                }
            }
        }

        public static object FindObject(IntPtr key)
        {
            object target = null;
            lock (_table)
            {
                WeakReference reference = _table[key] as WeakReference;
                if (reference != null)
                {
                    target = reference.Target;
                }
            }
            return target;
        }

        public static void RemoveObject(IntPtr key, object val)
        {
            lock (_table)
            {
                WeakReference reference = _table[key] as WeakReference;
                if ((reference != null) && ((reference.Target == val) || (reference.Target == null)))
                {
                    _table.Remove(key);
                    reference.Target = null;
                }
            }
        }
    }
}

