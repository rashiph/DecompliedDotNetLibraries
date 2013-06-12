namespace System.Net
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class LazyAsyncResult : IAsyncResult
    {
        private const int c_ForceAsyncCount = 50;
        private const int c_HighBit = -2147483648;
        private System.AsyncCallback m_AsyncCallback;
        private object m_AsyncObject;
        private object m_AsyncState;
        private bool m_EndCalled;
        private int m_ErrorCode;
        private object m_Event;
        private int m_IntCompleted;
        private object m_Result;
        private bool m_UserEvent;
        [ThreadStatic]
        private static ThreadContext t_ThreadContext;

        internal LazyAsyncResult(object myObject, object myState, System.AsyncCallback myCallBack)
        {
            this.m_AsyncObject = myObject;
            this.m_AsyncState = myState;
            this.m_AsyncCallback = myCallBack;
            this.m_Result = DBNull.Value;
        }

        internal LazyAsyncResult(object myObject, object myState, System.AsyncCallback myCallBack, object result)
        {
            this.m_AsyncObject = myObject;
            this.m_AsyncState = myState;
            this.m_AsyncCallback = myCallBack;
            this.m_Result = result;
            this.m_IntCompleted = 1;
            if (this.m_AsyncCallback != null)
            {
                this.m_AsyncCallback(this);
            }
        }

        protected virtual void Cleanup()
        {
        }

        protected virtual void Complete(IntPtr userToken)
        {
            bool flag = false;
            ThreadContext currentThreadContext = CurrentThreadContext;
            try
            {
                currentThreadContext.m_NestedIOCount++;
                if (this.m_AsyncCallback != null)
                {
                    if (currentThreadContext.m_NestedIOCount >= 50)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.WorkerThreadComplete));
                        flag = true;
                    }
                    else
                    {
                        this.m_AsyncCallback(this);
                    }
                }
            }
            finally
            {
                currentThreadContext.m_NestedIOCount--;
                if (!flag)
                {
                    this.Cleanup();
                }
            }
        }

        [Conditional("DEBUG")]
        protected void DebugProtectState(bool protect)
        {
        }

        internal void InternalCleanup()
        {
            if (((this.m_IntCompleted & 0x7fffffff) == 0) && ((Interlocked.Increment(ref this.m_IntCompleted) & 0x7fffffff) == 1))
            {
                this.m_Result = null;
                this.Cleanup();
            }
        }

        internal object InternalWaitForCompletion()
        {
            return this.WaitForCompletion(true);
        }

        internal void InvokeCallback()
        {
            this.ProtectedInvokeCallback(null, IntPtr.Zero);
        }

        internal void InvokeCallback(object result)
        {
            this.ProtectedInvokeCallback(result, IntPtr.Zero);
        }

        private bool LazilyCreateEvent(out ManualResetEvent waitHandle)
        {
            bool flag;
            waitHandle = new ManualResetEvent(false);
            try
            {
                if (Interlocked.CompareExchange(ref this.m_Event, waitHandle, null) == null)
                {
                    if (this.InternalPeekCompleted)
                    {
                        waitHandle.Set();
                    }
                    return true;
                }
                waitHandle.Close();
                waitHandle = (ManualResetEvent) this.m_Event;
                flag = false;
            }
            catch
            {
                this.m_Event = null;
                if (waitHandle != null)
                {
                    waitHandle.Close();
                }
                throw;
            }
            return flag;
        }

        protected void ProtectedInvokeCallback(object result, IntPtr userToken)
        {
            if (result == DBNull.Value)
            {
                throw new ArgumentNullException("result");
            }
            if (((this.m_IntCompleted & 0x7fffffff) == 0) && ((Interlocked.Increment(ref this.m_IntCompleted) & 0x7fffffff) == 1))
            {
                if (this.m_Result == DBNull.Value)
                {
                    this.m_Result = result;
                }
                ManualResetEvent event2 = (ManualResetEvent) this.m_Event;
                if (event2 != null)
                {
                    try
                    {
                        event2.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                this.Complete(userToken);
            }
        }

        private object WaitForCompletion(bool snap)
        {
            ManualResetEvent waitHandle = null;
            bool flag = false;
            if (!(snap ? this.IsCompleted : this.InternalPeekCompleted))
            {
                waitHandle = (ManualResetEvent) this.m_Event;
                if (waitHandle == null)
                {
                    flag = this.LazilyCreateEvent(out waitHandle);
                }
            }
            if (waitHandle == null)
            {
                goto Label_0077;
            }
            try
            {
                try
                {
                    waitHandle.WaitOne(-1, false);
                }
                catch (ObjectDisposedException)
                {
                }
                goto Label_0077;
            }
            finally
            {
                if (flag && !this.m_UserEvent)
                {
                    ManualResetEvent event3 = (ManualResetEvent) this.m_Event;
                    this.m_Event = null;
                    if (!this.m_UserEvent)
                    {
                        event3.Close();
                    }
                }
            }
        Label_0071:
            Thread.SpinWait(1);
        Label_0077:
            if (this.m_Result == DBNull.Value)
            {
                goto Label_0071;
            }
            return this.m_Result;
        }

        private void WorkerThreadComplete(object state)
        {
            try
            {
                this.m_AsyncCallback(this);
            }
            finally
            {
                this.Cleanup();
            }
        }

        protected System.AsyncCallback AsyncCallback
        {
            get
            {
                return this.m_AsyncCallback;
            }
            set
            {
                this.m_AsyncCallback = value;
            }
        }

        internal object AsyncObject
        {
            get
            {
                return this.m_AsyncObject;
            }
        }

        public object AsyncState
        {
            get
            {
                return this.m_AsyncState;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                this.m_UserEvent = true;
                if (this.m_IntCompleted == 0)
                {
                    Interlocked.CompareExchange(ref this.m_IntCompleted, -2147483648, 0);
                }
                ManualResetEvent waitHandle = (ManualResetEvent) this.m_Event;
                while (waitHandle == null)
                {
                    this.LazilyCreateEvent(out waitHandle);
                }
                return waitHandle;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                int intCompleted = this.m_IntCompleted;
                if (intCompleted == 0)
                {
                    intCompleted = Interlocked.CompareExchange(ref this.m_IntCompleted, -2147483648, 0);
                }
                return (intCompleted > 0);
            }
        }

        private static ThreadContext CurrentThreadContext
        {
            get
            {
                ThreadContext context = t_ThreadContext;
                if (context == null)
                {
                    context = new ThreadContext();
                    t_ThreadContext = context;
                }
                return context;
            }
        }

        internal bool EndCalled
        {
            get
            {
                return this.m_EndCalled;
            }
            set
            {
                this.m_EndCalled = value;
            }
        }

        internal int ErrorCode
        {
            get
            {
                return this.m_ErrorCode;
            }
            set
            {
                this.m_ErrorCode = value;
            }
        }

        internal bool InternalPeekCompleted
        {
            get
            {
                return ((this.m_IntCompleted & 0x7fffffff) != 0);
            }
        }

        public bool IsCompleted
        {
            get
            {
                int intCompleted = this.m_IntCompleted;
                if (intCompleted == 0)
                {
                    intCompleted = Interlocked.CompareExchange(ref this.m_IntCompleted, -2147483648, 0);
                }
                return ((intCompleted & 0x7fffffff) != 0);
            }
        }

        internal object Result
        {
            get
            {
                if (this.m_Result != DBNull.Value)
                {
                    return this.m_Result;
                }
                return null;
            }
            set
            {
                this.m_Result = value;
            }
        }

        private class ThreadContext
        {
            internal int m_NestedIOCount;
        }
    }
}

