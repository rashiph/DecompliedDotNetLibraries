namespace System.Web.Compilation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web;
    using System.Web.Util;

    internal sealed class CompilationMutex : IDisposable
    {
        private string _comment;
        private bool _draining;
        private int _lockStatus;
        private HandleRef _mutexHandle;
        private string _name;

        internal CompilationMutex(string name, string comment)
        {
            string str = (string) Misc.GetAspNetRegValue("CompilationMutexName", null, null);
            if (str != null)
            {
                string str2 = this._name;
                this._name = str2 + @"Global\" + name + "-" + str;
            }
            else
            {
                this._name = this._name + @"Local\" + name;
            }
            this._comment = comment;
            this._mutexHandle = new HandleRef(this, UnsafeNativeMethods.InstrumentedMutexCreate(this._name));
            if (this._mutexHandle.Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("CompilationMutex_Create"));
            }
        }

        internal void Close()
        {
            if (this._mutexHandle.Handle != IntPtr.Zero)
            {
                UnsafeNativeMethods.InstrumentedMutexDelete(this._mutexHandle);
                this._mutexHandle = new HandleRef(this, IntPtr.Zero);
            }
        }

        ~CompilationMutex()
        {
            this.Close();
        }

        internal void ReleaseMutex()
        {
            if (this._mutexHandle.Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("CompilationMutex_Null"));
            }
            if (UnsafeNativeMethods.InstrumentedMutexReleaseLock(this._mutexHandle) != 0)
            {
                Interlocked.Decrement(ref this._lockStatus);
            }
        }

        void IDisposable.Dispose()
        {
            this.Close();
            GC.SuppressFinalize(this);
        }

        internal void WaitOne()
        {
            int num;
            if (this._mutexHandle.Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("CompilationMutex_Null"));
            }
            do
            {
                num = this._lockStatus;
                if ((num == -1) || this._draining)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("CompilationMutex_Drained"));
                }
            }
            while (Interlocked.CompareExchange(ref this._lockStatus, num + 1, num) != num);
            if (UnsafeNativeMethods.InstrumentedMutexGetLock(this._mutexHandle, -1) == -1)
            {
                Interlocked.Decrement(ref this._lockStatus);
                throw new InvalidOperationException(System.Web.SR.GetString("CompilationMutex_Failed"));
            }
        }

        private string MutexDebugName
        {
            get
            {
                return this._name;
            }
        }
    }
}

