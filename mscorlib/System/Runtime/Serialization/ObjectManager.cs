namespace System.Runtime.Serialization
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Cache;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Text;

    [ComVisible(true)]
    public class ObjectManager
    {
        private const int ArrayMask = 0xfff;
        private const int DefaultInitialSize = 0x10;
        internal StreamingContext m_context;
        internal long m_fixupCount;
        private bool m_isCrossAppDomain;
        internal ObjectHolder[] m_objects;
        private DeserializationEventHandler m_onDeserializationHandler;
        private SerializationEventHandler m_onDeserializedHandler;
        internal ISurrogateSelector m_selector;
        internal ObjectHolderList m_specialFixupObjects;
        internal object m_topObject;
        private const int MaxArraySize = 0x1000;
        private const int MaxReferenceDepth = 100;
        private static RuntimeType[] SIConstructorTypes = new RuntimeType[] { ((RuntimeType) typeof(SerializationInfo)), ((RuntimeType) typeof(StreamingContext)) };
        private static RuntimeType[] SIWindowsIdentityConstructorTypes = new RuntimeType[] { ((RuntimeType) typeof(SerializationInfo)) };
        private static RuntimeType TypeOfWindowsIdentity = ((RuntimeType) typeof(WindowsIdentity));

        [SecuritySafeCritical]
        public ObjectManager(ISurrogateSelector selector, StreamingContext context) : this(selector, context, true, false)
        {
        }

        [SecurityCritical]
        internal ObjectManager(ISurrogateSelector selector, StreamingContext context, bool checkSecurity, bool isCrossAppDomain)
        {
            if (checkSecurity)
            {
                CodeAccessPermission.Demand(PermissionType.SecuritySerialization);
            }
            this.m_objects = new ObjectHolder[0x10];
            this.m_selector = selector;
            this.m_context = context;
            this.m_isCrossAppDomain = isCrossAppDomain;
        }

        private void AddObjectHolder(ObjectHolder holder)
        {
            if ((holder.m_id >= this.m_objects.Length) && (this.m_objects.Length != 0x1000))
            {
                int num = 0x1000;
                if (holder.m_id < 0x800L)
                {
                    num = this.m_objects.Length * 2;
                    while ((num <= holder.m_id) && (num < 0x1000))
                    {
                        num *= 2;
                    }
                    if (num > 0x1000)
                    {
                        num = 0x1000;
                    }
                }
                ObjectHolder[] destinationArray = new ObjectHolder[num];
                Array.Copy(this.m_objects, destinationArray, this.m_objects.Length);
                this.m_objects = destinationArray;
            }
            int index = (int) (holder.m_id & 0xfffL);
            ObjectHolder holder2 = this.m_objects[index];
            holder.m_next = holder2;
            this.m_objects[index] = holder;
        }

        internal virtual void AddOnDeserialization(DeserializationEventHandler handler)
        {
            this.m_onDeserializationHandler = (DeserializationEventHandler) Delegate.Combine(this.m_onDeserializationHandler, handler);
        }

        internal virtual void AddOnDeserialized(object obj)
        {
            this.m_onDeserializedHandler = SerializationEventsCache.GetSerializationEventsForType(obj.GetType()).AddOnDeserialized(obj, this.m_onDeserializedHandler);
        }

        [SecurityCritical]
        private bool CanCallGetType(object obj)
        {
            if (RemotingServices.IsTransparentProxy(obj))
            {
                return false;
            }
            return true;
        }

        [SecurityCritical]
        internal void CompleteISerializableObject(object obj, SerializationInfo info, StreamingContext context)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (!(obj is ISerializable))
            {
                throw new ArgumentException(Environment.GetResourceString("Serialization_NotISer"));
            }
            RuntimeConstructorInfo constructor = null;
            RuntimeType t = (RuntimeType) obj.GetType();
            try
            {
                if ((t == TypeOfWindowsIdentity) && this.m_isCrossAppDomain)
                {
                    constructor = GetConstructor(t, SIWindowsIdentityConstructorTypes);
                }
                else
                {
                    constructor = GetConstructor(t);
                }
            }
            catch (Exception exception)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_ConstructorNotFound", new object[] { t }), exception);
            }
            constructor.SerializationInvoke(obj, info, context);
        }

        [SecurityCritical]
        internal void CompleteObject(ObjectHolder holder, bool bObjectFullyComplete)
        {
            FixupHolderList missingElements = holder.m_missingElements;
            object member = null;
            ObjectHolder holder3 = null;
            int num = 0;
            if (holder.ObjectValue == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_MissingObject", new object[] { holder.m_id }));
            }
            if (missingElements != null)
            {
                if (holder.HasSurrogate || holder.HasISerializable)
                {
                    SerializationInfo serInfo = holder.m_serInfo;
                    if (serInfo == null)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFixupDiscovered"));
                    }
                    if (missingElements != null)
                    {
                        for (int i = 0; i < missingElements.m_count; i++)
                        {
                            if ((missingElements.m_values[i] != null) && this.GetCompletionInfo(missingElements.m_values[i], out holder3, out member, bObjectFullyComplete))
                            {
                                object objectValue = holder3.ObjectValue;
                                if (this.CanCallGetType(objectValue))
                                {
                                    serInfo.UpdateValue((string) member, objectValue, objectValue.GetType());
                                }
                                else
                                {
                                    serInfo.UpdateValue((string) member, objectValue, typeof(MarshalByRefObject));
                                }
                                num++;
                                missingElements.m_values[i] = null;
                                if (!bObjectFullyComplete)
                                {
                                    holder.DecrementFixupsRemaining(this);
                                    holder3.RemoveDependency(holder.m_id);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < missingElements.m_count; j++)
                    {
                        MemberInfo info2;
                        FixupHolder fixup = missingElements.m_values[j];
                        if ((fixup == null) || !this.GetCompletionInfo(fixup, out holder3, out member, bObjectFullyComplete))
                        {
                            continue;
                        }
                        if (holder3.TypeLoadExceptionReachable)
                        {
                            holder.TypeLoadException = holder3.TypeLoadException;
                            if (holder.Reachable)
                            {
                                throw new SerializationException(Environment.GetResourceString("Serialization_TypeLoadFailure", new object[] { holder.TypeLoadException.TypeName }));
                            }
                        }
                        if (holder.Reachable)
                        {
                            holder3.Reachable = true;
                        }
                        switch (fixup.m_fixupType)
                        {
                            case 1:
                                if (holder.RequiresValueTypeFixup)
                                {
                                    throw new SerializationException(Environment.GetResourceString("Serialization_ValueTypeFixup"));
                                }
                                break;

                            case 2:
                                info2 = (MemberInfo) member;
                                if (info2.MemberType != MemberTypes.Field)
                                {
                                    throw new SerializationException(Environment.GetResourceString("Serialization_UnableToFixup"));
                                }
                                if (!holder.RequiresValueTypeFixup || !holder.ValueTypeFixupPerformed)
                                {
                                    goto Label_0242;
                                }
                                if (!this.DoValueTypeFixup((FieldInfo) info2, holder, holder3.ObjectValue))
                                {
                                    throw new SerializationException(Environment.GetResourceString("Serialization_PartialValueTypeFixup"));
                                }
                                goto Label_0256;

                            default:
                                throw new SerializationException(Environment.GetResourceString("Serialization_UnableToFixup"));
                        }
                        ((Array) holder.ObjectValue).SetValue(holder3.ObjectValue, (int[]) member);
                        goto Label_0289;
                    Label_0242:
                        FormatterServices.SerializationSetValue(info2, holder.ObjectValue, holder3.ObjectValue);
                    Label_0256:
                        if (holder3.RequiresValueTypeFixup)
                        {
                            holder3.ValueTypeFixupPerformed = true;
                        }
                    Label_0289:
                        num++;
                        missingElements.m_values[j] = null;
                        if (!bObjectFullyComplete)
                        {
                            holder.DecrementFixupsRemaining(this);
                            holder3.RemoveDependency(holder.m_id);
                        }
                    }
                }
                this.m_fixupCount -= num;
                if (missingElements.m_count == num)
                {
                    holder.m_missingElements = null;
                }
            }
        }

        [SecuritySafeCritical]
        public virtual void DoFixups()
        {
            ObjectHolder current;
            int num = -1;
            while (num != 0)
            {
                num = 0;
                ObjectHolderListEnumerator fixupEnumerator = this.SpecialFixupObjects.GetFixupEnumerator();
                while (fixupEnumerator.MoveNext())
                {
                    current = fixupEnumerator.Current;
                    if (current.ObjectValue == null)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_ObjectNotSupplied", new object[] { current.m_id }));
                    }
                    if (current.TotalDependentObjects == 0)
                    {
                        if (current.RequiresSerInfoFixup)
                        {
                            this.FixupSpecialObject(current);
                            num++;
                        }
                        else if (!current.IsIncompleteObjectReference)
                        {
                            this.CompleteObject(current, true);
                        }
                        if (current.IsIncompleteObjectReference && this.ResolveObjectReference(current))
                        {
                            num++;
                        }
                    }
                }
            }
            if (this.m_fixupCount == 0L)
            {
                if (this.TopObject is TypeLoadExceptionHolder)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_TypeLoadFailure", new object[] { ((TypeLoadExceptionHolder) this.TopObject).TypeName }));
                }
            }
            else
            {
                for (int i = 0; i < this.m_objects.Length; i++)
                {
                    for (current = this.m_objects[i]; current != null; current = current.m_next)
                    {
                        if (current.TotalDependentObjects > 0)
                        {
                            this.CompleteObject(current, true);
                        }
                    }
                    if (this.m_fixupCount == 0L)
                    {
                        return;
                    }
                }
                throw new SerializationException(Environment.GetResourceString("Serialization_IncorrectNumberOfFixups"));
            }
        }

        [SecurityCritical]
        private void DoNewlyRegisteredObjectFixups(ObjectHolder holder)
        {
            if (!holder.CanObjectValueChange)
            {
                LongList dependentObjects = holder.DependentObjects;
                if (dependentObjects != null)
                {
                    dependentObjects.StartEnumeration();
                    while (dependentObjects.MoveNext())
                    {
                        ObjectHolder holder2 = this.FindObjectHolder(dependentObjects.Current);
                        holder2.DecrementFixupsRemaining(this);
                        if (holder2.DirectlyDependentObjects == 0)
                        {
                            if (holder2.ObjectValue != null)
                            {
                                this.CompleteObject(holder2, true);
                            }
                            else
                            {
                                holder2.MarkForCompletionWhenAvailable();
                            }
                        }
                    }
                }
            }
        }

        [SecurityCritical]
        private bool DoValueTypeFixup(FieldInfo memberToFix, ObjectHolder holder, object value)
        {
            FieldInfo[] sourceArray = new FieldInfo[4];
            FieldInfo[] flds = null;
            int index = 0;
            int[] indices = null;
            ValueTypeFixupInfo valueFixup = null;
            object objectValue = holder.ObjectValue;
            while (holder.RequiresValueTypeFixup)
            {
                if ((index + 1) >= sourceArray.Length)
                {
                    FieldInfo[] destinationArray = new FieldInfo[sourceArray.Length * 2];
                    Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                    sourceArray = destinationArray;
                }
                valueFixup = holder.ValueFixup;
                objectValue = holder.ObjectValue;
                if (valueFixup.ParentField != null)
                {
                    FieldInfo parentField = valueFixup.ParentField;
                    ObjectHolder holder2 = this.FindObjectHolder(valueFixup.ContainerID);
                    if (holder2.ObjectValue == null)
                    {
                        break;
                    }
                    if (Nullable.GetUnderlyingType(parentField.FieldType) != null)
                    {
                        sourceArray[index] = parentField.FieldType.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);
                        index++;
                    }
                    sourceArray[index] = parentField;
                    holder = holder2;
                    index++;
                }
                else
                {
                    holder = this.FindObjectHolder(valueFixup.ContainerID);
                    indices = valueFixup.ParentIndex;
                    if (holder.ObjectValue != null)
                    {
                    }
                    break;
                }
            }
            if (!(holder.ObjectValue is Array) && (holder.ObjectValue != null))
            {
                objectValue = holder.ObjectValue;
            }
            if (index != 0)
            {
                flds = new FieldInfo[index];
                for (int i = 0; i < index; i++)
                {
                    FieldInfo info3 = sourceArray[(index - 1) - i];
                    SerializationFieldInfo info4 = info3 as SerializationFieldInfo;
                    flds[i] = (info4 == null) ? info3 : info4.FieldInfo;
                }
                TypedReference reference = TypedReference.MakeTypedReference(objectValue, flds);
                if (memberToFix != null)
                {
                    ((RuntimeFieldInfo) memberToFix).SetValueDirect(reference, value);
                }
                else
                {
                    TypedReference.SetTypedReference(reference, value);
                }
            }
            else if (memberToFix != null)
            {
                FormatterServices.SerializationSetValue(memberToFix, objectValue, value);
            }
            if ((indices != null) && (holder.ObjectValue != null))
            {
                ((Array) holder.ObjectValue).SetValue(objectValue, indices);
            }
            return true;
        }

        [Conditional("SER_LOGGING")]
        private void DumpValueTypeFixup(object obj, FieldInfo[] intermediateFields, FieldInfo memberToFix, object value)
        {
            StringBuilder builder = new StringBuilder("  " + obj);
            if (intermediateFields != null)
            {
                for (int i = 0; i < intermediateFields.Length; i++)
                {
                    builder.Append("." + intermediateFields[i].Name);
                }
            }
            builder.Append(string.Concat(new object[] { ".", memberToFix.Name, "=", value }));
        }

        internal ObjectHolder FindObjectHolder(long objectID)
        {
            int index = (int) (objectID & 0xfffL);
            if (index >= this.m_objects.Length)
            {
                return null;
            }
            ObjectHolder next = this.m_objects[index];
            while (next != null)
            {
                if (next.m_id == objectID)
                {
                    return next;
                }
                next = next.m_next;
            }
            return next;
        }

        internal ObjectHolder FindOrCreateObjectHolder(long objectID)
        {
            ObjectHolder holder = this.FindObjectHolder(objectID);
            if (holder == null)
            {
                holder = new ObjectHolder(objectID);
                this.AddObjectHolder(holder);
            }
            return holder;
        }

        [SecurityCritical]
        private void FixupSpecialObject(ObjectHolder holder)
        {
            ISurrogateSelector selector = null;
            if (holder.HasSurrogate)
            {
                ISerializationSurrogate surrogate = holder.Surrogate;
                object obj2 = surrogate.SetObjectData(holder.ObjectValue, holder.SerializationInfo, this.m_context, selector);
                if (obj2 != null)
                {
                    if (!holder.CanSurrogatedObjectValueChange && (obj2 != holder.ObjectValue))
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_NotCyclicallyReferenceableSurrogate"), new object[] { surrogate.GetType().FullName }));
                    }
                    holder.SetObjectValue(obj2, this);
                }
                holder.m_surrogate = null;
                holder.SetFlags();
            }
            else
            {
                this.CompleteISerializableObject(holder.ObjectValue, holder.SerializationInfo, this.m_context);
            }
            holder.SerializationInfo = null;
            holder.RequiresSerInfoFixup = false;
            if (holder.RequiresValueTypeFixup && holder.ValueTypeFixupPerformed)
            {
                this.DoValueTypeFixup(null, holder, holder.ObjectValue);
            }
            this.DoNewlyRegisteredObjectFixups(holder);
        }

        private bool GetCompletionInfo(FixupHolder fixup, out ObjectHolder holder, out object member, bool bThrowIfMissing)
        {
            member = fixup.m_fixupInfo;
            holder = this.FindObjectHolder(fixup.m_id);
            if ((!holder.CompletelyFixed && (holder.ObjectValue != null)) && (holder.ObjectValue is ValueType))
            {
                this.SpecialFixupObjects.Add(holder);
                return false;
            }
            if (((holder != null) && !holder.CanObjectValueChange) && (holder.ObjectValue != null))
            {
                return true;
            }
            if (!bThrowIfMissing)
            {
                return false;
            }
            if (holder == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_NeverSeen", new object[] { fixup.m_id }));
            }
            if (holder.IsIncompleteObjectReference)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_IORIncomplete", new object[] { fixup.m_id }));
            }
            throw new SerializationException(Environment.GetResourceString("Serialization_ObjectNotSupplied", new object[] { fixup.m_id }));
        }

        internal static RuntimeConstructorInfo GetConstructor(RuntimeType t)
        {
            return GetConstructor(t, SIConstructorTypes);
        }

        internal static RuntimeConstructorInfo GetConstructor(RuntimeType t, RuntimeType[] ctorParams)
        {
            RuntimeConstructorInfo info;
            if (!TryGetConstructor(t, ctorParams, out info))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_ConstructorNotFound", new object[] { t.FullName }));
            }
            return info;
        }

        public virtual object GetObject(long objectID)
        {
            if (objectID <= 0L)
            {
                throw new ArgumentOutOfRangeException("objectID", Environment.GetResourceString("ArgumentOutOfRange_ObjectID"));
            }
            ObjectHolder holder = this.FindObjectHolder(objectID);
            if ((holder != null) && !holder.CanObjectValueChange)
            {
                return holder.ObjectValue;
            }
            return null;
        }

        public virtual void RaiseDeserializationEvent()
        {
            if (this.m_onDeserializedHandler != null)
            {
                this.m_onDeserializedHandler(this.m_context);
            }
            if (this.m_onDeserializationHandler != null)
            {
                this.m_onDeserializationHandler(null);
            }
        }

        internal virtual void RaiseOnDeserializedEvent(object obj)
        {
            SerializationEventsCache.GetSerializationEventsForType(obj.GetType()).InvokeOnDeserialized(obj, this.m_context);
        }

        public void RaiseOnDeserializingEvent(object obj)
        {
            SerializationEventsCache.GetSerializationEventsForType(obj.GetType()).InvokeOnDeserializing(obj, this.m_context);
        }

        public virtual void RecordArrayElementFixup(long arrayToBeFixed, int index, long objectRequired)
        {
            int[] indices = new int[] { index };
            this.RecordArrayElementFixup(arrayToBeFixed, indices, objectRequired);
        }

        public virtual void RecordArrayElementFixup(long arrayToBeFixed, int[] indices, long objectRequired)
        {
            if ((arrayToBeFixed <= 0L) || (objectRequired <= 0L))
            {
                throw new ArgumentOutOfRangeException((arrayToBeFixed <= 0L) ? "objectToBeFixed" : "objectRequired", Environment.GetResourceString("Serialization_IdTooSmall"));
            }
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            FixupHolder fixup = new FixupHolder(objectRequired, indices, 1);
            this.RegisterFixup(fixup, arrayToBeFixed, objectRequired);
        }

        public virtual void RecordDelayedFixup(long objectToBeFixed, string memberName, long objectRequired)
        {
            if ((objectToBeFixed <= 0L) || (objectRequired <= 0L))
            {
                throw new ArgumentOutOfRangeException((objectToBeFixed <= 0L) ? "objectToBeFixed" : "objectRequired", Environment.GetResourceString("Serialization_IdTooSmall"));
            }
            if (memberName == null)
            {
                throw new ArgumentNullException("memberName");
            }
            FixupHolder fixup = new FixupHolder(objectRequired, memberName, 4);
            this.RegisterFixup(fixup, objectToBeFixed, objectRequired);
        }

        public virtual void RecordFixup(long objectToBeFixed, MemberInfo member, long objectRequired)
        {
            if ((objectToBeFixed <= 0L) || (objectRequired <= 0L))
            {
                throw new ArgumentOutOfRangeException((objectToBeFixed <= 0L) ? "objectToBeFixed" : "objectRequired", Environment.GetResourceString("Serialization_IdTooSmall"));
            }
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            if (!(member is RuntimeFieldInfo) && !(member is SerializationFieldInfo))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidType", new object[] { member.GetType().ToString() }));
            }
            FixupHolder fixup = new FixupHolder(objectRequired, member, 2);
            this.RegisterFixup(fixup, objectToBeFixed, objectRequired);
        }

        private void RegisterFixup(FixupHolder fixup, long objectToBeFixed, long objectRequired)
        {
            ObjectHolder holder = this.FindOrCreateObjectHolder(objectToBeFixed);
            if (holder.RequiresSerInfoFixup && (fixup.m_fixupType == 2))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFixupType"));
            }
            holder.AddFixup(fixup, this);
            this.FindOrCreateObjectHolder(objectRequired).AddDependency(objectToBeFixed);
            this.m_fixupCount += 1L;
        }

        [SecurityCritical]
        public virtual void RegisterObject(object obj, long objectID)
        {
            this.RegisterObject(obj, objectID, null, 0L, null);
        }

        [SecurityCritical]
        public void RegisterObject(object obj, long objectID, SerializationInfo info)
        {
            this.RegisterObject(obj, objectID, info, 0L, null);
        }

        [SecurityCritical]
        public void RegisterObject(object obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member)
        {
            this.RegisterObject(obj, objectID, info, idOfContainingObj, member, null);
        }

        [SecurityCritical]
        public void RegisterObject(object obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member, int[] arrayIndex)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (objectID <= 0L)
            {
                throw new ArgumentOutOfRangeException("objectID", Environment.GetResourceString("ArgumentOutOfRange_ObjectID"));
            }
            if (((member != null) && !(member is RuntimeFieldInfo)) && !(member is SerializationFieldInfo))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMemberInfo"));
            }
            ISerializationSurrogate surrogate = null;
            if (this.m_selector != null)
            {
                ISurrogateSelector selector;
                Type type = null;
                if (this.CanCallGetType(obj))
                {
                    type = obj.GetType();
                }
                else
                {
                    type = typeof(MarshalByRefObject);
                }
                surrogate = this.m_selector.GetSurrogate(type, this.m_context, out selector);
            }
            if (obj is IDeserializationCallback)
            {
                IDeserializationCallback callback1 = (IDeserializationCallback) obj;
                DeserializationEventHandler handler = new DeserializationEventHandler(callback1.OnDeserialization);
                this.AddOnDeserialization(handler);
            }
            if (arrayIndex != null)
            {
                arrayIndex = (int[]) arrayIndex.Clone();
            }
            ObjectHolder holder = this.FindObjectHolder(objectID);
            if (holder == null)
            {
                holder = new ObjectHolder(obj, objectID, info, surrogate, idOfContainingObj, (FieldInfo) member, arrayIndex);
                this.AddObjectHolder(holder);
                if (holder.RequiresDelayedFixup)
                {
                    this.SpecialFixupObjects.Add(holder);
                }
                this.AddOnDeserialized(obj);
            }
            else
            {
                if (holder.ObjectValue != null)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_RegisterTwice"));
                }
                holder.UpdateData(obj, info, surrogate, idOfContainingObj, (FieldInfo) member, arrayIndex, this);
                if (holder.DirectlyDependentObjects > 0)
                {
                    this.CompleteObject(holder, false);
                }
                if (holder.RequiresDelayedFixup)
                {
                    this.SpecialFixupObjects.Add(holder);
                }
                if (holder.CompletelyFixed)
                {
                    this.DoNewlyRegisteredObjectFixups(holder);
                    holder.DependentObjects = null;
                }
                if (holder.TotalDependentObjects > 0)
                {
                    this.AddOnDeserialized(obj);
                }
                else
                {
                    this.RaiseOnDeserializedEvent(obj);
                }
            }
        }

        internal void RegisterString(string obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member)
        {
            ObjectHolder holder = new ObjectHolder(obj, objectID, info, null, idOfContainingObj, (FieldInfo) member, null);
            this.AddObjectHolder(holder);
        }

        internal virtual void RemoveOnDeserialization(DeserializationEventHandler handler)
        {
            this.m_onDeserializationHandler = (DeserializationEventHandler) Delegate.Remove(this.m_onDeserializationHandler, handler);
        }

        [SecurityCritical]
        private bool ResolveObjectReference(ObjectHolder holder)
        {
            int num = 0;
            try
            {
                object objectValue;
                do
                {
                    objectValue = holder.ObjectValue;
                    holder.SetObjectValue(((IObjectReference) holder.ObjectValue).GetRealObject(this.m_context), this);
                    if (holder.ObjectValue == null)
                    {
                        holder.SetObjectValue(objectValue, this);
                        return false;
                    }
                    if (num++ == 100)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_TooManyReferences"));
                    }
                }
                while ((holder.ObjectValue is IObjectReference) && (objectValue != holder.ObjectValue));
            }
            catch (NullReferenceException)
            {
                return false;
            }
            holder.IsIncompleteObjectReference = false;
            this.DoNewlyRegisteredObjectFixups(holder);
            return true;
        }

        internal static bool TryGetConstructor(RuntimeType t, out RuntimeConstructorInfo ctorInfo)
        {
            return TryGetConstructor(t, SIConstructorTypes, out ctorInfo);
        }

        [SecuritySafeCritical]
        internal static bool TryGetConstructor(RuntimeType t, RuntimeType[] ctorParams, out RuntimeConstructorInfo ctorInfo)
        {
            ctorInfo = t.RemotingCache[CacheObjType.ConstructorInfo] as RuntimeConstructorInfo;
            if (ctorInfo != null)
            {
                return true;
            }
            ctorInfo = t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, ctorParams, null) as RuntimeConstructorInfo;
            if (ctorInfo != null)
            {
                t.RemotingCache[CacheObjType.ConstructorInfo] = ctorInfo;
                return true;
            }
            return false;
        }

        internal ObjectHolderList SpecialFixupObjects
        {
            get
            {
                if (this.m_specialFixupObjects == null)
                {
                    this.m_specialFixupObjects = new ObjectHolderList();
                }
                return this.m_specialFixupObjects;
            }
        }

        internal object TopObject
        {
            get
            {
                return this.m_topObject;
            }
            set
            {
                this.m_topObject = value;
            }
        }
    }
}

