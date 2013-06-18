namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    public abstract class InstancePersistenceEvent : IEquatable<InstancePersistenceEvent>
    {
        internal InstancePersistenceEvent(XName name)
        {
            if (name == null)
            {
                throw Fx.Exception.ArgumentNull("name");
            }
            this.Name = name;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as InstancePersistenceEvent);
        }

        public bool Equals(InstancePersistenceEvent persistenceEvent)
        {
            return (!object.ReferenceEquals(persistenceEvent, null) && (persistenceEvent.Name == this.Name));
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public static bool operator ==(InstancePersistenceEvent left, InstancePersistenceEvent right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return true;
            }
            if (object.ReferenceEquals(left, null))
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(InstancePersistenceEvent left, InstancePersistenceEvent right)
        {
            return !(left == right);
        }

        public XName Name
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Name>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Name>k__BackingField = value;
            }
        }
    }
}

