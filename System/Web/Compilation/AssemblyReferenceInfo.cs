namespace System.Web.Compilation
{
    using System;
    using System.Reflection;

    internal class AssemblyReferenceInfo
    {
        internal System.Reflection.Assembly Assembly;
        internal int ReferenceIndex;

        internal AssemblyReferenceInfo(int referenceIndex)
        {
            this.ReferenceIndex = referenceIndex;
        }
    }
}

