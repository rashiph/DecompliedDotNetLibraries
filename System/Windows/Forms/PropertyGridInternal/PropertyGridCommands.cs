namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.ComponentModel.Design;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class PropertyGridCommands
    {
        public static readonly CommandID Commands = new CommandID(wfcMenuCommand, 0x3010);
        public static readonly CommandID Description = new CommandID(wfcMenuCommand, 0x3001);
        public static readonly CommandID Hide = new CommandID(wfcMenuCommand, 0x3002);
        public static readonly CommandID Reset = new CommandID(wfcMenuCommand, 0x3000);
        protected static readonly Guid wfcMenuCommand = new Guid("{5a51cf82-7619-4a5d-b054-47f438425aa7}");
        protected static readonly Guid wfcMenuGroup = new Guid("{a72bd644-1979-4cbc-a620-ea4112198a66}");
    }
}

