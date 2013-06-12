namespace Microsoft.SqlServer.Server
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false)]
    public sealed class SqlTriggerAttribute : Attribute
    {
        private string m_fEvent = null;
        private string m_fName = null;
        private string m_fTarget = null;

        public string Event
        {
            get
            {
                return this.m_fEvent;
            }
            set
            {
                this.m_fEvent = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_fName;
            }
            set
            {
                this.m_fName = value;
            }
        }

        public string Target
        {
            get
            {
                return this.m_fTarget;
            }
            set
            {
                this.m_fTarget = value;
            }
        }
    }
}

