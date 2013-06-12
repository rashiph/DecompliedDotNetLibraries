namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
    public sealed class ImportedFromTypeLibAttribute : Attribute
    {
        internal string _val;

        public ImportedFromTypeLibAttribute(string tlbFile)
        {
            this._val = tlbFile;
        }

        public string Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

