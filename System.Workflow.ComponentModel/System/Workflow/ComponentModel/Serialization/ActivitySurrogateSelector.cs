namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel;
    using System.Xml;

    public sealed class ActivitySurrogateSelector : SurrogateSelector
    {
        private ActivityExecutorSurrogate activityExecutorSurrogate = new ActivityExecutorSurrogate();
        private ActivitySurrogate activitySurrogate = new ActivitySurrogate();
        private static ActivitySurrogateSelector defaultActivitySurrogateSelector = new ActivitySurrogateSelector();
        private DependencyStoreSurrogate dependencyStoreSurrogate = new DependencyStoreSurrogate();
        private DictionarySurrogate dictionarySurrogate = new DictionarySurrogate();
        private XmlDocumentSurrogate domDocSurrogate = new XmlDocumentSurrogate();
        private GenericQueueSurrogate genericqueueSurrogate = new GenericQueueSurrogate();
        private ListSurrogate listSurrogate = new ListSurrogate();
        private ObjectSurrogate objectSurrogate = new ObjectSurrogate();
        private QueueSurrogate queueSurrogate = new QueueSurrogate();
        private SimpleTypesSurrogate simpleTypesSurrogate = new SimpleTypesSurrogate();
        private static Dictionary<Type, ISerializationSurrogate> surrogateCache = new Dictionary<Type, ISerializationSurrogate>();

        public override ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            bool flag;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            selector = this;
            ISerializationSurrogate activitySurrogate = null;
            lock (surrogateCache)
            {
                flag = surrogateCache.TryGetValue(type, out activitySurrogate);
            }
            if (flag)
            {
                if (activitySurrogate != null)
                {
                    return activitySurrogate;
                }
                return base.GetSurrogate(type, context, out selector);
            }
            if (typeof(Activity).IsAssignableFrom(type))
            {
                activitySurrogate = this.activitySurrogate;
            }
            else if (typeof(ActivityExecutor).IsAssignableFrom(type))
            {
                activitySurrogate = this.activityExecutorSurrogate;
            }
            else if (typeof(IDictionary<DependencyProperty, object>).IsAssignableFrom(type))
            {
                activitySurrogate = this.dependencyStoreSurrogate;
            }
            else if (typeof(XmlDocument).IsAssignableFrom(type))
            {
                activitySurrogate = this.domDocSurrogate;
            }
            else if (typeof(Queue) == type)
            {
                activitySurrogate = this.queueSurrogate;
            }
            else if (typeof(Guid) == type)
            {
                activitySurrogate = this.simpleTypesSurrogate;
            }
            else if (typeof(ActivityBind).IsAssignableFrom(type))
            {
                activitySurrogate = this.objectSurrogate;
            }
            else if (typeof(DependencyObject).IsAssignableFrom(type))
            {
                activitySurrogate = this.objectSurrogate;
            }
            lock (surrogateCache)
            {
                surrogateCache[type] = activitySurrogate;
            }
            if (activitySurrogate != null)
            {
                return activitySurrogate;
            }
            return base.GetSurrogate(type, context, out selector);
        }

        public static ActivitySurrogateSelector Default
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return defaultActivitySurrogateSelector;
            }
        }

        private sealed class ObjectSurrogate : ISerializationSurrogate
        {
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                info.AddValue("type", obj.GetType());
                string[] names = null;
                MemberInfo[] serializableMembers = FormatterServicesNoSerializableCheck.GetSerializableMembers(obj.GetType(), out names);
                object[] objectData = FormatterServices.GetObjectData(obj, serializableMembers);
                info.AddValue("memberDatas", objectData);
                info.SetType(typeof(ObjectSerializedRef));
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                return null;
            }

            [Serializable]
            private sealed class ObjectSerializedRef : IObjectReference, IDeserializationCallback
            {
                private object[] memberDatas;
                [NonSerialized]
                private object returnedObject;
                private Type type;

                void IDeserializationCallback.OnDeserialization(object sender)
                {
                    if (this.returnedObject != null)
                    {
                        string[] names = null;
                        MemberInfo[] serializableMembers = FormatterServicesNoSerializableCheck.GetSerializableMembers(this.type, out names);
                        FormatterServices.PopulateObjectMembers(this.returnedObject, serializableMembers, this.memberDatas);
                        this.returnedObject = null;
                    }
                }

                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    if (this.returnedObject == null)
                    {
                        this.returnedObject = FormatterServices.GetUninitializedObject(this.type);
                    }
                    return this.returnedObject;
                }
            }
        }
    }
}

