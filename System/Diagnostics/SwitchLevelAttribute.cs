namespace System.Diagnostics
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SwitchLevelAttribute : Attribute
    {
        private Type type;

        public SwitchLevelAttribute(Type switchLevelType)
        {
            this.SwitchLevelType = switchLevelType;
        }

        public Type SwitchLevelType
        {
            get
            {
                return this.type;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.type = value;
            }
        }
    }
}

