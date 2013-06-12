namespace System.Internal
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;

    internal class DebugHandleTracker
    {
        private static Hashtable handleTypes = new Hashtable();
        private static object internalSyncObject = new object();
        private static System.Internal.DebugHandleTracker tracker = new System.Internal.DebugHandleTracker();

        static DebugHandleTracker()
        {
            if ((System.ComponentModel.CompModSwitches.HandleLeak.Level > TraceLevel.Off) || System.ComponentModel.CompModSwitches.TraceCollect.Enabled)
            {
                System.Internal.HandleCollector.HandleAdded += new System.Internal.HandleChangeEventHandler(tracker.OnHandleAdd);
                System.Internal.HandleCollector.HandleRemoved += new System.Internal.HandleChangeEventHandler(tracker.OnHandleRemove);
            }
        }

        private DebugHandleTracker()
        {
        }

        public static void CheckLeaks()
        {
            lock (internalSyncObject)
            {
                if (System.ComponentModel.CompModSwitches.HandleLeak.Level >= TraceLevel.Warning)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    HandleType[] array = new HandleType[handleTypes.Values.Count];
                    handleTypes.Values.CopyTo(array, 0);
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] != null)
                        {
                            array[i].CheckLeaks();
                        }
                    }
                }
            }
        }

        public static void IgnoreCurrentHandlesAsLeaks()
        {
            lock (internalSyncObject)
            {
                if (System.ComponentModel.CompModSwitches.HandleLeak.Level >= TraceLevel.Warning)
                {
                    HandleType[] array = new HandleType[handleTypes.Values.Count];
                    handleTypes.Values.CopyTo(array, 0);
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] != null)
                        {
                            array[i].IgnoreCurrentHandlesAsLeaks();
                        }
                    }
                }
            }
        }

        public static void Initialize()
        {
        }

        private void OnHandleAdd(string handleName, IntPtr handle, int handleCount)
        {
            HandleType type = (HandleType) handleTypes[handleName];
            if (type == null)
            {
                type = new HandleType(handleName);
                handleTypes[handleName] = type;
            }
            type.Add(handle);
        }

        private void OnHandleRemove(string handleName, IntPtr handle, int HandleCount)
        {
            HandleType type = (HandleType) handleTypes[handleName];
            bool flag = false;
            if (type != null)
            {
                flag = type.Remove(handle);
            }
            if (!flag)
            {
                TraceLevel level = System.ComponentModel.CompModSwitches.HandleLeak.Level;
            }
        }

        private class HandleType
        {
            private HandleEntry[] buckets;
            private const int BUCKETS = 10;
            private int handleCount;
            public readonly string name;

            public HandleType(string name)
            {
                this.name = name;
                this.buckets = new HandleEntry[10];
            }

            public void Add(IntPtr handle)
            {
                lock (this)
                {
                    int index = this.ComputeHash(handle);
                    if (System.ComponentModel.CompModSwitches.HandleLeak.Level >= TraceLevel.Info)
                    {
                        TraceLevel level = System.ComponentModel.CompModSwitches.HandleLeak.Level;
                    }
                    for (HandleEntry entry = this.buckets[index]; entry != null; entry = entry.next)
                    {
                    }
                    this.buckets[index] = new HandleEntry(this.buckets[index], handle);
                    this.handleCount++;
                }
            }

            public void CheckLeaks()
            {
                lock (this)
                {
                    bool flag = false;
                    if (this.handleCount > 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            for (HandleEntry entry = this.buckets[i]; entry != null; entry = entry.next)
                            {
                                if (!entry.ignorableAsLeak && !flag)
                                {
                                    flag = true;
                                }
                            }
                        }
                    }
                }
            }

            private int ComputeHash(IntPtr handle)
            {
                return ((((int) handle) & 0xffff) % 10);
            }

            public void IgnoreCurrentHandlesAsLeaks()
            {
                lock (this)
                {
                    if (this.handleCount > 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            for (HandleEntry entry = this.buckets[i]; entry != null; entry = entry.next)
                            {
                                entry.ignorableAsLeak = true;
                            }
                        }
                    }
                }
            }

            public bool Remove(IntPtr handle)
            {
                lock (this)
                {
                    int index = this.ComputeHash(handle);
                    if (System.ComponentModel.CompModSwitches.HandleLeak.Level >= TraceLevel.Info)
                    {
                        TraceLevel level = System.ComponentModel.CompModSwitches.HandleLeak.Level;
                    }
                    HandleEntry next = this.buckets[index];
                    HandleEntry entry2 = null;
                    while ((next != null) && (next.handle != handle))
                    {
                        entry2 = next;
                        next = next.next;
                    }
                    if (next != null)
                    {
                        if (entry2 == null)
                        {
                            this.buckets[index] = next.next;
                        }
                        else
                        {
                            entry2.next = next.next;
                        }
                        this.handleCount--;
                        return true;
                    }
                    return false;
                }
            }

            private class HandleEntry
            {
                public readonly string callStack;
                public readonly IntPtr handle;
                public bool ignorableAsLeak;
                public System.Internal.DebugHandleTracker.HandleType.HandleEntry next;

                public HandleEntry(System.Internal.DebugHandleTracker.HandleType.HandleEntry next, IntPtr handle)
                {
                    this.handle = handle;
                    this.next = next;
                    if (System.ComponentModel.CompModSwitches.HandleLeak.Level > TraceLevel.Off)
                    {
                        this.callStack = Environment.StackTrace;
                    }
                    else
                    {
                        this.callStack = null;
                    }
                }

                public string ToString(System.Internal.DebugHandleTracker.HandleType type)
                {
                    StackParser parser = new StackParser(this.callStack);
                    parser.DiscardTo("HandleCollector.Add");
                    parser.DiscardNext();
                    parser.Truncate(40);
                    string str = "";
                    return (Convert.ToString((int) this.handle, 0x10) + str + ": " + parser.ToString());
                }

                private class StackParser
                {
                    internal int endIndex;
                    internal int length;
                    internal string releventStack;
                    internal int startIndex;

                    public StackParser(string callStack)
                    {
                        this.releventStack = callStack;
                        this.length = this.releventStack.Length;
                    }

                    private static bool ContainsString(string str, string token)
                    {
                        int length = str.Length;
                        int num2 = token.Length;
                        for (int i = 0; i < length; i++)
                        {
                            int num4 = 0;
                            while ((num4 < num2) && (str[i + num4] == token[num4]))
                            {
                                num4++;
                            }
                            if (num4 == num2)
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                    public void DiscardNext()
                    {
                        this.GetLine();
                    }

                    public void DiscardTo(string discardText)
                    {
                        while (this.startIndex < this.length)
                        {
                            string line = this.GetLine();
                            if (line == null)
                            {
                                break;
                            }
                            if (ContainsString(line, discardText))
                            {
                                return;
                            }
                        }
                    }

                    private string GetLine()
                    {
                        char ch;
                        this.endIndex = this.releventStack.IndexOf('\r', this.startIndex);
                        if (this.endIndex < 0)
                        {
                            this.endIndex = this.length - 1;
                        }
                        string str = this.releventStack.Substring(this.startIndex, this.endIndex - this.startIndex);
                        while ((this.endIndex < this.length) && (((ch = this.releventStack[this.endIndex]) == '\r') || (ch == '\n')))
                        {
                            this.endIndex++;
                        }
                        if (this.startIndex == this.endIndex)
                        {
                            return null;
                        }
                        this.startIndex = this.endIndex;
                        return str.Replace('\t', ' ');
                    }

                    public override string ToString()
                    {
                        return this.releventStack.Substring(this.startIndex);
                    }

                    public void Truncate(int lines)
                    {
                        string line = "";
                        while ((lines-- > 0) && (this.startIndex < this.length))
                        {
                            if (line == null)
                            {
                                line = this.GetLine();
                            }
                            else
                            {
                                line = line + ": " + this.GetLine();
                            }
                            line = line + Environment.NewLine;
                        }
                        this.releventStack = line;
                        this.startIndex = 0;
                        this.endIndex = 0;
                        this.length = this.releventStack.Length;
                    }
                }
            }
        }
    }
}

