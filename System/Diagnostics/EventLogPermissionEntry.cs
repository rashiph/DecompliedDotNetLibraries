namespace System.Diagnostics
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    [Serializable]
    public class EventLogPermissionEntry
    {
        private string machineName;
        private EventLogPermissionAccess permissionAccess;

        internal EventLogPermissionEntry(ResourcePermissionBaseEntry baseEntry)
        {
            this.permissionAccess = (EventLogPermissionAccess) baseEntry.PermissionAccess;
            this.machineName = baseEntry.PermissionAccessPath[0];
        }

        public EventLogPermissionEntry(EventLogPermissionAccess permissionAccess, string machineName)
        {
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "MachineName", machineName }));
            }
            this.permissionAccess = permissionAccess;
            this.machineName = machineName;
        }

        internal ResourcePermissionBaseEntry GetBaseEntry()
        {
            return new ResourcePermissionBaseEntry((int) this.PermissionAccess, new string[] { this.MachineName });
        }

        public string MachineName
        {
            get
            {
                return this.machineName;
            }
        }

        public EventLogPermissionAccess PermissionAccess
        {
            get
            {
                return this.permissionAccess;
            }
        }
    }
}

