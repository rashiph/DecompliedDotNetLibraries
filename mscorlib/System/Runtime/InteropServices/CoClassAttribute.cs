namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Interface, Inherited=false), ComVisible(true)]
    public sealed class CoClassAttribute : Attribute
    {
        internal Type _CoClass;

        public CoClassAttribute(Type coClass)
        {
            this._CoClass = coClass;
        }

        public Type CoClass
        {
            get
            {
                return this._CoClass;
            }
        }
    }
}

