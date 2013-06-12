namespace System.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class DebuggerTypeProxyAttribute : Attribute
    {
        private Type target;
        private string targetName;
        private string typeName;

        public DebuggerTypeProxyAttribute(string typeName)
        {
            this.typeName = typeName;
        }

        public DebuggerTypeProxyAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.typeName = type.AssemblyQualifiedName;
        }

        public string ProxyTypeName
        {
            get
            {
                return this.typeName;
            }
        }

        public Type Target
        {
            get
            {
                return this.target;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.targetName = value.AssemblyQualifiedName;
                this.target = value;
            }
        }

        public string TargetTypeName
        {
            get
            {
                return this.targetName;
            }
            set
            {
                this.targetName = value;
            }
        }
    }
}

