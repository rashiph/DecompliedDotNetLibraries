namespace System.Diagnostics
{
    using System;
    using System.ComponentModel;

    public class EventInstance
    {
        private int _categoryNumber;
        private EventLogEntryType _entryType;
        private long _instanceId;

        public EventInstance(long instanceId, int categoryId)
        {
            this._entryType = EventLogEntryType.Information;
            this.CategoryId = categoryId;
            this.InstanceId = instanceId;
        }

        public EventInstance(long instanceId, int categoryId, EventLogEntryType entryType) : this(instanceId, categoryId)
        {
            this.EntryType = entryType;
        }

        public int CategoryId
        {
            get
            {
                return this._categoryNumber;
            }
            set
            {
                if ((value > 0xffff) || (value < 0))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._categoryNumber = value;
            }
        }

        public EventLogEntryType EntryType
        {
            get
            {
                return this._entryType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(EventLogEntryType), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(EventLogEntryType));
                }
                this._entryType = value;
            }
        }

        public long InstanceId
        {
            get
            {
                return this._instanceId;
            }
            set
            {
                if ((value > 0xffffffffL) || (value < 0L))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._instanceId = value;
            }
        }
    }
}

