namespace System.Web.Util
{
    using System;
    using System.Security.Permissions;
    using System.Web.Security;

    internal static class SystemWebProxy
    {
        public static readonly IMembershipAdapter Membership = GetMembershipAdapter();

        private static IMembershipAdapter CreateSystemWebMembershipAdapter()
        {
            bool throwOnError = false;
            Type type = Type.GetType("System.Web.Security.MembershipAdapter, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", throwOnError);
            if (type != null)
            {
                return (IMembershipAdapter) DangerousCreateInstance(type);
            }
            return null;
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        private static object DangerousCreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        private static IMembershipAdapter GetMembershipAdapter()
        {
            IMembershipAdapter adapter = CreateSystemWebMembershipAdapter();
            if (adapter == null)
            {
                adapter = new DefaultMembershipAdapter();
            }
            return adapter;
        }
    }
}

