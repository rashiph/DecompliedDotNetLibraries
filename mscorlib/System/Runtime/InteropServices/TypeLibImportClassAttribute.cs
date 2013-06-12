namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Interface, Inherited=false)]
    public sealed class TypeLibImportClassAttribute : Attribute
    {
        internal string _importClassName;

        public TypeLibImportClassAttribute(Type importClass)
        {
            this._importClassName = importClass.ToString();
        }

        public string Value
        {
            get
            {
                return this._importClassName;
            }
        }
    }
}

