namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class SerializationEvents
    {
        private List<MethodInfo> m_OnDeserializedMethods;
        private List<MethodInfo> m_OnDeserializingMethods;
        private List<MethodInfo> m_OnSerializedMethods;
        private List<MethodInfo> m_OnSerializingMethods;

        internal SerializationEvents(Type t)
        {
            this.m_OnSerializingMethods = this.GetMethodsWithAttribute(typeof(OnSerializingAttribute), t);
            this.m_OnSerializedMethods = this.GetMethodsWithAttribute(typeof(OnSerializedAttribute), t);
            this.m_OnDeserializingMethods = this.GetMethodsWithAttribute(typeof(OnDeserializingAttribute), t);
            this.m_OnDeserializedMethods = this.GetMethodsWithAttribute(typeof(OnDeserializedAttribute), t);
        }

        internal SerializationEventHandler AddOnDeserialized(object obj, SerializationEventHandler handler)
        {
            if (this.m_OnDeserializedMethods != null)
            {
                foreach (MethodInfo info in this.m_OnDeserializedMethods)
                {
                    SerializationEventHandler b = (SerializationEventHandler) Delegate.InternalCreateDelegate(typeof(SerializationEventHandler), obj, info);
                    handler = (SerializationEventHandler) Delegate.Combine(handler, b);
                }
            }
            return handler;
        }

        internal SerializationEventHandler AddOnSerialized(object obj, SerializationEventHandler handler)
        {
            if (this.m_OnSerializedMethods != null)
            {
                foreach (MethodInfo info in this.m_OnSerializedMethods)
                {
                    SerializationEventHandler b = (SerializationEventHandler) Delegate.InternalCreateDelegate(typeof(SerializationEventHandler), obj, info);
                    handler = (SerializationEventHandler) Delegate.Combine(handler, b);
                }
            }
            return handler;
        }

        private List<MethodInfo> GetMethodsWithAttribute(Type attribute, Type t)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            for (Type type = t; (type != null) && (type != typeof(object)); type = type.BaseType)
            {
                RuntimeType type1 = (RuntimeType) type;
                foreach (MethodInfo info in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (info.IsDefined(attribute, false))
                    {
                        list.Add(info);
                    }
                }
            }
            list.Reverse();
            if (list.Count != 0)
            {
                return list;
            }
            return null;
        }

        internal void InvokeOnDeserialized(object obj, StreamingContext context)
        {
            if (this.m_OnDeserializedMethods != null)
            {
                SerializationEventHandler a = null;
                foreach (MethodInfo info in this.m_OnDeserializedMethods)
                {
                    SerializationEventHandler b = (SerializationEventHandler) Delegate.InternalCreateDelegate(typeof(SerializationEventHandler), obj, info);
                    a = (SerializationEventHandler) Delegate.Combine(a, b);
                }
                a(context);
            }
        }

        internal void InvokeOnDeserializing(object obj, StreamingContext context)
        {
            if (this.m_OnDeserializingMethods != null)
            {
                SerializationEventHandler a = null;
                foreach (MethodInfo info in this.m_OnDeserializingMethods)
                {
                    SerializationEventHandler b = (SerializationEventHandler) Delegate.InternalCreateDelegate(typeof(SerializationEventHandler), obj, info);
                    a = (SerializationEventHandler) Delegate.Combine(a, b);
                }
                a(context);
            }
        }

        internal void InvokeOnSerializing(object obj, StreamingContext context)
        {
            if (this.m_OnSerializingMethods != null)
            {
                SerializationEventHandler a = null;
                foreach (MethodInfo info in this.m_OnSerializingMethods)
                {
                    SerializationEventHandler b = (SerializationEventHandler) Delegate.InternalCreateDelegate(typeof(SerializationEventHandler), obj, info);
                    a = (SerializationEventHandler) Delegate.Combine(a, b);
                }
                a(context);
            }
        }

        internal bool HasOnSerializingEvents
        {
            get
            {
                if (this.m_OnSerializingMethods == null)
                {
                    return (this.m_OnSerializedMethods != null);
                }
                return true;
            }
        }
    }
}

