namespace System.Diagnostics.Eventing
{
    using System;
    using System.Runtime.CompilerServices;

    [FriendAccessAllowed, AttributeUsage(AttributeTargets.Method)]
    internal sealed class EventAttribute : Attribute
    {
        public EventAttribute(int eventId)
        {
            this.EventId = eventId;
        }

        public bool CaptureStack { get; set; }

        public EventChannel Channel { get; set; }

        public int EventId { get; internal set; }

        public EventKeywords Keywords { get; set; }

        public EventLevel Level { get; set; }

        public string Message { get; set; }

        public string MessageResourceId { get; set; }

        public string Name { get; set; }

        public EventOpcode Opcode { get; set; }

        public EventTask Task { get; set; }

        public byte Version { get; set; }
    }
}

