namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    public abstract class ConnectionPoint
    {
        private bool _allowsMultipleConnections;
        private MethodInfo _callbackMethod;
        private Type _controlType;
        private string _displayName;
        private string _id;
        private Type _interfaceType;
        public static readonly string DefaultID = "default";
        internal const string DefaultIDInternal = "default";

        internal ConnectionPoint(MethodInfo callbackMethod, Type interfaceType, Type controlType, string displayName, string id, bool allowsMultipleConnections)
        {
            if (callbackMethod == null)
            {
                throw new ArgumentNullException("callbackMethod");
            }
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            if (controlType == null)
            {
                throw new ArgumentNullException("controlType");
            }
            if (!controlType.IsSubclassOf(typeof(Control)))
            {
                throw new ArgumentException(System.Web.SR.GetString("ConnectionPoint_InvalidControlType"), "controlType");
            }
            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException("displayName");
            }
            this._callbackMethod = callbackMethod;
            this._interfaceType = interfaceType;
            this._controlType = controlType;
            this._displayName = displayName;
            this._id = id;
            this._allowsMultipleConnections = allowsMultipleConnections;
        }

        public virtual bool GetEnabled(Control control)
        {
            return true;
        }

        public bool AllowsMultipleConnections
        {
            get
            {
                return this._allowsMultipleConnections;
            }
        }

        internal MethodInfo CallbackMethod
        {
            get
            {
                return this._callbackMethod;
            }
        }

        public Type ControlType
        {
            get
            {
                return this._controlType;
            }
        }

        public string DisplayName
        {
            get
            {
                return this._displayName;
            }
        }

        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(this._id))
                {
                    return DefaultID;
                }
                return this._id;
            }
        }

        public Type InterfaceType
        {
            get
            {
                return this._interfaceType;
            }
        }
    }
}

