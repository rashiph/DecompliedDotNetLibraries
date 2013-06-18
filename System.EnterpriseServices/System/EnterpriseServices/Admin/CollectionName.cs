namespace System.EnterpriseServices.Admin
{
    using System;
    using System.Runtime.CompilerServices;

    internal class CollectionName
    {
        private static string _apps;
        private static string _comps;
        private static volatile bool _initialized;
        private static string _interfaces;
        private static string _meths;
        private static string _roles;
        private static string _user;

        private static void Initialize()
        {
            if (!_initialized)
            {
                lock (typeof(CollectionName))
                {
                    if (!_initialized)
                    {
                        _apps = "Applications";
                        _comps = "Components";
                        _interfaces = "InterfacesForComponent";
                        _meths = "MethodsForInterface";
                        _roles = "Roles";
                        _user = "UsersInRole";
                        _initialized = true;
                    }
                }
            }
        }

        internal static string RolesFor(string target)
        {
            return ("RolesFor" + target);
        }

        internal static string Applications
        {
            get
            {
                Initialize();
                return _apps;
            }
        }

        internal static string Components
        {
            get
            {
                Initialize();
                return _comps;
            }
        }

        internal static string Interfaces
        {
            get
            {
                Initialize();
                return _interfaces;
            }
        }

        internal static string Methods
        {
            get
            {
                Initialize();
                return _meths;
            }
        }

        internal static string Roles
        {
            get
            {
                Initialize();
                return _roles;
            }
        }

        internal static string UsersInRole
        {
            get
            {
                Initialize();
                return _user;
            }
        }
    }
}

