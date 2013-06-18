namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization.Formatters.Binary;

    internal class XomlComponentSerializationService : ComponentSerializationService
    {
        private IServiceProvider serviceProvider;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal XomlComponentSerializationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override SerializationStore CreateStore()
        {
            return new WorkflowMarkupSerializationStore(this.serviceProvider);
        }

        public override ICollection Deserialize(SerializationStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            WorkflowMarkupSerializationStore store2 = store as WorkflowMarkupSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_UnknownSerializationStore"));
            }
            return store2.Deserialize(this.serviceProvider);
        }

        public override ICollection Deserialize(SerializationStore store, IContainer container)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            WorkflowMarkupSerializationStore store2 = store as WorkflowMarkupSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_UnknownSerializationStore"));
            }
            return store2.Deserialize(this.serviceProvider, container);
        }

        public override void DeserializeTo(SerializationStore store, IContainer container, bool validateRecycledTypes, bool applyDefaults)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            WorkflowMarkupSerializationStore store2 = store as WorkflowMarkupSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_UnknownSerializationStore"));
            }
            store2.DeserializeTo(this.serviceProvider, container);
        }

        internal static PropertyInfo GetProperty(Type type, string name, BindingFlags bindingFlags)
        {
            PropertyInfo property = null;
            try
            {
                property = type.GetProperty(name, bindingFlags);
            }
            catch (AmbiguousMatchException)
            {
                foreach (PropertyInfo info2 in type.GetProperties(bindingFlags))
                {
                    if (info2.Name.Equals(name, StringComparison.Ordinal))
                    {
                        return info2;
                    }
                }
            }
            return property;
        }

        public override SerializationStore LoadStore(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            BinaryFormatter formatter = new BinaryFormatter();
            return (WorkflowMarkupSerializationStore) formatter.Deserialize(stream);
        }

        public override void Serialize(SerializationStore store, object value)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            WorkflowMarkupSerializationStore store2 = store as WorkflowMarkupSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_UnknownSerializationStore"));
            }
            store2.AddObject(value);
        }

        public override void SerializeAbsolute(SerializationStore store, object value)
        {
            this.Serialize(store, value);
        }

        public override void SerializeMember(SerializationStore store, object owningObject, MemberDescriptor member)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (owningObject == null)
            {
                throw new ArgumentNullException("owningObject");
            }
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            WorkflowMarkupSerializationStore store2 = store as WorkflowMarkupSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_UnknownSerializationStore"));
            }
            store2.AddMember(owningObject, member);
        }

        public override void SerializeMemberAbsolute(SerializationStore store, object owningObject, MemberDescriptor member)
        {
            this.SerializeMember(store, owningObject, member);
        }
    }
}

