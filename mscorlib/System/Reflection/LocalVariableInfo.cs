namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class LocalVariableInfo
    {
        private int m_isPinned;
        private int m_localIndex;
        private RuntimeType m_type;

        protected LocalVariableInfo()
        {
        }

        public override string ToString()
        {
            string str = string.Concat(new object[] { this.LocalType.ToString(), " (", this.LocalIndex, ")" });
            if (this.IsPinned)
            {
                str = str + " (pinned)";
            }
            return str;
        }

        public virtual bool IsPinned
        {
            get
            {
                return (this.m_isPinned != 0);
            }
        }

        public virtual int LocalIndex
        {
            get
            {
                return this.m_localIndex;
            }
        }

        public virtual Type LocalType
        {
            get
            {
                return this.m_type;
            }
        }
    }
}

