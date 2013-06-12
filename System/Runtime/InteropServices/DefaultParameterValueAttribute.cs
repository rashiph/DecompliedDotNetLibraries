namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DefaultParameterValueAttribute : Attribute
    {
        private object value;

        public DefaultParameterValueAttribute(object value)
        {
            this.value = value;
        }

        public object Value
        {
            get
            {
                return this.value;
            }
        }
    }
}

