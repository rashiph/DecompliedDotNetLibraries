namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Threading;
    using System.Xml.Linq;

    public abstract class InstancePersistenceEvent<T> : InstancePersistenceEvent where T: InstancePersistenceEvent<T>, new()
    {
        private static T instance;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected InstancePersistenceEvent(XName name) : base(name)
        {
        }

        public static T Value
        {
            get
            {
                if (InstancePersistenceEvent<T>.instance == null)
                {
                    Interlocked.CompareExchange<T>(ref InstancePersistenceEvent<T>.instance, Activator.CreateInstance<T>(), default(T));
                }
                return InstancePersistenceEvent<T>.instance;
            }
        }
    }
}

