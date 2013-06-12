namespace System.Drawing.Internal
{
    using Microsoft.Win32;
    using System;

    internal class SystemColorTracker
    {
        private static bool addedTracker;
        private static int count = 0;
        private static int EXPAND_FACTOR = 2;
        private static float EXPAND_THRESHOLD = 0.75f;
        private static int INITIAL_SIZE = 200;
        private static WeakReference[] list = new WeakReference[INITIAL_SIZE];
        private static int WARNING_SIZE = 0x186a0;

        private SystemColorTracker()
        {
        }

        internal static void Add(ISystemColorTracker obj)
        {
            lock (typeof(SystemColorTracker))
            {
                if (list.Length == SystemColorTracker.count)
                {
                    GarbageCollectList();
                }
                if (!addedTracker)
                {
                    addedTracker = true;
                    SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemColorTracker.OnUserPreferenceChanged);
                }
                int count = SystemColorTracker.count;
                SystemColorTracker.count++;
                if (list[count] == null)
                {
                    list[count] = new WeakReference(obj);
                }
                else
                {
                    list[count].Target = obj;
                }
            }
        }

        private static void CleanOutBrokenLinks()
        {
            int index = list.Length - 1;
            int num2 = 0;
            int length = list.Length;
        Label_001A:
            while ((num2 < length) && (list[num2].Target != null))
            {
                num2++;
            }
            while ((index >= 0) && (list[index].Target == null))
            {
                index--;
            }
            if (num2 >= index)
            {
                count = num2;
            }
            else
            {
                WeakReference reference = list[num2];
                list[num2] = list[index];
                list[index] = reference;
                num2++;
                index--;
                goto Label_001A;
            }
        }

        private static void GarbageCollectList()
        {
            CleanOutBrokenLinks();
            if ((((float) count) / ((float) list.Length)) > EXPAND_THRESHOLD)
            {
                WeakReference[] array = new WeakReference[list.Length * EXPAND_FACTOR];
                list.CopyTo(array, 0);
                list = array;
                int num1 = WARNING_SIZE;
                int length = list.Length;
            }
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
            {
                for (int i = 0; i < count; i++)
                {
                    ISystemColorTracker target = (ISystemColorTracker) list[i].Target;
                    if (target != null)
                    {
                        target.OnSystemColorChanged();
                    }
                }
            }
        }
    }
}

