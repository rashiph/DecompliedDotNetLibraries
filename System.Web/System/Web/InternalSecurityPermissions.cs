namespace System.Web
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    internal static class InternalSecurityPermissions
    {
        private static IStackWalk _appPathDiscovery;
        private static IStackWalk _controlPrincipal;
        private static IStackWalk _controlThread;
        private static IStackWalk _levelHigh;
        private static IStackWalk _levelLow;
        private static IStackWalk _levelMedium;
        private static IStackWalk _reflection;
        private static IStackWalk _unmanagedCode;
        private static IStackWalk _unrestricted;

        internal static IStackWalk FileReadAccess(string filename)
        {
            return new FileIOPermission(FileIOPermissionAccess.Read, filename);
        }

        internal static IStackWalk FileWriteAccess(string filename)
        {
            return new FileIOPermission(FileIOPermissionAccess.Append | FileIOPermissionAccess.Write, filename);
        }

        internal static IStackWalk PathDiscovery(string path)
        {
            return new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path);
        }

        internal static IStackWalk AppPathDiscovery
        {
            get
            {
                if (_appPathDiscovery == null)
                {
                    _appPathDiscovery = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, HttpRuntime.AppDomainAppPathInternal);
                }
                return _appPathDiscovery;
            }
        }

        internal static IStackWalk AspNetHostingPermissionLevelHigh
        {
            get
            {
                if (_levelHigh == null)
                {
                    _levelHigh = new AspNetHostingPermission(AspNetHostingPermissionLevel.High);
                }
                return _levelHigh;
            }
        }

        internal static IStackWalk AspNetHostingPermissionLevelLow
        {
            get
            {
                if (_levelLow == null)
                {
                    _levelLow = new AspNetHostingPermission(AspNetHostingPermissionLevel.Low);
                }
                return _levelLow;
            }
        }

        internal static IStackWalk AspNetHostingPermissionLevelMedium
        {
            get
            {
                if (_levelMedium == null)
                {
                    _levelMedium = new AspNetHostingPermission(AspNetHostingPermissionLevel.Medium);
                }
                return _levelMedium;
            }
        }

        internal static IStackWalk ControlPrincipal
        {
            get
            {
                if (_controlPrincipal == null)
                {
                    _controlPrincipal = new SecurityPermission(SecurityPermissionFlag.ControlPrincipal);
                }
                return _controlPrincipal;
            }
        }

        internal static IStackWalk ControlThread
        {
            get
            {
                if (_controlThread == null)
                {
                    _controlThread = new SecurityPermission(SecurityPermissionFlag.ControlThread);
                }
                return _controlThread;
            }
        }

        internal static IStackWalk Reflection
        {
            get
            {
                if (_reflection == null)
                {
                    _reflection = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
                }
                return _reflection;
            }
        }

        internal static IStackWalk UnmanagedCode
        {
            get
            {
                if (_unmanagedCode == null)
                {
                    _unmanagedCode = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                }
                return _unmanagedCode;
            }
        }

        internal static IStackWalk Unrestricted
        {
            get
            {
                if (_unrestricted == null)
                {
                    _unrestricted = new PermissionSet(PermissionState.Unrestricted);
                }
                return _unrestricted;
            }
        }
    }
}

