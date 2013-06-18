namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=false), EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class OptionTextAttribute : Attribute
    {
    }
}

