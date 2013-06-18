namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class SharedProperty
    {
        private ISharedProperty _x;

        internal SharedProperty(ISharedProperty prop)
        {
            this._x = prop;
        }

        public object Value
        {
            get
            {
                return this._x.Value;
            }
            set
            {
                this._x.Value = value;
            }
        }
    }
}

