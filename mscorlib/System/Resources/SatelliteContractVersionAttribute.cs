namespace System.Resources
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false), ComVisible(true)]
    public sealed class SatelliteContractVersionAttribute : Attribute
    {
        private string _version;

        public SatelliteContractVersionAttribute(string version)
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

