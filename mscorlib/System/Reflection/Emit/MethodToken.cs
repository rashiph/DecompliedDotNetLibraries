namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct MethodToken
    {
        public static readonly MethodToken Empty;
        internal int m_method;
        internal MethodToken(int str)
        {
            this.m_method = str;
        }

        public int Token
        {
            get
            {
                return this.m_method;
            }
        }
        public override int GetHashCode()
        {
            return this.m_method;
        }

        public override bool Equals(object obj)
        {
            return ((obj is MethodToken) && this.Equals((MethodToken) obj));
        }

        public bool Equals(MethodToken obj)
        {
            return (obj.m_method == this.m_method);
        }

        public static bool operator ==(MethodToken a, MethodToken b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MethodToken a, MethodToken b)
        {
            return !(a == b);
        }

        static MethodToken()
        {
            Empty = new MethodToken();
        }
    }
}

