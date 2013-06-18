namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Xml.Linq;

    [Serializable]
    internal class InstanceAlreadyLockedToOwnerException : InstancePersistenceCommandException
    {
        public InstanceAlreadyLockedToOwnerException(XName commandName, Guid instanceId, long instanceVersion) : base(commandName, instanceId)
        {
            this.InstanceVersion = instanceVersion;
        }

        public long InstanceVersion { get; private set; }
    }
}

