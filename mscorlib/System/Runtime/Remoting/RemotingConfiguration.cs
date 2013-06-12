namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true)]
    public static class RemotingConfiguration
    {
        private static bool s_ListeningForActivationRequests;

        [Obsolete("Use System.Runtime.Remoting.RemotingConfiguration.Configure(string fileName, bool ensureSecurity) instead.", false), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void Configure(string filename)
        {
            Configure(filename, false);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void Configure(string filename, bool ensureSecurity)
        {
            RemotingConfigHandler.DoConfiguration(filename, ensureSecurity);
            RemotingServices.InternalSetRemoteActivationConfigured();
        }

        public static bool CustomErrorsEnabled(bool isLocalRequest)
        {
            switch (CustomErrorsMode)
            {
                case CustomErrorsModes.On:
                    return true;

                case CustomErrorsModes.Off:
                    return false;

                case CustomErrorsModes.RemoteOnly:
                    return !isLocalRequest;
            }
            return true;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ActivatedClientTypeEntry[] GetRegisteredActivatedClientTypes()
        {
            return RemotingConfigHandler.GetRegisteredActivatedClientTypes();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ActivatedServiceTypeEntry[] GetRegisteredActivatedServiceTypes()
        {
            return RemotingConfigHandler.GetRegisteredActivatedServiceTypes();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static WellKnownClientTypeEntry[] GetRegisteredWellKnownClientTypes()
        {
            return RemotingConfigHandler.GetRegisteredWellKnownClientTypes();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static WellKnownServiceTypeEntry[] GetRegisteredWellKnownServiceTypes()
        {
            return RemotingConfigHandler.GetRegisteredWellKnownServiceTypes();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static bool IsActivationAllowed(Type svrType)
        {
            RuntimeType type = svrType as RuntimeType;
            if ((svrType != null) && (type == null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            return RemotingConfigHandler.IsActivationAllowed(type);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ActivatedClientTypeEntry IsRemotelyActivatedClientType(Type svrType)
        {
            if (svrType == null)
            {
                throw new ArgumentNullException("svrType");
            }
            RuntimeType type = svrType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            return RemotingConfigHandler.IsRemotelyActivatedClientType(type);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ActivatedClientTypeEntry IsRemotelyActivatedClientType(string typeName, string assemblyName)
        {
            return RemotingConfigHandler.IsRemotelyActivatedClientType(typeName, assemblyName);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static WellKnownClientTypeEntry IsWellKnownClientType(Type svrType)
        {
            if (svrType == null)
            {
                throw new ArgumentNullException("svrType");
            }
            RuntimeType type = svrType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            return RemotingConfigHandler.IsWellKnownClientType(type);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static WellKnownClientTypeEntry IsWellKnownClientType(string typeName, string assemblyName)
        {
            return RemotingConfigHandler.IsWellKnownClientType(typeName, assemblyName);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterActivatedClientType(ActivatedClientTypeEntry entry)
        {
            RemotingConfigHandler.RegisterActivatedClientType(entry);
            RemotingServices.InternalSetRemoteActivationConfigured();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterActivatedClientType(Type type, string appUrl)
        {
            ActivatedClientTypeEntry entry = new ActivatedClientTypeEntry(type, appUrl);
            RegisterActivatedClientType(entry);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterActivatedServiceType(ActivatedServiceTypeEntry entry)
        {
            RemotingConfigHandler.RegisterActivatedServiceType(entry);
            if (!s_ListeningForActivationRequests)
            {
                s_ListeningForActivationRequests = true;
                ActivationServices.StartListeningForRemoteRequests();
            }
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterActivatedServiceType(Type type)
        {
            ActivatedServiceTypeEntry entry = new ActivatedServiceTypeEntry(type);
            RegisterActivatedServiceType(entry);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterWellKnownClientType(WellKnownClientTypeEntry entry)
        {
            RemotingConfigHandler.RegisterWellKnownClientType(entry);
            RemotingServices.InternalSetRemoteActivationConfigured();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterWellKnownClientType(Type type, string objectUrl)
        {
            WellKnownClientTypeEntry entry = new WellKnownClientTypeEntry(type, objectUrl);
            RegisterWellKnownClientType(entry);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterWellKnownServiceType(WellKnownServiceTypeEntry entry)
        {
            RemotingConfigHandler.RegisterWellKnownServiceType(entry);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterWellKnownServiceType(Type type, string objectUri, WellKnownObjectMode mode)
        {
            WellKnownServiceTypeEntry entry = new WellKnownServiceTypeEntry(type, objectUri, mode);
            RegisterWellKnownServiceType(entry);
        }

        public static string ApplicationId
        {
            [SecurityCritical]
            get
            {
                return Identity.AppDomainUniqueId;
            }
        }

        public static string ApplicationName
        {
            get
            {
                if (!RemotingConfigHandler.HasApplicationNameBeenSet())
                {
                    return null;
                }
                return RemotingConfigHandler.ApplicationName;
            }
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                RemotingConfigHandler.ApplicationName = value;
            }
        }

        public static CustomErrorsModes CustomErrorsMode
        {
            get
            {
                return RemotingConfigHandler.CustomErrorsMode;
            }
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                RemotingConfigHandler.CustomErrorsMode = value;
            }
        }

        public static string ProcessId
        {
            [SecurityCritical]
            get
            {
                return Identity.ProcessGuid;
            }
        }
    }
}

