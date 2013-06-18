namespace System.Web.Util
{
    using System;
    using System.Runtime;

    internal sealed class CalliHelper
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal CalliHelper()
        {
        }

        internal static void ArglessFunctionCaller(IntPtr fp, object o)
        {
            *?();
        }

        internal static void EventArgFunctionCaller(IntPtr fp, object o, object t, EventArgs e)
        {
            *?(t, e);
        }
    }
}

