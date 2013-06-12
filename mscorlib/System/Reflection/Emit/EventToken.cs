namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct EventToken
    {
        public static readonly EventToken Empty;
        internal int m_event;
        internal EventToken(int str)
        {
            this.m_event = str;
        }

        public int Token
        {
            get
            {
                return this.m_event;
            }
        }
        public override int GetHashCode()
        {
            return this.m_event;
        }

        public override bool Equals(object obj)
        {
            return ((obj is EventToken) && this.Equals((EventToken) obj));
        }

        public bool Equals(EventToken obj)
        {
            return (obj.m_event == this.m_event);
        }

        public static bool operator ==(EventToken a, EventToken b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(EventToken a, EventToken b)
        {
            return !(a == b);
        }

        static EventToken()
        {
            Empty = new EventToken();
        }
    }
}

