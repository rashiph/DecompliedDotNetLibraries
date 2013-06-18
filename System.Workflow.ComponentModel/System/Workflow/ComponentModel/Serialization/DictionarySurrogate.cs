namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal sealed class DictionarySurrogate : ISerializationSurrogate
    {
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (!obj.GetType().IsGenericType || (obj.GetType().GetGenericTypeDefinition() != typeof(Dictionary<,>)))
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "obj");
            }
            Type[] genericArguments = obj.GetType().GetGenericArguments();
            if (genericArguments.Length != 2)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "obj");
            }
            PropertyInfo property = obj.GetType().GetProperty("Keys");
            if (property == null)
            {
                throw new NullReferenceException("keysProperty");
            }
            ArrayList list = new ArrayList(property.GetValue(obj, null) as ICollection);
            PropertyInfo info3 = obj.GetType().GetProperty("Values");
            if (info3 == null)
            {
                throw new NullReferenceException("valuesProperty");
            }
            ArrayList list2 = new ArrayList(info3.GetValue(obj, null) as ICollection);
            if (list.Count == 1)
            {
                info.AddValue("key", list[0]);
                info.AddValue("value", list2[0]);
            }
            else if (list.Count > 1)
            {
                info.AddValue("keys", list.ToArray());
                info.AddValue("values", list2.ToArray());
            }
            info.AddValue("keyType", genericArguments[0]);
            info.AddValue("valueType", genericArguments[1]);
            info.SetType(typeof(DictionaryRef));
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        [Serializable]
        private sealed class DictionaryRef : IObjectReference, IDeserializationCallback
        {
            [NonSerialized]
            private object dictionary;
            [OptionalField]
            private object key;
            [OptionalField]
            private IList keys;
            private Type keyType;
            [OptionalField]
            private object value;
            [OptionalField]
            private IList values;
            private Type valueType;

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                if (this.dictionary != null)
                {
                    MethodInfo method = this.dictionary.GetType().GetMethod("Add");
                    if (method == null)
                    {
                        throw new NullReferenceException("addMethod");
                    }
                    object[] parameters = new object[2];
                    if (this.keys != null)
                    {
                        for (int i = 0; i < this.keys.Count; i++)
                        {
                            parameters[0] = this.keys[i];
                            parameters[1] = this.values[i];
                            method.Invoke(this.dictionary, parameters);
                        }
                    }
                    else if (this.key != null)
                    {
                        parameters[0] = this.key;
                        parameters[1] = this.value;
                        method.Invoke(this.dictionary, parameters);
                    }
                }
                this.dictionary = null;
            }

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.dictionary == null)
                {
                    Type type = typeof(Dictionary<int, int>).GetGenericTypeDefinition().MakeGenericType(new Type[] { this.keyType, this.valueType });
                    this.dictionary = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                }
                return this.dictionary;
            }
        }
    }
}

