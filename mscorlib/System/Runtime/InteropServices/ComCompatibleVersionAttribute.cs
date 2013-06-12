namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
    public sealed class ComCompatibleVersionAttribute : Attribute
    {
        internal int _build;
        internal int _major;
        internal int _minor;
        internal int _revision;

        public ComCompatibleVersionAttribute(int major, int minor, int build, int revision)
        {
            this._major = major;
            this._minor = minor;
            this._build = build;
            this._revision = revision;
        }

        public int BuildNumber
        {
            get
            {
                return this._build;
            }
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

        public int RevisionNumber
        {
            get
            {
                return this._revision;
            }
        }
    }
}

