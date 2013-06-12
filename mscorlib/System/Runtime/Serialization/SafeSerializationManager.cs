namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal sealed class SafeSerializationManager : IObjectReference, ISerializable
    {
        private object m_realObject;
        private RuntimeType m_realType;
        private SerializationInfo m_savedSerializationInfo;
        private IList<object> m_serializedStates;
        private const string RealTypeSerializationName = "CLR_SafeSerializationManager_RealType";

        internal event EventHandler<SafeSerializationEventArgs> SerializeObjectState;

        internal SafeSerializationManager()
        {
        }

        [SecurityCritical]
        private SafeSerializationManager(SerializationInfo info, StreamingContext context)
        {
            RuntimeType valueNoThrow = info.GetValueNoThrow("CLR_SafeSerializationManager_RealType", typeof(RuntimeType)) as RuntimeType;
            if (valueNoThrow == null)
            {
                this.m_serializedStates = info.GetValue("m_serializedStates", typeof(List<object>)) as List<object>;
            }
            else
            {
                this.m_realType = valueNoThrow;
                this.m_savedSerializationInfo = info;
            }
        }

        internal void CompleteDeserialization(object deserializedObject)
        {
            if (this.m_serializedStates != null)
            {
                foreach (ISafeSerializationData data in this.m_serializedStates)
                {
                    data.CompleteDeserialization(deserializedObject);
                }
            }
        }

        [SecurityCritical]
        internal void CompleteSerialization(object serializedObject, SerializationInfo info, StreamingContext context)
        {
            this.m_serializedStates = null;
            EventHandler<SafeSerializationEventArgs> serializeObjectState = this.SerializeObjectState;
            if (serializeObjectState != null)
            {
                SafeSerializationEventArgs e = new SafeSerializationEventArgs(context);
                serializeObjectState(serializedObject, e);
                this.m_serializedStates = e.SerializedStates;
                info.AddValue("CLR_SafeSerializationManager_RealType", serializedObject.GetType(), typeof(RuntimeType));
                info.SetType(typeof(SafeSerializationManager));
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (this.m_realObject != null)
            {
                SerializationEventsCache.GetSerializationEventsForType(this.m_realObject.GetType()).InvokeOnDeserialized(this.m_realObject, context);
                this.m_realObject = null;
            }
        }

        [SecurityCritical]
        object IObjectReference.GetRealObject(StreamingContext context)
        {
            if (this.m_realObject != null)
            {
                return this.m_realObject;
            }
            if (this.m_realType == null)
            {
                return this;
            }
            Stack stack = new Stack();
            RuntimeType realType = this.m_realType;
            do
            {
                stack.Push(realType);
                realType = realType.BaseType as RuntimeType;
            }
            while (realType != typeof(object));
            RuntimeConstructorInfo ctorInfo = null;
            RuntimeType t = null;
            do
            {
                t = realType;
                realType = stack.Pop() as RuntimeType;
            }
            while (ObjectManager.TryGetConstructor(realType, out ctorInfo) && ctorInfo.IsSecurityCritical);
            ctorInfo = ObjectManager.GetConstructor(t);
            object uninitializedObject = FormatterServices.GetUninitializedObject(this.m_realType);
            ctorInfo.SerializationInvoke(uninitializedObject, this.m_savedSerializationInfo, context);
            this.m_savedSerializationInfo = null;
            this.m_realType = null;
            this.m_realObject = uninitializedObject;
            return uninitializedObject;
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("m_serializedStates", this.m_serializedStates, typeof(List<IDeserializationCallback>));
        }

        internal bool IsActive
        {
            get
            {
                return (this.SerializeObjectState != null);
            }
        }
    }
}

