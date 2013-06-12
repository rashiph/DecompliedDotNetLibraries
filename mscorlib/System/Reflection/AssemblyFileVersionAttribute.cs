namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class AssemblyFileVersionAttribute : Attribute
    {
        private string _version;

        public AssemblyFileVersionAttribute(string version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            this._version = version;
        }

        public string Version
        {
            get
            {
                return this._version;
            }
        }
    }
}

