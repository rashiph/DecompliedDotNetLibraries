namespace System.Web.Util
{
    using System;
    using System.Reflection;
    using System.Security.Permissions;

    internal class ArglessEventHandlerProxy
    {
        private MethodInfo _arglessMethod;
        private object _target;

        internal ArglessEventHandlerProxy(object target, MethodInfo arglessMethod)
        {
            this._target = target;
            this._arglessMethod = arglessMethod;
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.RestrictedMemberAccess)]
        internal void Callback(object sender, EventArgs e)
        {
            this._arglessMethod.Invoke(this._target, new object[0]);
        }

        internal EventHandler Handler
        {
            get
            {
                return new EventHandler(this.Callback);
            }
        }
    }
}

