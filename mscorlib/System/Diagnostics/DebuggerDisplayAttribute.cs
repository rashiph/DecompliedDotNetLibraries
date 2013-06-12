namespace System.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true), ComVisible(true)]
    public sealed class DebuggerDisplayAttribute : Attribute
    {
        private string name;
        private System.Type target;
        private string targetName;
        private string type;
        private string value;

        public DebuggerDisplayAttribute(string value)
        {
            if (value == null)
            {
                this.value = "";
            }
            else
            {
                this.value = value;
            }
            this.name = "";
            this.type = "";
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public System.Type Target
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

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
        }
    }
}

