namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class ObjectDataSourceDisposingEventArgs : CancelEventArgs
    {
        private object _objectInstance;

        public ObjectDataSourceDisposingEventArgs(object objectInstance)
        {
            this._objectInstance = objectInstance;
        }

        public object ObjectInstance
        {
            get
            {
                return this._objectInstance;
            }
        }
    }
}

