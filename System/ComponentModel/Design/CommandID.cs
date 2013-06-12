namespace System.ComponentModel.Design
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class CommandID
    {
        private readonly int commandID;
        private readonly System.Guid menuGroup;

        public CommandID(System.Guid menuGroup, int commandID)
        {
            this.menuGroup = menuGroup;
            this.commandID = commandID;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CommandID))
            {
                return false;
            }
            CommandID did = (CommandID) obj;
            return (did.menuGroup.Equals(this.menuGroup) && (did.commandID == this.commandID));
        }

        public override int GetHashCode()
        {
            return ((this.menuGroup.GetHashCode() << 2) | this.commandID);
        }

        public override string ToString()
        {
            return (this.menuGroup.ToString() + " : " + this.commandID.ToString(CultureInfo.CurrentCulture));
        }

        public virtual System.Guid Guid
        {
            get
            {
                return this.menuGroup;
            }
        }

        public virtual int ID
        {
            get
            {
                return this.commandID;
            }
        }
    }
}

