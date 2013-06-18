namespace System.ServiceModel
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
    public sealed class MessagePropertyAttribute : Attribute
    {
        private bool isNameSetExplicit;
        private string name;

        internal bool IsNameSetExplicit
        {
            get
            {
                return this.isNameSetExplicit;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.isNameSetExplicit = true;
                this.name = value;
            }
        }
    }
}

