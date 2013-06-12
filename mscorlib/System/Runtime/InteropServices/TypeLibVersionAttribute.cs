namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class TypeLibVersionAttribute : Attribute
    {
        internal int _major;
        internal int _minor;

        public TypeLibVersionAttribute(int major, int minor)
        {
            this._major = major;
            this._minor = minor;
        }

        public int MajorVersion
        {
            get
            {
                return this._major;
            }
        }

        public int MinorVersion
        {
            get
            {
                return this._minor;
            }
        }
    }
}

