namespace System.Runtime.CompilerServices
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Assembly, Inherited=false, AllowMultiple=false)]
    public sealed class RuntimeCompatibilityAttribute : Attribute
    {
        private bool m_wrapNonExceptionThrows;

        public bool WrapNonExceptionThrows
        {
            get
            {
                return this.m_wrapNonExceptionThrows;
            }
            set
            {
                this.m_wrapNonExceptionThrows = value;
            }
        }
    }
}

