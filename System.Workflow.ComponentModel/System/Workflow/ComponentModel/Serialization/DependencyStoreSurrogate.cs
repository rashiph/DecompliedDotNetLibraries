namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel;

    internal sealed class DependencyStoreSurrogate : ISerializationSurrogate
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DependencyStoreSurrogate()
        {
        }

        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            IDictionary<DependencyProperty, object> dictionary = obj as IDictionary<DependencyProperty, object>;
            if (dictionary == null)
            {
                throw new ArgumentException("obj");
            }
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            foreach (KeyValuePair<DependencyProperty, object> pair in dictionary)
            {
                if (!pair.Key.DefaultMetadata.IsNonSerialized)
                {
                    if (pair.Key.IsKnown)
                    {
                        list.Add(pair.Key.KnownIndex);
                    }
                    else
                    {
                        list.Add(pair.Key);
                    }
                    list2.Add(pair.Value);
                }
            }
            info.AddValue("keys", list.ToArray());
            info.AddValue("values", list2.ToArray());
            info.SetType(typeof(DependencyStoreRef));
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        [Serializable]
        private sealed class DependencyStoreRef : IObjectReference, IDeserializationCallback
        {
            private IList keys;
            [NonSerialized]
            private IDictionary<DependencyProperty, object> store;
            private IList values;

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                if (this.store != null)
                {
                    for (int i = 0; i < this.keys.Count; i++)
                    {
                        DependencyProperty key = this.keys[i] as DependencyProperty;
                        if (key == null)
                        {
                            key = DependencyProperty.FromKnown((byte) this.keys[i]);
                        }
                        this.store.Add(key, this.values[i]);
                    }
                }
                this.store = null;
            }

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.store == null)
                {
                    this.store = new Dictionary<DependencyProperty, object>();
                }
                return this.store;
            }
        }
    }
}

