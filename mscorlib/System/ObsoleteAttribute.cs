namespace System
{
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, Inherited=false)]
    public sealed class ObsoleteAttribute : Attribute
    {
        private bool _error;
        private string _message;

        public ObsoleteAttribute()
        {
            this._message = null;
            this._error = false;
        }

        public ObsoleteAttribute(string message)
        {
            this._message = message;
            this._error = false;
        }

        public ObsoleteAttribute(string message, bool error)
        {
            this._message = message;
            this._error = error;
        }

        public bool IsError
        {
            get
            {
                return this._error;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }
    }
}

