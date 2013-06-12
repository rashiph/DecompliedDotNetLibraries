namespace System.Diagnostics
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    [Serializable]
    public class PerformanceCounterPermissionEntry
    {
        private string categoryName;
        private string machineName;
        private PerformanceCounterPermissionAccess permissionAccess;

        internal PerformanceCounterPermissionEntry(ResourcePermissionBaseEntry baseEntry)
        {
            this.permissionAccess = (PerformanceCounterPermissionAccess) baseEntry.PermissionAccess;
            this.machineName = baseEntry.PermissionAccessPath[0];
            this.categoryName = baseEntry.PermissionAccessPath[1];
        }

        public PerformanceCounterPermissionEntry(PerformanceCounterPermissionAccess permissionAccess, string machineName, string categoryName)
        {
            if (categoryName == null)
            {
                throw new ArgumentNullException("categoryName");
            }
            if ((permissionAccess & ~PerformanceCounterPermissionAccess.Administer) != PerformanceCounterPermissionAccess.None)
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "permissionAccess", permissionAccess }));
            }
            if (machineName == null)
            {
                throw new ArgumentNullException("machineName");
            }
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "MachineName", machineName }));
            }
            this.permissionAccess = permissionAccess;
            this.machineName = machineName;
            this.categoryName = categoryName;
        }

        internal ResourcePermissionBaseEntry GetBaseEntry()
        {
            return new ResourcePermissionBaseEntry((int) this.PermissionAccess, new string[] { this.MachineName, this.CategoryName });
        }

        public string CategoryName
        {
            get
            {
                return this.categoryName;
            }
        }

        public string MachineName
        {
            get
            {
                return this.machineName;
            }
        }

        public PerformanceCounterPermissionAccess PermissionAccess
        {
            get
            {
                return this.permissionAccess;
            }
        }
    }
}

