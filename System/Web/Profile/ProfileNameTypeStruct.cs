namespace System.Web.Profile
{
    using System;
    using System.CodeDom;

    internal class ProfileNameTypeStruct
    {
        internal string FileName;
        internal bool IsReadOnly;
        internal int LineNumber;
        internal string Name;
        internal CodeTypeReference PropertyCodeRefType;
        internal Type PropertyType;
    }
}

