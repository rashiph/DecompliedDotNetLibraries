namespace System.Xaml.MS.Impl
{
    using System;
    using System.Runtime.CompilerServices;

    internal class PositionalParameterDescriptor
    {
        public PositionalParameterDescriptor(object value, bool wasText)
        {
            this.Value = value;
            this.WasText = wasText;
        }

        public object Value { get; set; }

        public bool WasText { get; set; }
    }
}

