namespace System.Web.Caching
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web;

    internal class SRef
    {
        private object _sizedRef;
        private static Type s_type = Type.GetType("System.SizedReference", true, false);

        internal SRef(object target)
        {
            this._sizedRef = HttpRuntime.CreateNonPublicInstance(s_type, new object[] { target });
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal void Dispose()
        {
            s_type.InvokeMember("Dispose", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, this._sizedRef, null, CultureInfo.InvariantCulture);
        }

        internal long ApproximateSize
        {
            [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            get
            {
                return (long) s_type.InvokeMember("ApproximateSize", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, this._sizedRef, null, CultureInfo.InvariantCulture);
            }
        }
    }
}

