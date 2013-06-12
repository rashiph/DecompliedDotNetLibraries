namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;

    internal abstract class Com2ExtendedBrowsingHandler
    {
        protected Com2ExtendedBrowsingHandler()
        {
        }

        public virtual void SetupPropertyHandlers(Com2PropertyDescriptor propDesc)
        {
            this.SetupPropertyHandlers(new Com2PropertyDescriptor[] { propDesc });
        }

        public abstract void SetupPropertyHandlers(Com2PropertyDescriptor[] propDesc);

        public abstract Type Interface { get; }
    }
}

