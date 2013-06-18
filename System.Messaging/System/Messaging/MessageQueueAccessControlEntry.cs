namespace System.Messaging
{
    using System;

    public class MessageQueueAccessControlEntry : AccessControlEntry
    {
        public MessageQueueAccessControlEntry(Trustee trustee, System.Messaging.MessageQueueAccessRights rights) : base(trustee)
        {
            base.CustomAccessRights |= rights;
        }

        public MessageQueueAccessControlEntry(Trustee trustee, System.Messaging.MessageQueueAccessRights rights, AccessControlEntryType entryType) : base(trustee)
        {
            base.CustomAccessRights |= rights;
            base.EntryType = entryType;
        }

        public System.Messaging.MessageQueueAccessRights MessageQueueAccessRights
        {
            get
            {
                return (System.Messaging.MessageQueueAccessRights) base.CustomAccessRights;
            }
            set
            {
                base.CustomAccessRights = (int) value;
            }
        }
    }
}

