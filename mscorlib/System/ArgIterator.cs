namespace System
{
    using System.Runtime.CompilerServices;
    using System.Security;

    public struct ArgIterator
    {
        private IntPtr ArgCookie;
        private IntPtr ArgPtr;
        private int RemainingArgs;
        private IntPtr sigPtr;
        private IntPtr sigPtrLen;

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern ArgIterator(IntPtr arglist);
        [SecuritySafeCritical]
        public ArgIterator(RuntimeArgumentHandle arglist) : this(arglist.Value)
        {
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern unsafe ArgIterator(IntPtr arglist, void* ptr);
        [SecurityCritical, CLSCompliant(false)]
        public unsafe ArgIterator(RuntimeArgumentHandle arglist, void* ptr) : this(arglist.Value, ptr)
        {
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern unsafe void* _GetNextArgType();
        public void End()
        {
        }

        public override bool Equals(object o)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NYI"));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern unsafe void FCallGetNextArg(void* result);
        public override int GetHashCode()
        {
            return ValueType.GetHashCodeOfPtr(this.ArgCookie);
        }

        [SecuritySafeCritical, CLSCompliant(false)]
        public unsafe TypedReference GetNextArg()
        {
            TypedReference reference = new TypedReference();
            this.FCallGetNextArg((void*) &reference);
            return reference;
        }

        [SecuritySafeCritical, CLSCompliant(false)]
        public unsafe TypedReference GetNextArg(RuntimeTypeHandle rth)
        {
            if (this.sigPtr != IntPtr.Zero)
            {
                return this.GetNextArg();
            }
            if (this.ArgPtr == IntPtr.Zero)
            {
                throw new ArgumentNullException();
            }
            TypedReference reference = new TypedReference();
            this.InternalGetNextArg((void*) &reference, rth.GetRuntimeType());
            return reference;
        }

        [SecuritySafeCritical]
        public RuntimeTypeHandle GetNextArgType()
        {
            return new RuntimeTypeHandle(Type.GetTypeFromHandleUnsafe((IntPtr) this._GetNextArgType()));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern int GetRemainingCount();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern unsafe void InternalGetNextArg(void* result, RuntimeType rt);
    }
}

