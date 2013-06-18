namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;

    [Browsable(false)]
    internal abstract class BindBase
    {
        [NonSerialized]
        protected bool designMode = true;
        [NonSerialized]
        private object syncRoot = new object();

        protected BindBase()
        {
        }

        public abstract object GetRuntimeValue(Activity activity);
        public abstract object GetRuntimeValue(Activity activity, Type targetType);
        protected virtual void OnRuntimeInitialized(Activity activity)
        {
        }

        public abstract void SetRuntimeValue(Activity activity, object value);
    }
}

