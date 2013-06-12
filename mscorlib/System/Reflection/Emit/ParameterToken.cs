namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct ParameterToken
    {
        public static readonly ParameterToken Empty;
        internal int m_tkParameter;
        internal ParameterToken(int tkParam)
        {
            this.m_tkParameter = tkParam;
        }

        public int Token
        {
            get
            {
                return this.m_tkParameter;
            }
        }
        public override int GetHashCode()
        {
            return this.m_tkParameter;
        }

        public override bool Equals(object obj)
        {
            return ((obj is ParameterToken) && this.Equals((ParameterToken) obj));
        }

        public bool Equals(ParameterToken obj)
        {
            return (obj.m_tkParameter == this.m_tkParameter);
        }

        public static bool operator ==(ParameterToken a, ParameterToken b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ParameterToken a, ParameterToken b)
        {
            return !(a == b);
        }

        static ParameterToken()
        {
            Empty = new ParameterToken();
        }
    }
}

