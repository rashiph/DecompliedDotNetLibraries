namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Runtime.Serialization;

    internal sealed class QueueSurrogate : ISerializationSurrogate
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal QueueSurrogate()
        {
        }

        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            object[] objArray = ((Queue) obj).ToArray();
            if (objArray.Length == 1)
            {
                info.AddValue("item", objArray[0]);
            }
            else
            {
                info.AddValue("items", objArray);
            }
            info.SetType(typeof(QRef));
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        [Serializable]
        private sealed class QRef : IObjectReference, IDeserializationCallback
        {
            [OptionalField]
            private object item;
            [OptionalField]
            private IList items;
            [NonSerialized]
            private Queue queue;

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                if (this.queue != null)
                {
                    if (this.items != null)
                    {
                        for (int i = 0; i < this.items.Count; i++)
                        {
                            this.queue.Enqueue(this.items[i]);
                        }
                    }
                    else
                    {
                        this.queue.Enqueue(this.item);
                    }
                    this.queue = null;
                }
            }

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.queue == null)
                {
                    this.queue = new Queue();
                }
                return this.queue;
            }
        }
    }
}

