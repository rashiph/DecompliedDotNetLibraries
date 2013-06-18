namespace System.Data.Common
{
    using System;
    using System.Data;

    public abstract class DbDataSourceEnumerator
    {
        protected DbDataSourceEnumerator()
        {
        }

        public abstract DataTable GetDataSources();
    }
}

