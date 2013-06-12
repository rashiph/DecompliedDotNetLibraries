namespace System
{
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Parameter, Inherited=true, AllowMultiple=false), ComVisible(true)]
    public sealed class ParamArrayAttribute : Attribute
    {
    }
}

