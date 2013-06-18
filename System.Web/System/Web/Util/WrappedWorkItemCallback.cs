namespace System.Web.Util
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;

    internal class WrappedWorkItemCallback
    {
        private WorkItemCallback _originalCallback;
        private GCHandle _rootedThis;
        private WorkItemCallback _wrapperCallback;

        internal WrappedWorkItemCallback(WorkItemCallback callback)
        {
            this._originalCallback = callback;
            this._wrapperCallback = new WorkItemCallback(this.OnCallback);
        }

        private void OnCallback()
        {
            this._rootedThis.Free();
            this._originalCallback();
        }

        internal void Post()
        {
            this._rootedThis = GCHandle.Alloc(this);
            if (UnsafeNativeMethods.PostThreadPoolWorkItem(this._wrapperCallback) != 1)
            {
                this._rootedThis.Free();
                throw new HttpException(System.Web.SR.GetString("Cannot_post_workitem"));
            }
        }
    }
}

