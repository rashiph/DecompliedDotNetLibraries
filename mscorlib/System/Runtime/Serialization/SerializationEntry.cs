namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct SerializationEntry
    {
        private Type m_type;
        private object m_value;
        private string m_name;
        public object Value
        {
            get
            {
                return this.m_value;
            }
        }
        public string Name
        {
            get
            {
                return this.m_name;
            }
        }
        public Type ObjectType
        {
            get
            {
                return this.m_type;
            }
        }
        internal SerializationEntry(string entryName, object entryValue, Type entryType)
        {
            this.m_value = entryValue;
            this.m_name = entryName;
            this.m_type = entryType;
        }
    }
}

