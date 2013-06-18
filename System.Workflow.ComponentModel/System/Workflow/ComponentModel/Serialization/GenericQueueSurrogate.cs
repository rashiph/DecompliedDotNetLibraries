namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;

    internal sealed class GenericQueueSurrogate : ISerializationSurrogate
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal GenericQueueSurrogate()
        {
        }

        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (!obj.GetType().IsGenericType || (obj.GetType().GetGenericTypeDefinition() != typeof(Queue<>)))
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "obj");
            }
            Type[] genericArguments = obj.GetType().GetGenericArguments();
            if (genericArguments.Length != 1)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "obj");
            }
            ArrayList list = new ArrayList(obj as ICollection);
            if (list.Count == 1)
            {
                info.AddValue("item", list[0]);
            }
            else
            {
                info.AddValue("items", list.ToArray());
            }
            info.AddValue("itemType", genericArguments[0]);
            info.SetType(typeof(GenericQRef));
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        [Serializable]
        private sealed class GenericQRef : IObjectReference, IDeserializationCallback
        {
            [OptionalField]
            private object item;
            [OptionalField]
            private IList items;
            private Type itemType;
            [NonSerialized]
            private object queue;

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                if (this.queue != null)
                {
                    MethodInfo method = this.queue.GetType().GetMethod("Enqueue");
                    if (method == null)
                    {
                        throw new NullReferenceException("enqueueMethod");
                    }
                    if (this.items != null)
                    {
                        for (int i = 0; i < this.items.Count; i++)
                        {
                            method.Invoke(this.queue, new object[] { this.items[i] });
                        }
                    }
                    else
                    {
                        method.Invoke(this.queue, new object[] { this.item });
                    }
                    this.queue = null;
                }
            }

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.queue == null)
                {
                    Type type = typeof(Queue<int>).GetGenericTypeDefinition().MakeGenericType(new Type[] { this.itemType });
                    this.queue = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                }
                return this.queue;
            }
        }
    }
}

