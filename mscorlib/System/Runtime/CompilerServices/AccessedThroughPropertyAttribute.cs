namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Field)]
    public sealed class AccessedThroughPropertyAttribute : Attribute
    {
        private readonly string propertyName;

        public AccessedThroughPropertyAttribute(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }
    }
}

