namespace System.Web.UI.WebControls
{
    using System;

    public class ObjectDataSourceEventArgs : EventArgs
    {
        private object _objectInstance;

        public ObjectDataSourceEventArgs(object objectInstance)
        {
            this._objectInstance = objectInstance;
        }

        public object ObjectInstance
        {
            get
            {
                return this._objectInstance;
            }
            set
            {
                this._objectInstance = value;
            }
        }
    }
}

