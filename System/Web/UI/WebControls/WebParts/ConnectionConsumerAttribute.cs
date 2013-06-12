namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web;

    [AttributeUsage(AttributeTargets.Method)]
    public class ConnectionConsumerAttribute : Attribute
    {
        private bool _allowsMultipleConnections;
        private Type _connectionPointType;
        private string _displayName;
        private string _id;

        public ConnectionConsumerAttribute(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException("displayName");
            }
            this._displayName = displayName;
            this._allowsMultipleConnections = false;
        }

        public ConnectionConsumerAttribute(string displayName, string id) : this(displayName)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            this._id = id;
        }

        public ConnectionConsumerAttribute(string displayName, Type connectionPointType) : this(displayName)
        {
            if (connectionPointType == null)
            {
                throw new ArgumentNullException("connectionPointType");
            }
            this._connectionPointType = connectionPointType;
        }

        public ConnectionConsumerAttribute(string displayName, string id, Type connectionPointType) : this(displayName, connectionPointType)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            this._id = id;
        }

        public bool AllowsMultipleConnections
        {
            get
            {
                return this._allowsMultipleConnections;
            }
            set
            {
                this._allowsMultipleConnections = value;
            }
        }

        public Type ConnectionPointType
        {
            get
            {
                if (!WebPartUtil.IsConnectionPointTypeValid(this._connectionPointType, true))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ConnectionConsumerAttribute_InvalidConnectionPointType", new object[] { this._connectionPointType.Name }));
                }
                return this._connectionPointType;
            }
        }

        public virtual string DisplayName
        {
            get
            {
                return this.DisplayNameValue;
            }
        }

        protected string DisplayNameValue
        {
            get
            {
                return this._displayName;
            }
            set
            {
                this._displayName = value;
            }
        }

        public string ID
        {
            get
            {
                if (this._id == null)
                {
                    return string.Empty;
                }
                return this._id;
            }
        }
    }
}

