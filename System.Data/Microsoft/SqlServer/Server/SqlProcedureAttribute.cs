namespace Microsoft.SqlServer.Server
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false)]
    public sealed class SqlProcedureAttribute : Attribute
    {
        private string m_fName = null;

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
    }
}

