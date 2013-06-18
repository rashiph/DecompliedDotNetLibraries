namespace System.ServiceProcess
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SessionChangeDescription
    {
        private SessionChangeReason _reason;
        private int _id;
        public SessionChangeReason Reason
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._reason;
            }
        }
        public int SessionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._id;
            }
        }
        public override bool Equals(object obj)
        {
            return (((obj != null) && (obj is SessionChangeDescription)) && this.Equals((SessionChangeDescription) obj));
        }

        public override int GetHashCode()
        {
            return (((int) this._reason) ^ this._id);
        }

        public bool Equals(SessionChangeDescription changeDescription)
        {
            return ((this._reason == changeDescription._reason) && (this._id == changeDescription._id));
        }

        public static bool operator ==(SessionChangeDescription a, SessionChangeDescription b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SessionChangeDescription a, SessionChangeDescription b)
        {
            return !a.Equals(b);
        }

        internal SessionChangeDescription(SessionChangeReason reason, int id)
        {
            this._reason = reason;
            this._id = id;
        }
    }
}

