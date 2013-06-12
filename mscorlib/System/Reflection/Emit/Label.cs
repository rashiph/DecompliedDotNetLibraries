namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct Label
    {
        internal int m_label;
        internal Label(int label)
        {
            this.m_label = label;
        }

        internal int GetLabelValue()
        {
            return this.m_label;
        }

        public override int GetHashCode()
        {
            return this.m_label;
        }

        public override bool Equals(object obj)
        {
            return ((obj is Label) && this.Equals((Label) obj));
        }

        public bool Equals(Label obj)
        {
            return (obj.m_label == this.m_label);
        }

        public static bool operator ==(Label a, Label b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Label a, Label b)
        {
            return !(a == b);
        }
    }
}

