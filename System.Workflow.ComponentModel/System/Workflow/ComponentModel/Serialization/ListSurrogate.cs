namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;

    internal sealed class ListSurrogate : ISerializationSurrogate
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ListSurrogate()
        {
        }

        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (!obj.GetType().IsGenericType || (obj.GetType().GetGenericTypeDefinition() != typeof(List<>)))
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "obj");
            }
            Type[] genericArguments = obj.GetType().GetGenericArguments();
            if (genericArguments.Length != 1)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "obj");
            }
            ArrayList list = new ArrayList(obj as IList);
            if (list.Count == 1)
            {
                info.AddValue("item", list[0]);
            }
            else
            {
                info.AddValue("items", list.ToArray());
            }
            info.AddValue("itemType", genericArguments[0]);
            info.SetType(typeof(ListRef));
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        [Serializable]
        private sealed class ListRef : IObjectReference, IDeserializationCallback
        {
            [OptionalField]
            private object item;
            [OptionalField]
            private IList items;
            private Type itemType;
            [NonSerialized]
            private object list;

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                if (this.list != null)
                {
                    MethodInfo method = this.list.GetType().GetMethod("Add");
                    if (method == null)
                    {
                        throw new NullReferenceException("addMethod");
                    }
                    if (this.items != null)
                    {
                        for (int i = 0; i < this.items.Count; i++)
                        {
                            method.Invoke(this.list, new object[] { this.items[i] });
                        }
                    }
                    else
                    {
                        method.Invoke(this.list, new object[] { this.item });
                    }
                    this.list = null;
                }
            }

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.list == null)
                {
                    Type type = typeof(List<int>).GetGenericTypeDefinition().MakeGenericType(new Type[] { this.itemType });
                    this.list = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                }
                return this.list;
            }
        }
    }
}

