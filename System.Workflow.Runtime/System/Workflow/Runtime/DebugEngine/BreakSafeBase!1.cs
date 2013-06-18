namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class BreakSafeBase<T> where T: ICloneable, new()
    {
        private int controllerManagedThreadId;
        private object controllerUpdateObject;
        private volatile object currentData;
        private T nonEEDataClone;
        private volatile bool nonEEDataConsistent;
        private volatile bool nonEEIgnoreUpdate;
        private Mutex nonEELock;

        protected BreakSafeBase(int controllerManagedThreadId)
        {
            this.currentData = (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
            this.nonEEDataClone = default(T);
            this.nonEEDataConsistent = false;
            this.nonEEIgnoreUpdate = false;
            this.nonEELock = new Mutex(false);
            this.controllerManagedThreadId = controllerManagedThreadId;
        }

        protected object GetControllerUpdateObject()
        {
            return this.controllerUpdateObject;
        }

        protected T GetReaderData()
        {
            return (T) this.currentData;
        }

        protected T GetWriterData()
        {
            if (this.IsEECall)
            {
                if (this.nonEEDataConsistent && !this.nonEEIgnoreUpdate)
                {
                    return (T) this.currentData;
                }
                this.nonEEIgnoreUpdate = true;
                T local = (T) this.currentData;
                return (T) local.Clone();
            }
            this.nonEEIgnoreUpdate = false;
            T currentData = (T) this.currentData;
            return (T) currentData.Clone();
        }

        protected void Lock()
        {
            if (!this.IsEECall)
            {
                this.nonEELock.WaitOne();
            }
        }

        protected void SaveData(T data)
        {
            if (this.IsEECall)
            {
                this.currentData = data;
            }
            else
            {
                this.nonEEDataClone = data;
                this.nonEEDataConsistent = true;
                this.controllerUpdateObject = null;
                if (!this.nonEEIgnoreUpdate)
                {
                    this.currentData = data;
                }
                this.nonEEDataConsistent = false;
                this.nonEEDataClone = default(T);
            }
        }

        protected void SetControllerUpdateObject(object updateObject)
        {
            this.controllerUpdateObject = updateObject;
        }

        protected void Unlock()
        {
            if (!this.IsEECall)
            {
                this.nonEELock.ReleaseMutex();
            }
        }

        private bool IsEECall
        {
            get
            {
                return (Thread.CurrentThread.ManagedThreadId == this.controllerManagedThreadId);
            }
        }
    }
}

