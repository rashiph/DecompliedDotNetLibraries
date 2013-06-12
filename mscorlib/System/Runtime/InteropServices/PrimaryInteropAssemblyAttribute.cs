namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Assembly, Inherited=false, AllowMultiple=true)]
    public sealed class PrimaryInteropAssemblyAttribute : Attribute
    {
        internal int _major;
        internal int _minor;

        public PrimaryInteropAssemblyAttribute(int major, int minor)
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

