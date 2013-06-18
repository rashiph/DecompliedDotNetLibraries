namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class NoPersistHandle : Handle
    {
        public void Enter(NativeActivityContext context)
        {
            context.ThrowIfDisposed();
            base.ThrowIfUninitialized();
            context.EnterNoPersist(this);
        }

        public void Exit(NativeActivityContext context)
        {
            context.ThrowIfDisposed();
            base.ThrowIfUninitialized();
            context.ExitNoPersist(this);
        }
    }
}

