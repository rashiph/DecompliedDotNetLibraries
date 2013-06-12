namespace System.Diagnostics.Eventing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [StructLayout(LayoutKind.Explicit, Size=0x10), FriendAccessAllowed, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal struct EventDescriptorInternal
    {
        [FieldOffset(3)]
        private byte m_channel;
        [FieldOffset(0)]
        private ushort m_id;
        [FieldOffset(8)]
        private long m_keywords;
        [FieldOffset(4)]
        private byte m_level;
        [FieldOffset(5)]
        private byte m_opcode;
        [FieldOffset(6)]
        private ushort m_task;
        [FieldOffset(2)]
        private byte m_version;

        public EventDescriptorInternal(int id, byte version, byte channel, byte level, byte opcode, int task, long keywords)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException("id", SRETW.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (id > 0xffff)
            {
                throw new ArgumentOutOfRangeException("id", SRETW.GetString("ArgumentOutOfRange_NeedValidId", new object[] { 1, (ushort) 0xffff }));
            }
            this.m_id = (ushort) id;
            this.m_version = version;
            this.m_channel = channel;
            this.m_level = level;
            this.m_opcode = opcode;
            this.m_keywords = keywords;
            if (task < 0)
            {
                throw new ArgumentOutOfRangeException("task", SRETW.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (task > 0xffff)
            {
                throw new ArgumentOutOfRangeException("task", SRETW.GetString("ArgumentOutOfRange_NeedValidId", new object[] { 1, (ushort) 0xffff }));
            }
            this.m_task = (ushort) task;
        }

        public bool Equals(EventDescriptorInternal other)
        {
            return ((((this.m_id == other.m_id) && (this.m_version == other.m_version)) && ((this.m_channel == other.m_channel) && (this.m_level == other.m_level))) && (((this.m_opcode == other.m_opcode) && (this.m_task == other.m_task)) && (this.m_keywords == other.m_keywords)));
        }

        public override bool Equals(object obj)
        {
            return ((obj is EventDescriptorInternal) && this.Equals((EventDescriptorInternal) obj));
        }

        public override int GetHashCode()
        {
            return ((((((this.m_id ^ this.m_version) ^ this.m_channel) ^ this.m_level) ^ this.m_opcode) ^ this.m_task) ^ ((int) this.m_keywords));
        }

        public static bool operator ==(EventDescriptorInternal event1, EventDescriptorInternal event2)
        {
            return event1.Equals(event2);
        }

        public static bool operator !=(EventDescriptorInternal event1, EventDescriptorInternal event2)
        {
            return !event1.Equals(event2);
        }

        public byte Channel
        {
            get
            {
                return this.m_channel;
            }
        }

        public int EventId
        {
            get
            {
                return this.m_id;
            }
        }

        public long Keywords
        {
            get
            {
                return this.m_keywords;
            }
        }

        public byte Level
        {
            get
            {
                return this.m_level;
            }
        }

        public byte Opcode
        {
            get
            {
                return this.m_opcode;
            }
        }

        public int Task
        {
            get
            {
                return this.m_task;
            }
        }

        public byte Version
        {
            get
            {
                return this.m_version;
            }
        }
    }
}

