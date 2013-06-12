namespace System.Runtime.InteropServices
{
    using System;

    [Obsolete("This attribute has been deprecated.  Application Domains no longer respect Activation Context boundaries in IDispatch calls.", false), AttributeUsage(AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class SetWin32ContextInIDispatchAttribute : Attribute
    {
    }
}

