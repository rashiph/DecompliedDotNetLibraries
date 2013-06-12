namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    internal class ContextAwareResult : LazyAsyncResult
    {
        private volatile ExecutionContext _Context;
        private StateFlags _Flags;
        private object _Lock;
        private WindowsIdentity _Wi;

        internal ContextAwareResult(object myObject, object myState, AsyncCallback myCallBack) : this(false, false, myObject, myState, myCallBack)
        {
        }

        internal ContextAwareResult(bool captureIdentity, bool forceCaptureContext, object myObject, object myState, AsyncCallback myCallBack) : this(captureIdentity, forceCaptureContext, false, myObject, myState, myCallBack)
        {
        }

        internal ContextAwareResult(bool captureIdentity, bool forceCaptureContext, bool threadSafeContextCopy, object myObject, object myState, AsyncCallback myCallBack) : base(myObject, myState, myCallBack)
        {
            if (forceCaptureContext)
            {
                this._Flags = StateFlags.CaptureContext;
            }
            if (captureIdentity)
            {
                this._Flags |= StateFlags.CaptureIdentity;
            }
            if (threadSafeContextCopy)
            {
                this._Flags |= StateFlags.ThreadSafeContextCopy;
            }
        }

        private bool CaptureOrComplete(ref ExecutionContext cachedContext, bool returnContext)
        {
            bool flag = (base.AsyncCallback != null) || ((this._Flags & StateFlags.CaptureContext) != StateFlags.None);
            if ((((this._Flags & StateFlags.CaptureIdentity) != StateFlags.None) && !base.InternalPeekCompleted) && (!flag || SecurityContext.IsWindowsIdentityFlowSuppressed()))
            {
                this.SafeCaptureIdenity();
            }
            if (flag && !base.InternalPeekCompleted)
            {
                if (cachedContext == null)
                {
                    cachedContext = ExecutionContext.Capture();
                }
                if (cachedContext != null)
                {
                    if (!returnContext)
                    {
                        this._Context = cachedContext;
                        cachedContext = null;
                    }
                    else
                    {
                        this._Context = cachedContext.CreateCopy();
                    }
                }
            }
            else
            {
                cachedContext = null;
            }
            if (base.CompletedSynchronously)
            {
                base.Complete(IntPtr.Zero);
                return true;
            }
            return false;
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            if (this._Wi != null)
            {
                this._Wi.Dispose();
                this._Wi = null;
            }
        }

        protected override void Complete(IntPtr userToken)
        {
            if ((this._Flags & StateFlags.PostBlockStarted) == StateFlags.None)
            {
                base.Complete(userToken);
            }
            else if (!base.CompletedSynchronously)
            {
                ExecutionContext context = this._Context;
                if ((userToken != IntPtr.Zero) || (context == null))
                {
                    base.Complete(userToken);
                }
                else
                {
                    ExecutionContext.Run(((this._Flags & StateFlags.ThreadSafeContextCopy) != StateFlags.None) ? context.CreateCopy() : context, new ContextCallback(this.CompleteCallback), null);
                }
            }
        }

        private void CompleteCallback(object state)
        {
            base.Complete(IntPtr.Zero);
        }

        internal bool FinishPostingAsyncOp()
        {
            if ((this._Flags & (StateFlags.PostBlockFinished | StateFlags.PostBlockStarted)) != StateFlags.PostBlockStarted)
            {
                return false;
            }
            this._Flags |= StateFlags.PostBlockFinished;
            ExecutionContext cachedContext = null;
            return this.CaptureOrComplete(ref cachedContext, false);
        }

        internal bool FinishPostingAsyncOp(ref CallbackClosure closure)
        {
            ExecutionContext context;
            if ((this._Flags & (StateFlags.PostBlockFinished | StateFlags.PostBlockStarted)) != StateFlags.PostBlockStarted)
            {
                return false;
            }
            this._Flags |= StateFlags.PostBlockFinished;
            CallbackClosure closure2 = closure;
            if (closure2 == null)
            {
                context = null;
            }
            else if (!closure2.IsCompatible(base.AsyncCallback))
            {
                closure = null;
                context = null;
            }
            else
            {
                base.AsyncCallback = closure2.AsyncCallback;
                context = closure2.Context;
            }
            bool flag = this.CaptureOrComplete(ref context, true);
            if (((closure == null) && (base.AsyncCallback != null)) && (context != null))
            {
                closure = new CallbackClosure(context, base.AsyncCallback);
            }
            return flag;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        private void SafeCaptureIdenity()
        {
            this._Wi = WindowsIdentity.GetCurrent();
        }

        internal object StartPostingAsyncOp()
        {
            return this.StartPostingAsyncOp(true);
        }

        internal object StartPostingAsyncOp(bool lockCapture)
        {
            this._Lock = lockCapture ? new object() : null;
            this._Flags |= StateFlags.PostBlockStarted;
            return this._Lock;
        }

        internal ExecutionContext ContextCopy
        {
            get
            {
                if (base.InternalPeekCompleted)
                {
                    throw new InvalidOperationException(SR.GetString("net_completed_result"));
                }
                ExecutionContext context = this._Context;
                if (context != null)
                {
                    return context.CreateCopy();
                }
                if ((this._Flags & StateFlags.PostBlockFinished) == StateFlags.None)
                {
                    lock (this._Lock)
                    {
                    }
                }
                if (base.InternalPeekCompleted)
                {
                    throw new InvalidOperationException(SR.GetString("net_completed_result"));
                }
                context = this._Context;
                if (context != null)
                {
                    return context.CreateCopy();
                }
                return null;
            }
        }

        internal WindowsIdentity Identity
        {
            get
            {
                if (base.InternalPeekCompleted)
                {
                    throw new InvalidOperationException(SR.GetString("net_completed_result"));
                }
                if (this._Wi == null)
                {
                    if ((this._Flags & StateFlags.PostBlockFinished) == StateFlags.None)
                    {
                        lock (this._Lock)
                        {
                        }
                    }
                    if (base.InternalPeekCompleted)
                    {
                        throw new InvalidOperationException(SR.GetString("net_completed_result"));
                    }
                }
                return this._Wi;
            }
        }

        [Flags]
        private enum StateFlags
        {
            CaptureContext = 2,
            CaptureIdentity = 1,
            None = 0,
            PostBlockFinished = 0x10,
            PostBlockStarted = 8,
            ThreadSafeContextCopy = 4
        }
    }
}

