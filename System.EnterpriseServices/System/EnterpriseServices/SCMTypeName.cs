namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.Remoting;

    [Serializable]
    internal class SCMTypeName : IRemotingTypeInfo
    {
        private Type _serverType;
        private string _serverTypeName;

        internal SCMTypeName(Type serverType)
        {
            this._serverType = serverType;
            this._serverTypeName = serverType.AssemblyQualifiedName;
        }

        public virtual bool CanCastTo(Type castType, object o)
        {
            return castType.IsAssignableFrom(this._serverType);
        }

        public virtual string TypeName
        {
            get
            {
                return this._serverTypeName;
            }
            set
            {
                this._serverTypeName = value;
            }
        }
    }
}

