namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field, Inherited=false), ComVisible(true)]
    public sealed class DateTimeConstantAttribute : CustomConstantAttribute
    {
        private DateTime date;

        public DateTimeConstantAttribute(long ticks)
        {
            this.date = new DateTime(ticks);
        }

        public override object Value
        {
            get
            {
                return this.date;
            }
        }
    }
}

