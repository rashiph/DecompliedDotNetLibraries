namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct WaitForChangedResult
    {
        private WatcherChangeTypes changeType;
        private string name;
        private string oldName;
        private bool timedOut;
        internal static readonly WaitForChangedResult TimedOutResult;
        internal WaitForChangedResult(WatcherChangeTypes changeType, string name, bool timedOut) : this(changeType, name, null, timedOut)
        {
        }

        internal WaitForChangedResult(WatcherChangeTypes changeType, string name, string oldName, bool timedOut)
        {
            this.changeType = changeType;
            this.name = name;
            this.oldName = oldName;
            this.timedOut = timedOut;
        }

        public WatcherChangeTypes ChangeType
        {
            get
            {
                return this.changeType;
            }
            set
            {
                this.changeType = value;
            }
        }
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
        public string OldName
        {
            get
            {
                return this.oldName;
            }
            set
            {
                this.oldName = value;
            }
        }
        public bool TimedOut
        {
            get
            {
                return this.timedOut;
            }
            set
            {
                this.timedOut = value;
            }
        }
        static WaitForChangedResult()
        {
            TimedOutResult = new WaitForChangedResult(0, null, true);
        }
    }
}

