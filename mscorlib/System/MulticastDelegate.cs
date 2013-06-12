namespace System
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public abstract class MulticastDelegate : Delegate
    {
        private IntPtr _invocationCount;
        private object _invocationList;

        protected MulticastDelegate(object target, string method) : base(target, method)
        {
        }

        protected MulticastDelegate(Type target, string method) : base(target, method)
        {
        }

        [SecuritySafeCritical]
        protected sealed override Delegate CombineImpl(Delegate follow)
        {
            object[] objArray;
            int num2;
            if (follow == null)
            {
                return this;
            }
            if (!Delegate.InternalEqualTypes(this, follow))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTypeMis"));
            }
            MulticastDelegate o = (MulticastDelegate) follow;
            int num = 1;
            object[] objArray2 = o._invocationList as object[];
            if (objArray2 != null)
            {
                num = (int) o._invocationCount;
            }
            object[] objArray3 = this._invocationList as object[];
            if (objArray3 == null)
            {
                num2 = 1 + num;
                objArray = new object[num2];
                objArray[0] = this;
                if (objArray2 == null)
                {
                    objArray[1] = o;
                }
                else
                {
                    for (int i = 0; i < num; i++)
                    {
                        objArray[1 + i] = objArray2[i];
                    }
                }
                return this.NewMulticastDelegate(objArray, num2);
            }
            int index = (int) this._invocationCount;
            num2 = index + num;
            objArray = null;
            if (num2 <= objArray3.Length)
            {
                objArray = objArray3;
                if (objArray2 == null)
                {
                    if (!this.TrySetSlot(objArray, index, o))
                    {
                        objArray = null;
                    }
                }
                else
                {
                    for (int j = 0; j < num; j++)
                    {
                        if (!this.TrySetSlot(objArray, index + j, objArray2[j]))
                        {
                            objArray = null;
                            break;
                        }
                    }
                }
            }
            if (objArray == null)
            {
                int length = objArray3.Length;
                while (length < num2)
                {
                    length *= 2;
                }
                objArray = new object[length];
                for (int k = 0; k < index; k++)
                {
                    objArray[k] = objArray3[k];
                }
                if (objArray2 == null)
                {
                    objArray[index] = o;
                }
                else
                {
                    for (int m = 0; m < num; m++)
                    {
                        objArray[index + m] = objArray2[m];
                    }
                }
            }
            return this.NewMulticastDelegate(objArray, num2, true);
        }

        [ForceTokenStabilization, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), DebuggerNonUserCode]
        private void CtorClosed(object target, IntPtr methodPtr)
        {
            if (target == null)
            {
                this.ThrowNullThisInDelegateToInstance();
            }
            base._target = target;
            base._methodPtr = methodPtr;
        }

        [ForceTokenStabilization, DebuggerNonUserCode]
        private void CtorClosedStatic(object target, IntPtr methodPtr)
        {
            base._target = target;
            base._methodPtr = methodPtr;
        }

        [DebuggerNonUserCode, ForceTokenStabilization, SecurityCritical]
        private void CtorCollectibleClosedStatic(object target, IntPtr methodPtr, IntPtr gchandle)
        {
            base._target = target;
            base._methodPtr = methodPtr;
            base._methodBase = GCHandle.InternalGet(gchandle);
        }

        [SecurityCritical, DebuggerNonUserCode, ForceTokenStabilization]
        private void CtorCollectibleOpened(object target, IntPtr methodPtr, IntPtr shuffleThunk, IntPtr gchandle)
        {
            base._target = this;
            base._methodPtr = shuffleThunk;
            base._methodPtrAux = methodPtr;
            base._methodBase = GCHandle.InternalGet(gchandle);
        }

        [DebuggerNonUserCode, SecurityCritical, ForceTokenStabilization]
        private void CtorCollectibleVirtualDispatch(object target, IntPtr methodPtr, IntPtr shuffleThunk, IntPtr gchandle)
        {
            base._target = this;
            base._methodPtr = shuffleThunk;
            base._methodPtrAux = base.GetCallStub(methodPtr);
            base._methodBase = GCHandle.InternalGet(gchandle);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ForceTokenStabilization, DebuggerNonUserCode]
        private void CtorOpened(object target, IntPtr methodPtr, IntPtr shuffleThunk)
        {
            base._target = this;
            base._methodPtr = shuffleThunk;
            base._methodPtrAux = methodPtr;
        }

        [ForceTokenStabilization, DebuggerNonUserCode, SecurityCritical]
        private void CtorRTClosed(object target, IntPtr methodPtr)
        {
            base._target = target;
            base._methodPtr = base.AdjustTarget(target, methodPtr);
        }

        [ForceTokenStabilization, SecurityCritical, DebuggerNonUserCode]
        private void CtorSecureClosed(object target, IntPtr methodPtr, IntPtr callThunk, IntPtr creatorMethod)
        {
            MulticastDelegate delegate2 = Delegate.InternalAllocLike(this);
            delegate2.CtorClosed(target, methodPtr);
            this._invocationList = delegate2;
            base._target = this;
            base._methodPtr = callThunk;
            base._methodPtrAux = creatorMethod;
            this._invocationCount = base.GetInvokeMethod();
        }

        [SecurityCritical, ForceTokenStabilization, DebuggerNonUserCode]
        private void CtorSecureClosedStatic(object target, IntPtr methodPtr, IntPtr callThunk, IntPtr creatorMethod)
        {
            MulticastDelegate delegate2 = Delegate.InternalAllocLike(this);
            delegate2.CtorClosedStatic(target, methodPtr);
            this._invocationList = delegate2;
            base._target = this;
            base._methodPtr = callThunk;
            base._methodPtrAux = creatorMethod;
            this._invocationCount = base.GetInvokeMethod();
        }

        [SecurityCritical, DebuggerNonUserCode, ForceTokenStabilization]
        private void CtorSecureOpened(object target, IntPtr methodPtr, IntPtr shuffleThunk, IntPtr callThunk, IntPtr creatorMethod)
        {
            MulticastDelegate delegate2 = Delegate.InternalAllocLike(this);
            delegate2.CtorOpened(target, methodPtr, shuffleThunk);
            this._invocationList = delegate2;
            base._target = this;
            base._methodPtr = callThunk;
            base._methodPtrAux = creatorMethod;
            this._invocationCount = base.GetInvokeMethod();
        }

        [SecurityCritical, DebuggerNonUserCode, ForceTokenStabilization]
        private void CtorSecureRTClosed(object target, IntPtr methodPtr, IntPtr callThunk, IntPtr creatorMethod)
        {
            MulticastDelegate delegate2 = Delegate.InternalAllocLike(this);
            delegate2.CtorRTClosed(target, methodPtr);
            this._invocationList = delegate2;
            base._target = this;
            base._methodPtr = callThunk;
            base._methodPtrAux = creatorMethod;
            this._invocationCount = base.GetInvokeMethod();
        }

        [ForceTokenStabilization, SecurityCritical, DebuggerNonUserCode]
        private void CtorSecureVirtualDispatch(object target, IntPtr methodPtr, IntPtr shuffleThunk, IntPtr callThunk, IntPtr creatorMethod)
        {
            MulticastDelegate delegate2 = Delegate.InternalAllocLike(this);
            delegate2.CtorVirtualDispatch(target, methodPtr, shuffleThunk);
            this._invocationList = delegate2;
            base._target = this;
            base._methodPtr = callThunk;
            base._methodPtrAux = creatorMethod;
            this._invocationCount = base.GetInvokeMethod();
        }

        [ForceTokenStabilization, SecurityCritical, DebuggerNonUserCode]
        private void CtorVirtualDispatch(object target, IntPtr methodPtr, IntPtr shuffleThunk)
        {
            base._target = this;
            base._methodPtr = shuffleThunk;
            base._methodPtrAux = base.GetCallStub(methodPtr);
        }

        private object[] DeleteFromInvocationList(object[] invocationList, int invocationCount, int deleteIndex, int deleteCount)
        {
            object[] objArray = this._invocationList as object[];
            int length = objArray.Length;
            while ((length / 2) >= (invocationCount - deleteCount))
            {
                length /= 2;
            }
            object[] objArray2 = new object[length];
            for (int i = 0; i < deleteIndex; i++)
            {
                objArray2[i] = invocationList[i];
            }
            for (int j = deleteIndex + deleteCount; j < invocationCount; j++)
            {
                objArray2[j - deleteCount] = invocationList[j];
            }
            return objArray2;
        }

        private bool EqualInvocationLists(object[] a, object[] b, int start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!a[start + i].Equals(b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        [SecuritySafeCritical]
        public sealed override bool Equals(object obj)
        {
            if ((obj == null) || !Delegate.InternalEqualTypes(this, obj))
            {
                return false;
            }
            MulticastDelegate delegate2 = obj as MulticastDelegate;
            if (delegate2 == null)
            {
                return false;
            }
            if (this._invocationCount != IntPtr.Zero)
            {
                if (this.InvocationListLogicallyNull())
                {
                    if (!this.IsUnmanagedFunctionPtr())
                    {
                        return base.Equals(obj);
                    }
                    if (!delegate2.IsUnmanagedFunctionPtr())
                    {
                        return false;
                    }
                    return Delegate.CompareUnmanagedFunctionPtrs(this, delegate2);
                }
                if (this._invocationList is Delegate)
                {
                    return this._invocationList.Equals(obj);
                }
                return this.InvocationListEquals(delegate2);
            }
            if (!this.InvocationListLogicallyNull())
            {
                if (!this._invocationList.Equals(delegate2._invocationList))
                {
                    return false;
                }
                return base.Equals(delegate2);
            }
            if (((delegate2._invocationCount != IntPtr.Zero) || !this.InvocationListLogicallyNull()) && (delegate2._invocationList is Delegate))
            {
                return (delegate2._invocationList as Delegate).Equals(this);
            }
            return base.Equals(delegate2);
        }

        public sealed override int GetHashCode()
        {
            if (this.IsUnmanagedFunctionPtr())
            {
                return ValueType.GetHashCodeOfPtr(base._methodPtr);
            }
            object[] objArray = this._invocationList as object[];
            if (objArray == null)
            {
                return base.GetHashCode();
            }
            int num = 0;
            for (int i = 0; i < ((int) this._invocationCount); i++)
            {
                num = (num * 0x21) + objArray[i].GetHashCode();
            }
            return num;
        }

        public sealed override Delegate[] GetInvocationList()
        {
            object[] objArray = this._invocationList as object[];
            if (objArray == null)
            {
                return new Delegate[] { this };
            }
            int num = (int) this._invocationCount;
            Delegate[] delegateArray = new Delegate[num];
            for (int i = 0; i < num; i++)
            {
                delegateArray[i] = (Delegate) objArray[i];
            }
            return delegateArray;
        }

        protected override MethodInfo GetMethodImpl()
        {
            if ((this._invocationCount != IntPtr.Zero) && (this._invocationList != null))
            {
                object[] objArray = this._invocationList as object[];
                if (objArray != null)
                {
                    int index = ((int) this._invocationCount) - 1;
                    return ((Delegate) objArray[index]).Method;
                }
                MulticastDelegate delegate2 = this._invocationList as MulticastDelegate;
                if (delegate2 != null)
                {
                    return delegate2.GetMethodImpl();
                }
            }
            return base.GetMethodImpl();
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int targetIndex = 0;
            object[] objArray = this._invocationList as object[];
            if (objArray == null)
            {
                MethodInfo method = base.Method;
                if (!(method is RuntimeMethodInfo) || this.IsUnmanagedFunctionPtr())
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidDelegateType"));
                }
                if (!this.InvocationListLogicallyNull() && !this._invocationCount.IsNull())
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidDelegateType"));
                }
                DelegateSerializationHolder.GetDelegateSerializationInfo(info, base.GetType(), base.Target, method, targetIndex);
            }
            else
            {
                DelegateSerializationHolder.DelegateEntry entry = null;
                int index = (int) this._invocationCount;
                while (--index >= 0)
                {
                    MulticastDelegate delegate2 = (MulticastDelegate) objArray[index];
                    MethodInfo info3 = delegate2.Method;
                    if (((info3 is RuntimeMethodInfo) && !this.IsUnmanagedFunctionPtr()) && (delegate2.InvocationListLogicallyNull() || delegate2._invocationCount.IsNull()))
                    {
                        DelegateSerializationHolder.DelegateEntry entry2 = DelegateSerializationHolder.GetDelegateSerializationInfo(info, delegate2.GetType(), delegate2.Target, info3, targetIndex++);
                        if (entry != null)
                        {
                            entry.Entry = entry2;
                        }
                        entry = entry2;
                    }
                }
                if (entry == null)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidDelegateType"));
                }
            }
        }

        internal override object GetTarget()
        {
            if (this._invocationCount != IntPtr.Zero)
            {
                if (this.InvocationListLogicallyNull())
                {
                    return null;
                }
                object[] objArray = this._invocationList as object[];
                if (objArray != null)
                {
                    int num = (int) this._invocationCount;
                    return ((Delegate) objArray[num - 1]).GetTarget();
                }
                Delegate delegate2 = this._invocationList as Delegate;
                if (delegate2 != null)
                {
                    return delegate2.GetTarget();
                }
            }
            return base.GetTarget();
        }

        private bool InvocationListEquals(MulticastDelegate d)
        {
            object[] objArray = this._invocationList as object[];
            if (d._invocationCount != this._invocationCount)
            {
                return false;
            }
            int num = (int) this._invocationCount;
            for (int i = 0; i < num; i++)
            {
                Delegate delegate2 = (Delegate) objArray[i];
                object[] objArray2 = d._invocationList as object[];
                if (!delegate2.Equals(objArray2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal bool InvocationListLogicallyNull()
        {
            if ((this._invocationList != null) && !(this._invocationList is LoaderAllocator))
            {
                return (this._invocationList is DynamicResolver);
            }
            return true;
        }

        internal bool IsUnmanagedFunctionPtr()
        {
            return (this._invocationCount == ((IntPtr) (-1)));
        }

        internal MulticastDelegate NewMulticastDelegate(object[] invocationList, int invocationCount)
        {
            return this.NewMulticastDelegate(invocationList, invocationCount, false);
        }

        [SecuritySafeCritical]
        internal MulticastDelegate NewMulticastDelegate(object[] invocationList, int invocationCount, bool thisIsMultiCastAlready)
        {
            MulticastDelegate delegate2 = Delegate.InternalAllocLike(this);
            if (thisIsMultiCastAlready)
            {
                delegate2._methodPtr = base._methodPtr;
                delegate2._methodPtrAux = base._methodPtrAux;
            }
            else
            {
                delegate2._methodPtr = base.GetMulticastInvoke();
                delegate2._methodPtrAux = base.GetInvokeMethod();
            }
            delegate2._target = delegate2;
            delegate2._invocationList = invocationList;
            delegate2._invocationCount = (IntPtr) invocationCount;
            return delegate2;
        }

        public static bool operator ==(MulticastDelegate d1, MulticastDelegate d2)
        {
            if (d1 == null)
            {
                return (d2 == null);
            }
            return d1.Equals(d2);
        }

        public static bool operator !=(MulticastDelegate d1, MulticastDelegate d2)
        {
            if (d1 == null)
            {
                return (d2 != null);
            }
            return !d1.Equals(d2);
        }

        [SecuritySafeCritical]
        protected sealed override Delegate RemoveImpl(Delegate value)
        {
            MulticastDelegate delegate2 = value as MulticastDelegate;
            if (delegate2 != null)
            {
                if (delegate2._invocationList is object[])
                {
                    object[] a = this._invocationList as object[];
                    if (a != null)
                    {
                        int invocationCount = (int) this._invocationCount;
                        int count = (int) delegate2._invocationCount;
                        for (int i = invocationCount - count; i >= 0; i--)
                        {
                            if (this.EqualInvocationLists(a, delegate2._invocationList as object[], i, count))
                            {
                                if ((invocationCount - count) == 0)
                                {
                                    return null;
                                }
                                if ((invocationCount - count) == 1)
                                {
                                    return (Delegate) a[(i != 0) ? 0 : (invocationCount - 1)];
                                }
                                object[] invocationList = this.DeleteFromInvocationList(a, invocationCount, i, count);
                                return this.NewMulticastDelegate(invocationList, invocationCount - count, true);
                            }
                        }
                    }
                }
                else
                {
                    object[] objArray = this._invocationList as object[];
                    if (objArray == null)
                    {
                        if (this.Equals(value))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        int num = (int) this._invocationCount;
                        int index = num;
                        while (--index >= 0)
                        {
                            if (value.Equals(objArray[index]))
                            {
                                if (num == 2)
                                {
                                    return (Delegate) objArray[1 - index];
                                }
                                object[] objArray2 = this.DeleteFromInvocationList(objArray, num, index, 1);
                                return this.NewMulticastDelegate(objArray2, num - 1, true);
                            }
                        }
                    }
                }
            }
            return this;
        }

        internal void StoreDynamicMethod(MethodInfo dynamicMethod)
        {
            if (this._invocationCount != IntPtr.Zero)
            {
                MulticastDelegate delegate2 = (MulticastDelegate) this._invocationList;
                delegate2._methodBase = dynamicMethod;
            }
            else
            {
                base._methodBase = dynamicMethod;
            }
        }

        [DebuggerNonUserCode]
        private void ThrowNullThisInDelegateToInstance()
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_DlgtNullInst"));
        }

        private bool TrySetSlot(object[] a, int index, object o)
        {
            if ((a[index] == null) && (Interlocked.CompareExchange<object>(ref a[index], o, null) == null))
            {
                return true;
            }
            if (a[index] != null)
            {
                MulticastDelegate delegate2 = (MulticastDelegate) o;
                MulticastDelegate delegate3 = (MulticastDelegate) a[index];
                if (((delegate3._methodPtr == delegate2._methodPtr) && (delegate3._target == delegate2._target)) && (delegate3._methodPtrAux == delegate2._methodPtrAux))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

