namespace System.Runtime.Caching
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    internal class SRef
    {
        private object _sizedRef;
        private static Type s_type = Type.GetType("System.SizedReference", true, false);

        internal SRef(object target)
        {
            this._sizedRef = Activator.CreateInstance(s_type, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[] { target }, null);
        }

        [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal void Dispose()
        {
            s_type.InvokeMember("Dispose", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, this._sizedRef, null, CultureInfo.InvariantCulture);
        }

        internal long ApproximateSize
        {
            [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            get
            {
                return (long) s_type.InvokeMember("ApproximateSize", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, this._sizedRef, null, CultureInfo.InvariantCulture);
            }
        }
    }
}

