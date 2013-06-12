namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field, Inherited=false), ComVisible(true)]
    public sealed class IDispatchConstantAttribute : CustomConstantAttribute
    {
        public override object Value
        {
            get
            {
                return new DispatchWrapper(null);
            }
        }
    }
}

