namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    public abstract class InstancePersistenceCommand
    {
        protected InstancePersistenceCommand(XName name)
        {
            if (name == null)
            {
                throw Fx.Exception.ArgumentNull("name");
            }
            this.Name = name;
        }

        internal virtual IEnumerable<InstancePersistenceCommand> Reduce(InstanceView view)
        {
            return null;
        }

        protected internal virtual void Validate(InstanceView view)
        {
        }

        protected internal virtual bool AutomaticallyAcquiringLock
        {
            get
            {
                return false;
            }
        }

        protected internal virtual bool IsTransactionEnlistmentOptional
        {
            get
            {
                return false;
            }
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

