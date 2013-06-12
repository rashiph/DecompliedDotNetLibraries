namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Field, Inherited=false)]
    public sealed class OptionalFieldAttribute : Attribute
    {
        private int versionAdded = 1;

        public int VersionAdded
        {
            get
            {
                return this.versionAdded;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException(Environment.GetResourceString("Serialization_OptionalFieldVersionValue"));
                }
                this.versionAdded = value;
            }
        }
    }
}

