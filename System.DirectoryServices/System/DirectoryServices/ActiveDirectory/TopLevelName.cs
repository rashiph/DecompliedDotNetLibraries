namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    public class TopLevelName
    {
        private string name;
        private TopLevelNameStatus status;
        internal LARGE_INTEGER time;

        internal TopLevelName(int flag, LSA_UNICODE_STRING val, LARGE_INTEGER time)
        {
            this.status = (TopLevelNameStatus) flag;
            this.name = Marshal.PtrToStringUni(val.Buffer, val.Length / 2);
            this.time = time;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public TopLevelNameStatus Status
        {
            get
            {
                return this.status;
            }
            set
            {
                if (((value != TopLevelNameStatus.Enabled) && (value != TopLevelNameStatus.NewlyCreated)) && ((value != TopLevelNameStatus.AdminDisabled) && (value != TopLevelNameStatus.ConflictDisabled)))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(TopLevelNameStatus));
                }
                this.status = value;
            }
        }
    }
}

